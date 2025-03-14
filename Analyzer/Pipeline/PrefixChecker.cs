/******************************************************************************
* Filename    = PrefixChecker.cs
*
* Author      = Monesh Vanga 
* 
* Product     = Analyzer
* 
* Project     = Analyzer
*
* Description = An analyzer that checks the correctness of prefixes of interfaces and classes in parsed DLL files.
*****************************************************************************/

using System;
using Analyzer.Parsing;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer.Pipeline
{
    public class PrefixCheckerAnalyzer : AnalyzerBase
    {
        private string _errorMessage;
        private int _verdict;
        private readonly string _analyzerID;
       
        /// <summary>
        /// Initializes a new instance of the BaseAnalyzer with parsed DLL files.
        /// </summary>
        /// <param name="dllFiles">The parsed DLL files for analysis.</param>
        public PrefixCheckerAnalyzer(List<ParsedDLLFile> dllFiles) : base(dllFiles)
        {
            // The constructor can be used for any necessary setup or initialization.
            _errorMessage = "";
            _verdict = 1;
            _analyzerID = "115";
        }



        /// <summary>
        /// Analyzes the DLL files to check type name prefixes for correctness.
        /// </summary>
        /// <returns>The number of errors found during the analysis.</returns>
        protected override AnalyzerResult AnalyzeSingleDLL(ParsedDLLFile parsedDLLFile)
        {
            _errorMessage = "";
            _verdict = 1;
            int errorCount = 0;
            int flag = 0;
            int flag1 = 0;

            //Checks the prefixes of classes
            foreach (ParsedClass classObj in parsedDLLFile.classObjList)
            {
                if (!IsCorrectTypeName(classObj.Name))
                {
                    flag++;
                    Console.WriteLine($"Incorrect Class Prefix : {classObj.Name}");

                    //if it is the first mistake
                    if(flag == 1)
                    {
                        _errorMessage += "Incorrect Class Prefix : " + classObj.Name + " ";
                    }

                    //if it not the first mistake
                    else
                    {
                        _errorMessage += ", " + classObj.Name + " ";
                    }
                    errorCount++;
                }
            }

            // To check interfaces prefixes
            foreach (ParsedInterface interfaceObj in parsedDLLFile.interfaceObjList)
            {
                if (!IsCorrectInterfaceName(interfaceObj.Name))
                {
                    flag1++;
                    Console.WriteLine($"Incorrect Interface Prefix : {interfaceObj.Name}");

                    //if it is the first mistake
                    if(flag1 == 1)
                    {
                        _errorMessage += "Incorrect Interface Prefix : " + interfaceObj.Name + " ";
                    }

                    //if it is not the first mistake
                    else
                    {
                        _errorMessage += ", " + interfaceObj.Name + " ";
                    }
                    errorCount++;
                }
            }

            //if errors are found
            if (errorCount == 0)
            {
                _errorMessage = "No Violation Found";
                _verdict = 1;
            }

            //if errors are not found
            else
            {
                _verdict = 0;
            }

            return new AnalyzerResult(_analyzerID, _verdict, _errorMessage);
        }


        /// <summary>
        /// Checks if a type name follows the correct interface prefix.
        /// </summary>
        /// <param name="name">The type name to check.</param>
        /// <returns>True if the type name has the correct interface prefix, otherwise false.</returns>
        private bool IsCorrectInterfaceName(string name)
        {
            return name.Length > 2 && name[0] == 'I' && char.IsUpper(name[1]);
        }

        /// <summary>
        /// Checks if a type name follows the correct type prefix.
        /// </summary>
        /// <param name="name">The type name to check.</param>
        /// <returns>True if the type name has the correct type prefix, otherwise false.</returns>
        private bool IsCorrectTypeName(string name)
        {
            if (name.Length < 3)
            {
                return true;
            }

            if (name[0] == 'I')
            {
                if (char.IsLower( name[1] ))
                {
                    return true;
                }
                else
                {
                    return char.IsUpper( name[2] );
                }
            }
            else
            {
                return true;
            }
        }
    }
}
