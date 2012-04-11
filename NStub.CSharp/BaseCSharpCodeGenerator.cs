﻿using System;
using System.Collections.Generic;
using System.Text;
using System.CodeDom;
using System.IO;
using System.Linq;
using NStub.Core;
using Microsoft.CSharp;
using System.CodeDom.Compiler;

namespace NStub.CSharp
{
    public abstract class BaseCSharpCodeGenerator
    {

        private CodeNamespace _codeNamespace;
        private string _outputDirectory;

        /// <summary>
        /// Gets or sets the <see cref="System.CodeDom.CodeNamespace"/> object 
        /// the generator is currently working from.
        /// </summary>
        /// <value>The <see cref="System.CodeDom.CodeNamespace"/> object the 
        /// generator is currently working from.</value>
        public CodeNamespace CodeNamespace
        {
            get
            {
                return this._codeNamespace;
            }
            set
            {
                this._codeNamespace = value;
            }
        }

        /// <summary>
        /// Gets or sets the directory the new sources files will be output to.
        /// </summary>
        /// <value>The directory the new source files will be output to.</value>
        public string OutputDirectory
        {
            get
            {
                return this._outputDirectory;
            }

            set
            {
                this._outputDirectory = value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CSharpCodeGenerator"/> class
        /// based the given CodeNamespace which will output to the given directory.
        /// </summary>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="outputDirectory">The output directory.</param>
        /// <exception cref="System.ArgumentNullException">codeNamepsace or
        /// outputDirectory is null.</exception>
        /// <exception cref="System.ArgumentException">outputDirectory is an
        /// empty string.</exception>
        /// <exception cref="DirectoryNotFoundException">outputDirectory
        /// cannot be found.</exception>
        protected BaseCSharpCodeGenerator(CodeNamespace codeNamespace,
            string outputDirectory)
        {
            #region Validation

            // Null arguments will not be accepted
            if (codeNamespace == null)
            {
                throw new ArgumentNullException("codeNamespace",
                    Exceptions.ParameterCannotBeNull);
            }
            if (outputDirectory == null)
            {
                throw new ArgumentNullException("outputDirectory",
                    Exceptions.ParameterCannotBeNull);
            }
            // Ensure that the output directory is not empty
            if (outputDirectory.Length == 0)
            {
                throw new ArgumentException(Exceptions.StringCannotBeEmpty,
                    "outputDirectory");
            }
            // Ensure that the output directory is valid
            if (!(Directory.Exists(outputDirectory)))
            {
                throw new DirectoryNotFoundException(Exceptions.DirectoryCannotBeFound);
            }

            #endregion Validation

            _codeNamespace = codeNamespace;
            _outputDirectory = outputDirectory;
        }

        /// <summary>
        /// This methods actually performs the code generation for the
        /// file current <see cref="System.CodeDom.CodeNamespace">CodeNamespace</see>. 
        /// All classes within the namespace will have exactly one file generated for them.  
        /// </summary>
        public void GenerateCode()
        {
            // We want to write a separate file for each type
            foreach (CodeTypeDeclaration codeTypeDeclaration in CodeNamespace.Types)
            {
                // Create a namespace for the Type in order to put it in scope
                CodeNamespace codeNamespace =
                    new CodeNamespace((CodeNamespace.Name));

                // add using imports.
                codeNamespace.Imports.AddRange(RetrieveNamespaceImports().ToArray());
                var indexcodeNs = codeTypeDeclaration.Name.LastIndexOf('.');
                if (indexcodeNs > 0)
                {
                    // try to import the namespace for the object under test.
                    var codeNs = codeTypeDeclaration.Name.Substring(0, indexcodeNs);
                    codeNamespace.Imports.Add(new CodeNamespaceImport(codeNs));
                }

                // Clean the type name
                codeTypeDeclaration.Name =
                    Utility.ScrubPathOfIllegalCharacters(codeTypeDeclaration.Name);

                var codeTypeDeclarationName = Utility.GetUnqualifiedTypeName(codeTypeDeclaration.Name);
                // Create our test type
                codeTypeDeclaration.Name = codeTypeDeclarationName + "Test";
                codeTypeDeclaration.IsPartial = true;
                
                // Add testObject field
                var testObjectMemberField = AddTestObjectField(codeTypeDeclaration, codeTypeDeclarationName);

                // Give it a default public constructor
                var codeConstructor = new CodeConstructor();
                codeConstructor.Attributes = MemberAttributes.Public;
                codeTypeDeclaration.Members.Add(codeConstructor);

                // Set out member names correctly
                foreach (CodeTypeMember typeMember in codeTypeDeclaration.Members)
                {
                    GenerateCodeTypeMember(typeMember);
                }

                // Setup and TearDown
                GenerateSetupAndTearDown(codeTypeDeclaration, codeTypeDeclarationName, testObjectMemberField);

                // Add test class to the CodeNamespace.
                codeNamespace.Types.Add(codeTypeDeclaration);

                RemoveDuplicatedMembers(codeTypeDeclaration);
                SortMembers(codeTypeDeclaration);
                WriteClassFile(codeTypeDeclaration.Name, codeNamespace);
            }
        }

        private void GenerateSetupAndTearDown(CodeTypeDeclaration codeTypeDeclaration, string codeTypeDeclarationName, CodeMemberField testObjectMemberField)
        {
            var setUpMethod = CreateCustomCodeMemberMethodWithSameNameAsAttribute("Setup");
            ComposeTestSetupMethod(setUpMethod, testObjectMemberField, codeTypeDeclarationName);
            //ComposeTestSetupMockery(codeTypeDeclaration, setUpMethod, testObjectMemberField, codeTypeDeclarationName);
            codeTypeDeclaration.Members.Add(setUpMethod);
            var tearDownMethod = CreateCustomCodeMemberMethodWithSameNameAsAttribute("TearDown");
            ComposeTestTearDownMethod(tearDownMethod, testObjectMemberField, codeTypeDeclarationName);
            codeTypeDeclaration.Members.Add(tearDownMethod);
        }

        protected virtual void ComposeTestSetupMethod(CodeMemberMethod setUpMethod, CodeMemberField testObjectMemberField, string codeTypeDeclarationName)
        { }
        protected virtual void ComposeTestTearDownMethod(CodeMemberMethod teardownMethod, CodeMemberField testObjectMemberField, string codeTypeDeclarationName)
        { }
        private static CodeMemberField AddTestObjectField(CodeTypeDeclaration codeTypeDeclaration, string codeTypeDeclarationName)
        {
            var memberField = new CodeMemberField(
                codeTypeDeclarationName, "testObject");
            memberField.Attributes = MemberAttributes.Private;
            //typeMember.Statements.Add(variableDeclaration);
            codeTypeDeclaration.Members.Add(memberField);
            return memberField;
        }

        /// <summary>
        /// Processes the code type members of the compilation unit.
        /// </summary>
        /// <param name="typeMember">The type member to process.</param>
        private void GenerateCodeTypeMember(CodeTypeMember typeMember)
        {
            var typeMemberName = typeMember.Name;
            if (typeMember is CodeMemberMethod)
            {
                // We don't generate default constructors
                if (!(typeMember is CodeConstructor))
                {
                    if (typeMember is CodeMemberProperty)
                    {
                        // before stub has created.
                        //PreComputeCodeMemberProperty(typeMember);
                    }

                    CreateStubForCodeMemberMethod(typeMember as CodeMemberMethod);

                    if (typeMember is CodeMemberMethod && (typeMember.Name.Contains("get_") || typeMember.Name.Contains("set_")))
                    {
                        var propertyName = typeMemberName.Replace("get_", "").Replace("set_", "");
                        // hmm Generate to generate new and compute to process existing !?!
                        ComputeCodeMemberProperty(typeMember as CodeMemberMethod, propertyName);
                    }
                    else if (typeMember is CodeMemberMethod && (typeMember.Name.Contains("add_") || typeMember.Name.Contains("remove_")))
                    {
                        var eventName = typeMemberName.Replace("add_", "").Replace("remove_", "");
                        // hmm Generate to generate new and compute to process existing !?!
                        ComputeCodeMemberEvent(typeMember as CodeMemberMethod, eventName);
                    }
                }
            }
        }

        /// <summary>
        /// Add namespace imports to the main compilation unit.
        /// </summary>
        /// <returns>A list of code name spaces, to be added to the compilation unit.</returns>
        protected abstract IEnumerable<CodeNamespaceImport> RetrieveNamespaceImports();

        /// <summary>
        /// Handle property related stuff before type generation.
        /// </summary>
        /// <param name="typeMember">The type member.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected abstract void ComputeCodeMemberProperty(CodeMemberMethod typeMember, string propertyName);

        /// <summary>
        /// Handle event related stuff before type generation.
        /// </summary>
        /// <param name="typeMember">The type member.</param>
        /// <param name="eventName">Name of the event.</param>
        protected abstract void ComputeCodeMemberEvent(CodeMemberMethod typeMember, string eventName);

        //protected abstract void CreateStubForCodeMemberMethod(CodeMemberMethod codeMemberMethod);

        /// <summary>
        /// Creates the stub for the code member method.  This method actually
        /// implements the method body for the test method.
        /// </summary>
        /// <param name="codeMemberMethod">The code member method.</param>
        protected virtual void CreateStubForCodeMemberMethod(CodeMemberMethod codeMemberMethod)
        {
            // Clean the member name and append 'Test' to the end of it
            codeMemberMethod.Name = Utility.ScrubPathOfIllegalCharacters(codeMemberMethod.Name);
            codeMemberMethod.Name = codeMemberMethod.Name + "Test";

            // Standard test methods accept no parameters and return void.
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
            codeMemberMethod.Parameters.Clear();

            //var testAttr = new CodeAttributeDeclaration(
            //            new CodeTypeReference(typeof(TestAttribute).Name));
            var testAttr = new CodeAttributeDeclaration(new CodeTypeReference("Test"));

            codeMemberMethod.CustomAttributes.Add(testAttr);
            //codeMemberMethod.CustomAttributes.Add(
            //		new CodeAttributeDeclaration(
            //		new CodeTypeReference(typeof(IgnoreAttribute))));
            codeMemberMethod.Statements.Add(
                new CodeCommentStatement("TODO: Implement unit test for " +
                                         codeMemberMethod.Name));
        }

        private class CustomCodeMemberMethod : CodeMemberMethod
        { 
        }

        /// <summary>
        /// Creates a custom member method with an attribute of the same name.
        /// </summary>
        /// <param name="methodName">Name of the method and attribute.</param>
        /// <returns>The new code member method with the specified method.</returns>
        private CodeMemberMethod CreateCustomCodeMemberMethodWithSameNameAsAttribute(string methodName)
        {
            var codeMemberMethod = new CustomCodeMemberMethod();
            codeMemberMethod.Attributes = MemberAttributes.Public;

            // Clean the member name and append 'Test' to the end of it
            codeMemberMethod.Name = methodName;

            // Standard test methods accept no parameters and return void.
            codeMemberMethod.ReturnType = new CodeTypeReference(typeof(void));
            codeMemberMethod.Parameters.Clear();

            var setupAttr = new CodeAttributeDeclaration(new CodeTypeReference(methodName));

            codeMemberMethod.CustomAttributes.Add(setupAttr);
            codeMemberMethod.Statements.Add(
                new CodeCommentStatement("ToDo: Implement " + methodName + " logic here "));
            return codeMemberMethod;
        }

        private static void SortMembers(CodeTypeDeclaration codeTypeDeclaration)
        {
            var members = codeTypeDeclaration.Members.OfType<CodeTypeMember>();
            //var ordered = members.OrderBy(e => e, new CodeTypeDeclarationComparer());
            //var grp = members.OrderBy(e => e, new CodeTypeDeclarationComparer()).GroupBy(e => e.GetType().FullName);
            var grp = members.GroupBy(e => e.GetType().FullName).Reverse();
            //var result = ordered.ToArray();
            //foreach (var item in grp)
            {
                //item.Select(e=>e.
            }
            var result = grp.SelectMany(
                (e, k) => e.Select(s => s).OrderBy(o => o, new CodeTypeDeclarationComparer())).ToArray();
            codeTypeDeclaration.Members.Clear();
            codeTypeDeclaration.Members.AddRange(result);
        }

        private class CodeTypeDeclarationComparer : IComparer<CodeTypeMember>
        {
            public int Compare(CodeTypeMember x, CodeTypeMember y)
            {
                int diff = 0;
                CompareNameStart(-2, "Event", x, y, ref diff);
                CompareNameStart(-1, "Property", x, y, ref diff);

                CompareForName(12, "EqualsTest", x, y, ref diff);
                CompareForName(13, "GetHashCodeTest", x, y, ref diff);
                CompareForName(14, "GetTypeTest", x, y, ref diff);
                CompareForName(15, "ToStringTest", x, y, ref diff);


                return x.Name.CompareTo(y.Name) + diff;
                /*if (y.Name == "Setup" && !(x.Name == "Setup"))
                {
                    return x.GetType().FullName.CompareTo("!!" + y.GetType().FullName);
                    //return int.MinValue;
                }
                else if (y.Name == "TearDown" && !(x.Name == "TearDown"))
                {
                    return x.GetType().FullName.CompareTo("!" + y.GetType().FullName);
                    //return int.MinValue + 1;
                }
                return (x.GetType().FullName.CompareTo(y.GetType().FullName) * 16) - (x.Name.CompareTo(y.Name));
                return x.Name.CompareTo(y.Name);*/
            }

            private static void CompareForName(int rank, string name, CodeTypeMember x, CodeTypeMember y, ref int diff)
            {
                if (x.Name == name)
                {
                    diff += rank;
                }
                if (y.Name == name)
                {
                    diff -= rank;
                }
            }
            
            private static void CompareNamePart(int rank, string name, CodeTypeMember x, CodeTypeMember y, ref int diff)
            {
                if (x.Name.Contains(name))
                {
                    diff += rank;
                }
                if (y.Name.Contains(name))
                {
                    diff -= rank;
                }
            }

            private static void CompareNameStart(int rank, string name, CodeTypeMember x, CodeTypeMember y, ref int diff)
            {
                if (x.Name.StartsWith(name))
                {
                    diff += rank;
                }
                if (y.Name.StartsWith(name))
                {
                    diff -= rank;
                }
            }

        }
        /// <summary>
        /// Since types can contain multiple overloads of the same method, once
        /// we remove the parameters from every method our type may have the 
        /// many duplicates of the same method.  This method removes those
        /// duplicates.
        /// </summary>
        /// <param name="codeTypeDeclaration">The <see cref="CodeTypeDeclaration"/>
        /// from which to remove the duplicates.</param>
        private static void RemoveDuplicatedMembers(CodeTypeDeclaration codeTypeDeclaration)
        {
            for (int i = 0; i < codeTypeDeclaration.Members.Count; ++i)
            {
                int occurrences = 0;
                for (int j = 0; j < codeTypeDeclaration.Members.Count; ++j)
                {
                    // Compare each CodeTypeMember to all other CodeTypeMembers
                    // in its type.  If more than one match is found (the 
                    // codeTypeMember matching itself) then we remove that match.
                    if (codeTypeDeclaration.Members[i].Name.Equals(
                        codeTypeDeclaration.Members[j].Name))
                    {
                        occurrences++;
                        if (occurrences > 1)
                        {
                            codeTypeDeclaration.Members.Remove(codeTypeDeclaration.Members[j]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Writes the class file.  This method actually creates the physical
        /// class file and populates it accordingly.
        /// </summary>
        /// <param name="className">Name of the class file to be written.</param>
        /// <param name="codeNamespace">The CodeNamespace which represents the
        /// file to be written.</param>
        private void WriteClassFile(string className, CodeNamespace codeNamespace)
        {
            CSharpCodeProvider cSharpCodeProvider = new CSharpCodeProvider();
            string sourceFile = OutputDirectory + Path.DirectorySeparatorChar +
                className + "." + cSharpCodeProvider.FileExtension;
            sourceFile = Utility.ScrubPathOfIllegalCharacters(sourceFile);
            IndentedTextWriter indentedTextWriter =
                new IndentedTextWriter(new StreamWriter(sourceFile, false), "  ");
            CodeGeneratorOptions codeGenerationOptions = new CodeGeneratorOptions();
            codeGenerationOptions.BracingStyle = "C";
            cSharpCodeProvider.GenerateCodeFromNamespace(codeNamespace, indentedTextWriter,
                codeGenerationOptions);
            indentedTextWriter.Flush();
            indentedTextWriter.Close();
        }

        /// <summary>
        /// Replaces the ending name of the test ("Test") with a specified string.
        /// </summary>
        /// <param name="typeMember">The type member that name gets modified.</param>
        /// <param name="replacement">The replacement string.</param>
        protected static void ReplaceTestInTestName(CodeTypeMember typeMember, string replacement)
        {
            typeMember.Name = typeMember.Name.Replace("Test", replacement);
        }


    }
}
