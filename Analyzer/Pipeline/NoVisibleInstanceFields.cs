﻿/******************************************************************************
* Filename    = NoVisibleInstanceFields.cs
* 
* Author      = Sneha Bhattacharjee
*
* Product     = Analyzer
* 
* Project     = Analyzer
*
* Description = Class should not have non-private instance field. 
*               The primary use of a field should be as an implementation detail. 
*               Fields should be private or internal and should be exposed by using properties. 
*****************************************************************************/

using Analyzer.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Mono.Cecil.Cil;
using Mono.Cecil;
using Analyzer.Pipeline;
using Analyzer;
using Mono.Cecil.Rocks;
using System.Diagnostics;

namespace Analyzer.Pipeline
{
    public class NoVisibleInstanceFields : AnalyzerBase
    {
        private string _errorMessage;
        private int _verdict;
        private readonly string _analyzerID;

        public NoVisibleInstanceFields(List<ParsedDLLFile> dllFiles) : base(dllFiles)
        {
            _errorMessage = "";
            _verdict = 1;
            _analyzerID = "118";
        }

        private List<FieldDefinition> FindVisibleNativeFields(ParsedDLLFile parsedDLLFile)
        {
            List<FieldDefinition> visibleNativeFieldsList = new();

            foreach (ParsedClassMonoCecil classobj in parsedDLLFile.classObjListMC)
            {
                TypeDefinition classtype = classobj.TypeObj;

                /*
                // rule does not apply to interface, enumerations and delegates or to types without fields
                if (classtype.IsValueType)
                {
                    // todo
                    // for now implementing just for classes
                    // classes and namespaces can have structures
                    // structures are recommended to be immutable too
                    // otherwise they should also have getter and setter
                    // note: parseddllfiles.classobjlistmc provides only classes
                    continue;
                }
                */

                // By default, this rule only looks at externally visible types
                if (!classtype.IsPublic)
                {
                    continue;
                }

                foreach (FieldDefinition field in classtype.Fields)
                {
                    // IsFamilyOrAssembly for protected internal
                    // IsFamily           for protected
                    // IsAssembly         for internal
                    if (field.IsPrivate || (field.IsAssembly && !field.IsFamilyOrAssembly))
                    {
                        continue;
                    }
                    else
                    {
                        if (field.IsPublic && field.IsInitOnly)
                        {
                            continue;
                        }
                        else
                        {
                            visibleNativeFieldsList.Add(field);
                        }
                    }
                }
            }
            return visibleNativeFieldsList;
        }

        private string ErrorMessage(List<FieldDefinition> visibleNativeFieldsList)
        {
            var errorLog = new System.Text.StringBuilder("The following native fields are visible:");

            foreach (FieldDefinition field in visibleNativeFieldsList)
            {
                errorLog.AppendLine( field.FullName );

            }
            return errorLog.ToString();
        }

        protected override AnalyzerResult AnalyzeSingleDLL(ParsedDLLFile parsedDLLFile)
        {
            List<FieldDefinition> visibleNativeFieldsList = FindVisibleNativeFields(parsedDLLFile);
            try
            {
                if (visibleNativeFieldsList.Count > 0)
                {
                    _verdict = 0;
                    _errorMessage = ErrorMessage(visibleNativeFieldsList);
                }
                else
                {
                    _errorMessage = "No violation found.";
                }
            }
            catch (NullReferenceException ex)
            {
                throw new NullReferenceException( "Encountered exception while processing." , ex );
            }

            return new AnalyzerResult(_analyzerID, _verdict, _errorMessage);
        }
    }
}
