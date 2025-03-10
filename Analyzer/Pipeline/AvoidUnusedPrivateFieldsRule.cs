﻿/******************************************************************************
* Filename    = AvoidUnusedPrivateFieldsRule.cs
*
* Author      = Mangesh Dalvi
* 
* Roll No     = 112001010
*
* Product     = Code Inspector
* 
* Project     = Analyzer
*
* Description = Unused private fields in a class in C# should be avoided to enhance code clarity and maintainability, minimizing unnecessary complexity.
******************************************************************************/

using Analyzer.Parsing;
using Mono.Cecil.Cil;
using Mono.Cecil;

namespace Analyzer.Pipeline
{
    /// <summary>
    /// This class represents an analyzer rule for detecting unused private fields in a DLL.
    /// </summary>
    public class AvoidUnusedPrivateFieldsRule : AnalyzerBase
    {

        private string _errorMessage;
        private int _verdict;
        private readonly string _analyzerID;


        /// <summary>
        /// Initializes a new instance of the AvoidUnusedPrivateFieldsRule class.
        /// </summary>
        /// <param name="dllFiles">The ParsedDLLFiles object containing the parsed DLL information.</param>
        public AvoidUnusedPrivateFieldsRule(List<ParsedDLLFile> dllFiles) : base(dllFiles)
        {
            _errorMessage = "";
            _verdict = 1;
            _analyzerID = "103";
        }

        /// <summary>
        /// Handles the analysis of a ParsedClassMonoCecil instance to find unused fields.
        /// </summary>
        /// <param name="cls"></param>
        /// <returns></returns>
        public List<string> HandleClass(ParsedClassMonoCecil cls)
        {
            List<string> unusedFields = new(cls.FieldsList.Select( field => field.Name.ToString()));

            foreach (MethodDefinition method in cls.MethodsList.Concat( cls.Constructors ))
            {
                if (!method.HasBody)
                {
                    continue;
                }

                foreach (Instruction? ins in method.Body.Instructions)
                {
                    // Check for instructions related to field access or loading
                    if (ins.OpCode == OpCodes.Ldfld || ins.OpCode == OpCodes.Ldsfld || ins.OpCode == OpCodes.Ldflda || ins.OpCode == OpCodes.Ldsflda || ins.OpCode == OpCodes.Ldloc || ins.OpCode == OpCodes.Ldloca)
                    {
                        FieldReference fieldReference = (FieldReference)ins.Operand;

                        string fieldName = fieldReference.Name.ToString();

                        if (unusedFields.Contains( fieldName ))
                        {
                            unusedFields.Remove( fieldName );
                        }
                    }
                }
            }

            return unusedFields;
        }

        /// <summary>
        /// Checks for unused private fields in the parsed DLL.
        /// </summary>
        /// <returns>True if no unused private fields are found; otherwise, false.</returns>
        private void Check(ParsedDLLFile parsedDLLFile)
        {
            foreach (ParsedClassMonoCecil cls in parsedDLLFile.classObjListMC)
            {
                List<string> unusedFields = HandleClass(cls);

                if(unusedFields.Count > 0)
                {
                    _verdict = 0;

                    _errorMessage += cls.Name + " : ";

                    foreach(string field in unusedFields)
                    {
                        _errorMessage += field + " ";
                    }

                    _errorMessage += ",";  
                }

            }

            _ = _verdict == 0 ? _errorMessage += " are unused private field." : _errorMessage = "No violation found.";
        }

        /// <summary>
        /// Constructs the AnalyzerResult object based on the analysis.
        /// </summary>
        /// <param name="parsedDLLFile"></param>
        protected override AnalyzerResult AnalyzeSingleDLL(ParsedDLLFile parsedDLLFile)
        {
            _errorMessage = "";
            _verdict = 1;

            Check(parsedDLLFile);
            return new AnalyzerResult(_analyzerID, _verdict, _errorMessage);
        }
    }
}
