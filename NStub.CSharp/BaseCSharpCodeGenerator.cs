﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BaseCSharpCodeGenerator.cs" company="EvePanix">
//   Copyright (c) Jedzia 2001-2012, EvePanix. All rights reserved.
//   See the license notes shipped with this source and the GNU GPL.
// </copyright>
// <author>Jedzia</author>
// <email>jed69@gmx.de</email>
// <date>$date$</date>
// --------------------------------------------------------------------------------------------------------------------

namespace NStub.CSharp
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.CSharp;
    using NStub.Core;
    using NStub.CSharp.BuildContext;
    using NStub.CSharp.ObjectGeneration;
    using NStub.CSharp.ObjectGeneration.Builders;

    /// <summary>
    /// The <see cref="BaseCSharpCodeGenerator"/> is responsible for the generation of the individual
    /// class files with <see cref="MbUnit"/> support which will make up the actual test project.  For information
    /// regarding the generation of the project file, see 
    /// <see cref="CSharpProjectGenerator"></see>.
    /// </summary>
    public abstract class BaseCSharpCodeGenerator
    {
        #region Fields

        private static readonly DuplicatedMemberComparer DuplicatedMemberComparerInstance =
            new DuplicatedMemberComparer();

        private readonly IBuildSystem buildSystem;
        private readonly IMemberBuilderFactory testBuilders;
        private BuildDataDictionary buildProps;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseCSharpCodeGenerator"/> class
        /// based the given <see cref="CodeNamespace"/> which will output to the given directory.
        /// </summary>
        /// <param name="buildSystem">The build system.</param>
        /// <param name="codeNamespace">The code namespace.</param>
        /// <param name="testBuilders">The test builder repository. If <c>null</c>, is substituted with <see cref="MemberBuilderFactory.Default"/></param>
        /// <param name="configuration">The configuration of the generator.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="codeNamespace"/> or
        /// <cref name="ICodeGeneratorParameters.OutputDirectory"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException"><cref name="ICodeGeneratorParameters.OutputDirectory"/> is an
        /// empty string.</exception>
        /// <exception cref="ApplicationException"><cref name="ICodeGeneratorParameters.OutputDirectory"/>
        /// cannot be found.</exception>
        protected BaseCSharpCodeGenerator(
            IBuildSystem buildSystem,
            CodeNamespace codeNamespace,
            IMemberBuilderFactory testBuilders,
            ICodeGeneratorParameters configuration)
        {
            Guard.NotNull(() => buildSystem, buildSystem);
            this.buildSystem = buildSystem;
            Guard.NotNull(() => configuration, configuration);
            this.Configuration = configuration;

            string outputDirectory = configuration.OutputDirectory;

            // Null arguments will not be accepted
            if (codeNamespace == null)
            {
                throw new ArgumentNullException(
                    "codeNamespace",
                    Exceptions.ParameterCannotBeNull);
            }

            // Null arguments will not be accepted
            if (testBuilders == null)
            {
                testBuilders = MemberBuilderFactory.Default;

                // throw new ArgumentNullException(
                // "testBuilders",
                // Exceptions.ParameterCannotBeNull);
            }

            if (outputDirectory == null)
            {
                throw new ArgumentNullException(
                    "configuration",
                    Exceptions.ParameterCannotBeNull);
            }

            // Ensure that the output directory is not empty
            if (outputDirectory.Length == 0)
            {
                throw new ArgumentException(
                    Exceptions.StringCannotBeEmpty,
                    "configuration");
            }

            // Ensure that the output directory is valid
            if (!this.buildSystem.DirectoryExists(outputDirectory))
            {
                throw new ApplicationException(Exceptions.DirectoryCannotBeFound);
            }

            this.CodeNamespace = codeNamespace;
            this.OutputDirectory = outputDirectory;
            this.testBuilders = testBuilders;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the build properties.
        /// </summary>
        /// <value>
        /// The build properties.
        /// </value>
        /// <exception cref="InvalidOperationException">Cannot set the BuildProperties twice or after access to it.</exception>
        public BuildDataDictionary BuildProperties
        {
            get
            {
                if (this.buildProps == null)
                {
                    this.buildProps = new BuildDataDictionary(); // InitializeBuildProperties();
                    InitializeBuildProperties(this.BuildProperties);
                }

                return this.buildProps;
            }

            set
            {
                Guard.NotNull(() => value, value);
                if (this.buildProps != null)
                {
                    throw new InvalidOperationException(
                        "Cannot set the BuildProperties twice or after access to it.");
                }
                // Todo maybe add the props here ?
                this.buildProps = value;
                InitializeBuildProperties(this.buildProps);
            }
        }

        /// <summary>
        /// Initializes the build properties.
        /// </summary>
        /// <returns>A ready set of build properties.</returns>
        protected virtual void InitializeBuildProperties(BuildDataDictionary props)
        {
            props.AddGeneralString(
                BuilderConstants.PropertyBaseClassOfINotifyPropertyChangedTest,
                GeneratorConstants.BaseClassOfINotifyPropertyChangedTest);

            //props.AddDataItem(
            //    BuilderConstants.PropertyGeneralCategory,
            //    BuilderConstants.PropertyBaseClassOfINotifyPropertyChangedTest,
           //     new StringConstantBuildParameter(GeneratorConstants.BaseClassOfINotifyPropertyChangedTest)
            //    , false);
        }

        /// <summary>
        /// Gets or sets the <see cref="System.CodeDom.CodeNamespace"/> object 
        /// the generator is currently working from.
        /// </summary>
        /// <value>The <see cref="System.CodeDom.CodeNamespace"/> object the 
        /// generator is currently working from.</value>
        public CodeNamespace CodeNamespace { get; set; }

        /// <summary>
        /// Gets the configuration of the code generator.
        /// </summary>
        public ICodeGeneratorParameters Configuration { get; private set; }

        /// <summary>
        /// Gets or sets the directory the new sources files will be output to.
        /// </summary>
        /// <value>The directory the new source files will be output to.</value>
        public string OutputDirectory { get; set; }

        /// <summary>
        /// Gets the test builder repository.
        /// </summary>
        /// <value>
        /// The test builders.
        /// </value>
        /// <exception cref="InvalidOperationException">The TestBuilders can only set once.</exception>
        public IMemberBuilderFactory TestBuilders
        {
            get
            {
                /*if (this.testBuilders == null)
                {
                    this.testBuilders = new TestBuilderFactory(
                        new PropertyBuilder(), new EventBuilder(), new MethodBuilder());
                }*/
                return this.testBuilders;
            }

            /*set
                                    {
                                        if (this.testBuilders != null)
                                        {
                                            throw new InvalidOperationException("The TestBuilders can only set once.");
                                        }

                                        this.testBuilders = value;
                                    }*/
        }

        /*public IBuildSystem buildSystem
        {
            get
            {
                if (this.buildSystem == null)
                {
                    this.buildSystem = new StandardBuildSystem();
                }

                return this.buildSystem;
            }

            set
            {
                if (this.buildSystem != null)
                {
                    throw new InvalidOperationException("The BuildSystem can only be setup once. This should happen early after instantiation.");
                }

                this.buildSystem = value;
            }
        }*/

        /// <summary>
        /// Gets the current test class declaration. The value is only valid during execution of 
        /// the <see cref="GenerateCode"/> method.
        /// </summary>
        protected CodeTypeDeclaration CurrentTestClassDeclaration { get; private set; }

        #endregion

        /// <summary>
        /// Add a member field to the test method.
        /// </summary>
        /// <param name="codeTypeDeclaration">The code type declaration of the test class.</param>
        /// <param name="testObjectType">Type of the object under test.</param>
        /// <param name="testObjectName">The name of the object under test.</param>
        /// <returns>A reference to the created member field of the test.</returns>
        public static CodeMemberField AddTestMemberField(
            CodeTypeDeclaration codeTypeDeclaration,
            string testObjectType,
            string testObjectName)
        {
            var memberField = CreateMemberField(testObjectType, testObjectName);
            codeTypeDeclaration.Members.Add(memberField);

            return memberField;
        }

        /// <summary>
        /// Create a private member field declaration.
        /// </summary>
        /// <param name="memberFieldType">Type of the field.</param>
        /// <param name="memberFieldName">The name of the field.</param>
        /// <returns>
        /// A new member field declaration.
        /// </returns>
        public static CodeMemberField CreateMemberField(string memberFieldType, string memberFieldName)
        {
            var memberField = new CodeMemberField(
                memberFieldType, memberFieldName)
                                  {
                                      Attributes = MemberAttributes.Private
                                  };
            return memberField;
        }

        /// <summary>
        /// Replaces the ending name of the test ("Test") with a specified string.
        /// </summary>
        /// <param name="typeMember">The type member that name gets modified.</param>
        /// <param name="replacement">The replacement string.</param>
        public static void ReplaceTestInTestName(CodeTypeMember typeMember, string replacement)
        {
            typeMember.Name = typeMember.Name.Replace("Test", replacement);
        }

        /// <summary>
        /// This methods actually performs the code generation for the
        /// file current <see cref="System.CodeDom.CodeNamespace"><see cref="CodeNamespace"/></see>. 
        /// All classes within the namespace will have exactly one file generated for them.  
        /// </summary>
        public void GenerateCode()
        {
            // var asd = this.CodeNamespace.Types.OfType<CodeTypeDeclaration>().Any(e=>e.Name =="ddepp");
            // We want to write a separate file for each type
            // this.BuildProperties.AddDataItem(
            var nd = new NamespaceDetector(this.CodeNamespace.Types);
            foreach (var itemx in this.BuildProperties)
            {

            }


            foreach (CodeTypeDeclaration testClassDeclaration in this.CodeNamespace.Types)
            {
                var fixtureAttr = new CodeAttributeDeclaration("TestFixture");
                testClassDeclaration.CustomAttributes.Add(fixtureAttr);

                if (testClassDeclaration.Name.Contains("MbUnit"))
                {
                }

                this.CurrentTestClassDeclaration = testClassDeclaration;

                var cts = new CodeTypeSetup(nd, this.BuildProperties, testClassDeclaration);

                var codeNamespace = cts.SetUpCodeNamespace(this.CodeNamespace.Name, this.RetrieveNamespaceImports());
                var testObjectName = cts.SetUpTestname();
                var baseKey = cts.BaseKey;

                testClassDeclaration.IsPartial = true;

                // Add testObject field
                var testObjectMemberField = AddTestMemberField(
                    testClassDeclaration, testObjectName, "testObject");

                // Give it a default public constructor
                var codeConstructor = new CodeConstructor { Attributes = MemberAttributes.Public };
                testClassDeclaration.Members.Add(codeConstructor);

                var initialMembers = testClassDeclaration.Members.Cast<CodeTypeMember>().ToList();

                // Setup and TearDown
                var setTearContext = this.GenerateSetupAndTearDown(
                    codeNamespace,
                    testClassDeclaration,
                    testObjectName,
                    testObjectMemberField,
                    this.BuildProperties);

                this.GenerateAdditional(setTearContext, testObjectName, testObjectMemberField);

                // Add test class to the CodeNamespace.
                codeNamespace.Types.Add(testClassDeclaration);

                // run the pre build step and builds the context lookup dictionary.
                List<CodeTypeMember> exludedMembers;
                var contextLookup = this.PreBuildStep(
                    testClassDeclaration,
                    codeNamespace,
                    initialMembers,
                    this.BuildProperties,
                    setTearContext,
                    out exludedMembers,
                    baseKey);

                foreach (var item in exludedMembers)
                {
                    initialMembers.Remove(item);
                    testClassDeclaration.Members.Remove(item);
                }

                // pre calculate property data and test names.
                this.DetermineTestNameAndSetPropertyBuildData(
                    contextLookup,
                    initialMembers,
                    this.BuildProperties);

                var addedCodeMethods = new List<CodeMemberMethod>();
                // Run test generation main routine.
                foreach (CodeTypeMember typeMember in initialMembers)
                {
                    if (typeMember is CodeMemberMethod)
                    {
                        if (!(typeMember is CodeConstructor))
                        {
                            this.CreateStubForCodeMemberMethod(typeMember as CodeMemberMethod);
                        }
                    }

                    /*var memberBuildContext = new MemberBuildContext(
                        codeNamespace, testClassDeclaration, typeMember, null, null);*/
                    var memberBuildContext = contextLookup[typeMember];
                    this.GenerateCodeTypeMember(memberBuildContext);
                    addedCodeMethods.AddRange(memberBuildContext.BuildResult.ClassMethodsToAdd);
                    if (memberBuildContext.BuildResult is MemberBuildResult)
                    {
                        ((MemberBuildResult)memberBuildContext.BuildResult).Reset();
                    }
                }

                // process the added CodeTypeMember's
                foreach (var addTypeMember in addedCodeMethods)
                {
                    var addMemberBuildContext = new MemberBuildContext(
                        codeNamespace, testClassDeclaration, addTypeMember, this.BuildProperties, setTearContext, baseKey);
                    this.GenerateCodeTypeMember(addMemberBuildContext);
                }

                {
                    var addSetUpBuildContext = new MemberBuildContext(
                        codeNamespace, testClassDeclaration, setTearContext.SetUpMethod, this.BuildProperties, setTearContext, baseKey);
                    this.GenerateCodeTypeMember(addSetUpBuildContext);
                }
                {
                    var addTearDownBuildContext = new MemberBuildContext(
                        codeNamespace, testClassDeclaration, setTearContext.TearDownMethod, this.BuildProperties, setTearContext, baseKey);
                    this.GenerateCodeTypeMember(addTearDownBuildContext);
                }
                /*{
                 * no need for this, the ctor is handled above
                    // Process the constructor.
                    var typeMember = codeConstructor;
                    var memberBuildContext = new MemberBuildContext(
                        codeNamespace, testClassDeclaration, typeMember, propertyData, setTearContext);

                    var builders = this.TestBuilders.GetBuilder(memberBuildContext).ToArray();
                    var composedTestName = memberBuildContext.TypeMember.Name;
                    foreach (var memberBuilder in builders)
                    {
                        composedTestName = ComputeTestName(memberBuilder, memberBuildContext, composedTestName);
                    }
                    memberBuildContext.TestKey = composedTestName;
                    
                    this.GenerateCodeTypeMember(memberBuildContext);
                }*/
                RemoveDuplicatedMembers(testClassDeclaration);
                SortMembers(testClassDeclaration);
                this.WriteClassFile(cts.FixForWriteFile(testClassDeclaration.Name), codeNamespace);
            }
        }

        /// <summary>
        /// Compose additional items of the test setup method.
        /// </summary>
        /// <param name="buildData">The build data dictionary.</param>
        /// <param name="setUpMethod">A reference to the test setup method.</param>
        /// <param name="testObjectMemberField">The member field of the object under test.</param>
        /// <param name="testObjectName">The name of the object under test.</param>
        /// <param name="testObjectType">Type of the object under test(OuT).</param>
        /// <returns>
        /// A test object creator for the object under test.
        /// Is <c>null</c>, when none is created.
        /// </returns>
        protected virtual ITestObjectComposer ComposeTestSetupMethod(
            BuildDataDictionary buildData,
            CodeMemberMethod setUpMethod,
            CodeMemberField testObjectMemberField,
            string testObjectName,
            Type testObjectType)
        {
            ITestObjectComposer objectBuilder = new TestObjectComposer(
                buildData, setUpMethod, testObjectMemberField, testObjectName, testObjectType);

            //if (testObjectType.Name.StartsWith("TestObjectComposer"))
            /*if (testObjectType.Name == "Dependency")
            {

            }*/

            // With a INotifyPropertyChanged based OuT, call base.Setup();
            if (typeof(System.ComponentModel.INotifyPropertyChanged).IsAssignableFrom(testObjectType))
            {
                setUpMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                var baseInvoker = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), setUpMethod.Name);

                setUpMethod.Statements.Add(baseInvoker);
            }

            // var testObjectConstructor = cr.BuildTestObject();
            // cr.AssignParameters(this.CurrentTestClassDeclaration, cr.TestObjectMemberFieldCreateExpression);
            objectBuilder.BuildTestObject(Configuration.MethodGeneratorLevelOfDetail);
            objectBuilder.AssignParameters(this.CurrentTestClassDeclaration);


            // return testObjectConstructor;
            return objectBuilder;

            /*var invokeExpression = new CodeMethodInvokeExpression(
                            new CodeTypeReferenceExpression("Assert"),
                            "AreEqual",
                            //new CodePrimitiveExpression("expected")
                            new CodeFieldReferenceExpression(testObjectMemberField, "bla")
                            , new CodeVariableReferenceExpression("actual"));*/

            /*var fieldRef1 =
                new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), testObjectMemberField.Name);

            var testObjectMemberFieldCreate = new CodeObjectCreateExpression(testObjectName, new CodeExpression[] { });
            var as1 = new CodeAssignStatement(fieldRef1, testObjectMemberFieldCreate);

            // Creates a statement using a code expression.
            // var expressionStatement = new CodeExpressionStatement(fieldRef1);
            setUpMethod.Statements.Add(as1);
            return testObjectMemberFieldCreate;*/
        }

        /// <summary>
        /// Compose additional items of the test TearDown method.
        /// </summary>
        /// <param name="teardownMethod">A reference to the TearDown method of the test.</param>
        /// <param name="testObjectMemberField">The member field of the object under test.</param>
        /// <param name="testObjectName">The name of the object under test.</param>
        /// <param name="testObjectType">Type of the object under test(OuT).</param>
        protected virtual void ComposeTestTearDownMethod(
            CodeMemberMethod teardownMethod,
            CodeMemberField testObjectMemberField,
            string testObjectName,
            Type testObjectType)
        {
            // With a INotifyPropertyChanged based OuT, call base.TearDown();
            if (typeof(System.ComponentModel.INotifyPropertyChanged).IsAssignableFrom(testObjectType))
            {
                teardownMethod.Attributes = MemberAttributes.Override | MemberAttributes.Public;
                var baseInvoker = new CodeMethodInvokeExpression(new CodeBaseReferenceExpression(), teardownMethod.Name);

                teardownMethod.Statements.Add(baseInvoker);
            }
        }

        /// <summary>
        /// Computes the test method code member via an <see cref="IMemberBuilder"/>.
        /// </summary>
        /// <param name="memberBuilder">The member builder used for the <paramref name="context"/>.</param>
        /// <param name="context">The build context of the test method member.</param>
        protected virtual void MemberBuilderRunBuild(IMemberBuilder memberBuilder, IMemberBuildContext context)
        {
            memberBuilder.Build(context);
        }

        /// <summary>
        /// Computes the test method name via an <see cref="IMemberBuilder"/>.
        /// </summary>
        /// <param name="memberBuilder">The member builder used for the <paramref name="context"/>.</param>
        /// <param name="context">The build context of the test method member.</param>
        /// <param name="originalName">Initial name of the test method name.</param>
        /// <returns>The calculated test method name.</returns>
        protected virtual string MemberBuilderGetTestname(
            IMemberBuilder memberBuilder, IMemberBuildContext context, string originalName)
        {
            var testName = memberBuilder.GetTestName(context, originalName);
            return testName;
        }

        /// <summary>
        /// Runs the pre build step of the test method via an <see cref="IMemberBuilder"/>.
        /// </summary>
        /// <param name="memberBuilder">The member builder used for the <paramref name="context"/>.</param>
        /// <param name="context">The build context of the test method member.</param>
        protected virtual void MemberBuilderRunPreBuildStep(
            IMemberBuilder memberBuilder, IMemberPreBuildContext context)
        {
            memberBuilder.RunPreBuild(context);
        }

        /*private Dictionary<string, CodeMemberField> testMemberFieldLookup =
            new Dictionary<string, CodeMemberField>();

        protected IDictionary<string, CodeMemberField> TestMemberFieldLookup
        {
            get
            {
                return this.testMemberFieldLookup;
            }
        }*/

        /*/// <summary>
        /// Handle event related stuff before type generation.
        /// </summary>
        /// <param name="typeMember">The type member.</param>
        /// <param name="eventName">Name of the event.</param>
        protected abstract void ComputeCodeMemberEvent(CodeMemberMethod typeMember, string eventName);*/

        /*/// <summary>
        /// Handle property related stuff before type generation.
        /// </summary>
        /// <param name="typeMember">The type member.</param>
        /// <param name="propertyName">Name of the property.</param>
        protected abstract void ComputeCodeMemberProperty(CodeMemberMethod typeMember, string propertyName);*/

        /// <summary>
        /// Creates a custom member method with an attribute of the same name.
        /// </summary>
        /// <param name="methodName">Name of the method and attribute.</param>
        /// <returns>The new code member method with the specified method.</returns>
        protected CodeMemberMethod CreateCustomCodeMemberMethodWithSameNameAsAttribute(string methodName)
        {
            var codeMemberMethod = new CustomCodeMemberMethod
                                       {
                                           Attributes = MemberAttributes.Public | MemberAttributes.Final,
                                           Name = methodName,
                                           ReturnType = new CodeTypeReference(typeof(void))
                                       };

            // Clean the member name and append 'Test' to the end of it

            // Standard test methods accept no parameters and return void.
            codeMemberMethod.Parameters.Clear();

            var setupAttr = new CodeAttributeDeclaration(new CodeTypeReference(methodName));

            codeMemberMethod.CustomAttributes.Add(setupAttr);
            codeMemberMethod.Statements.Add(
                new CodeCommentStatement("ToDo: Implement " + methodName + " logic here "));
            return codeMemberMethod;
        }

        // protected abstract void CreateStubForCodeMemberMethod(CodeMemberMethod codeMemberMethod);

        /// <summary>
        /// Creates the stub for the code member method.  This method actually
        /// implements the method body for the test method.
        /// </summary>
        /// <param name="codeMemberMethod">The code member method.</param>
        protected virtual void CreateStubForCodeMemberMethod(CodeMemberMethod codeMemberMethod)
        {
            CodeMethodComposer.CreateTestStubForMethod(codeMemberMethod);
            codeMemberMethod.Attributes = MemberAttributes.Public | MemberAttributes.Final;
        }

        /// <summary>
        /// Generates additional members of the test class.
        /// </summary>
        /// <param name="context">Contains data specific to SetUp and TearDown test-method generation.</param>
        /// <param name="testObjectName">The name of the object under test.</param>
        /// <param name="testObjectMemberField">The member field of the object under test.</param>
        protected virtual void GenerateAdditional(
            ISetupAndTearDownCreationContext context, string testObjectName, CodeMemberField testObjectMemberField)
        {
        }

        /// <summary>
        /// Add namespace imports to the main compilation unit.
        /// </summary>
        /// <returns>A list of code name spaces, to be added to the compilation unit.</returns>
        protected abstract IEnumerable<string> RetrieveNamespaceImports();

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
            // var ctm = codeTypeDeclaration.Members.OfType<CodeTypeMember>().ToArray();
            var ctm2 =
                codeTypeDeclaration.Members.OfType<CodeTypeMember>().Distinct(DuplicatedMemberComparerInstance).ToArray(
                    );
            codeTypeDeclaration.Members.Clear();
            foreach (var item in ctm2)
            {
                codeTypeDeclaration.Members.Add(item);
            }

            /*return;
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
                        }*/
        }

        private static void SortMembers(CodeTypeDeclaration codeTypeDeclaration)
        {
            var members = codeTypeDeclaration.Members.OfType<CodeTypeMember>();

            // var ordered = members.OrderBy(e => e, new CodeTypeDeclarationComparer());
            // var grp = members.OrderBy(e => e, new CodeTypeDeclarationComparer()).GroupBy(e => e.GetType().FullName);
            var grp = members.GroupBy(e => e.GetType().FullName).OrderBy(e => e.Key);
            {
                // var result = ordered.ToArray();
                // foreach (var item in grp)
                // item.Select(e=>e.
            }

            var result = grp.SelectMany(
                (e, k) => e.Select(s => s).OrderBy(o => o, new CodeTypeDeclarationComparer())).ToArray();
            codeTypeDeclaration.Members.Clear();
            codeTypeDeclaration.Members.AddRange(result);
        }

        /// <summary>
        /// Processes the code type members of the compilation unit.
        /// </summary>
        /// <param name="context">The build context for the test to create.</param>
        private void GenerateCodeTypeMember(MemberBuildContext context/*, bool runOnlyMultiBuilders*/)
        {
            var typeMember = context.TypeMember;

            // var initialTypeMemberName = typeMember.Name;
            if (typeMember is CodeMemberMethod)
            {
                // We don't generate default constructors
                /*if (typeMember is CodeMemberProperty)
                {
                    // before stub has created.
                    // PreComputeCodeMemberProperty(typeMember);
                }*/
                var builders = this.TestBuilders.GetBuilder(context).ToArray();

                /*foreach (var memberBuilder in builders)
                                {
                                    // Set the test name from the pre computed one.
                                    // var testName = context.TypeMember.Name;
                                    // testName = ComputeTestName(memberBuilder, context, testName);
                                    // context.TypeMember.Name = testName;
                                }*/

                // set the test method name to the context key that is the normalized test name.
                //context.TypeMember.Name = context.TestKey;

                foreach (var memberBuilder in builders)
                {
                    this.MemberBuilderRunBuild(memberBuilder, context);
                    if (context.BuildResult.ClassMethodsToAdd.Count > 0)
                    {

                    }
                }

                if (!(typeMember is CodeConstructor))
                {
                    CodeMethodComposer.AppendAssertInconclusive(
                        typeMember as CodeMemberMethod,
                        "Verify the correctness of this test method.");
                }

                /*if (context.IsProperty)
                {
                    var propertyName = typeMemberName.Replace("get_", string.Empty).Replace("set_", string.Empty);

                    // hmm Generate to generate new and compute to process existing !?!
                    this.ComputeCodeMemberProperty(typeMember as CodeMemberMethod, propertyName);
                }
                else if (context.IsEvent)
                {
                    var eventName = typeMemberName.Replace("add_", string.Empty).Replace("remove_", string.Empty);

                    // hmm Generate to generate new and compute to process existing !?!
                    this.ComputeCodeMemberEvent(typeMember as CodeMemberMethod, eventName);
                }*/
            }
        }

        /// <summary>
        /// Generates the setup and tear down methods.
        /// </summary>
        /// <param name="codeNamespace">The code namespace of the test class.</param>
        /// <param name="testClassDeclaration">The test class declaration.</param>
        /// <param name="testObjectName">The name of the object under test.</param>
        /// <param name="testObjectMemberField">The member field of the object under test.</param>
        /// <param name="buildData">The build data dictionary.</param>
        /// <returns>
        /// Data specific to SetUp and TearDown test-method generation.
        /// </returns>
        private ISetupAndTearDownCreationContext GenerateSetupAndTearDown(
            CodeNamespace codeNamespace,
            CodeTypeDeclaration testClassDeclaration,
            string testObjectName,
            CodeMemberField testObjectMemberField,
            BuildDataDictionary buildData)
        {
            var setUpMethod = this.CreateCustomCodeMemberMethodWithSameNameAsAttribute("SetUp");
            var testObjectType = testClassDeclaration.UserData[NStubConstants.UserDataClassTypeKey] as Type;

            var creator = this.ComposeTestSetupMethod(
                buildData, setUpMethod, testObjectMemberField, testObjectName, testObjectType);

            testClassDeclaration.Members.Add(setUpMethod);
            var tearDownMethod = this.CreateCustomCodeMemberMethodWithSameNameAsAttribute("TearDown");
            this.ComposeTestTearDownMethod(tearDownMethod, testObjectMemberField, testObjectName, testObjectType);
            testClassDeclaration.Members.Add(tearDownMethod);

            var result = new SetupAndTearDownContext(
                buildData, codeNamespace, testClassDeclaration, setUpMethod, tearDownMethod, creator);
            return result;

            /*var assignedMockObjects = this.ComposeTestSetupMockery(
                codeTypeDeclaration, setUpMethod, testObjectMemberField, testObjectName);
            if (assignedMockObjects.Count() > 0)
            {
                foreach (var mockObject in assignedMockObjects)
                {
                    testObjectMemberFieldCreate.Parameters.Add(mockObject.Left);
                }

                string rhinoImport = typeof(MockRepository).Namespace;
                codeNamespace.Imports.Add(new CodeNamespaceImport(rhinoImport));
            }*/
        }

        private Dictionary<CodeTypeMember, MemberBuildContext> PreBuildStep(
            CodeTypeDeclaration testClassDeclaration,
            CodeNamespace codeNamespace,
            IEnumerable<CodeTypeMember> initialMembers,
            BuildDataDictionary propertyData,
            ISetupAndTearDownCreationContext setTearContext,
            out List<CodeTypeMember> exludedMembers,
            string baseKey)
        {
            var contextLookup = new Dictionary<CodeTypeMember, MemberBuildContext>();
            var exclusions = new Dictionary<CodeTypeMember, bool>();

            // var propertyData = new Dictionary<string, IBuilderData>();
            // buildData.AddDataItem(propertyData);
            foreach (CodeTypeMember typeMember in initialMembers)
            {
                var memberBuildContext = new MemberBuildContext(
                    codeNamespace, testClassDeclaration, typeMember, propertyData, setTearContext, baseKey);

                // pre-calculate the name of the test method. Todo: maybe this step can be skipped 
                // if i put the stuff here into this.GenerateCodeTypeMember(...)
                var builders = this.TestBuilders.GetBuilder(memberBuildContext).ToArray();
                var composedTestName = memberBuildContext.TypeMember.Name;
                foreach (var memberBuilder in builders)
                {
                    this.MemberBuilderRunPreBuildStep(memberBuilder, memberBuildContext);
                    var buildResult = memberBuildContext.PreBuildResult;
                    if (buildResult.ExcludeMember)
                    {
                        // bool isExcluded;
                        // bool found = exclusions.TryGetValue(typeMember, out isExcluded);
                        if (!exclusions.ContainsKey(typeMember))
                        {
                            exclusions[typeMember] = true;
                            //break;
                        }
                    }
                }

                contextLookup.Add(typeMember, memberBuildContext);

                /*if (memberBuildContext.IsProperty)
                {
                    IBuilderData propertyDataItem;
                    var found = propertyData.TryGetValue("Property", composedTestName, out propertyDataItem);
                    if (found)
                    {
                        propertyDataItem.SetData(memberBuildContext.MemberInfo);
                        if (propertyDataItem is PropertyBuilderData)
                        {
                            var propData = (PropertyBuilderData)propertyDataItem;
                        }
                    }
                    else
                    {
                        var propdata = new PropertyBuilderData();
                        propdata.SetData(memberBuildContext.MemberInfo);
                        propertyData.AddDataItem("Property", composedTestName, propdata);
                    }
                }*/
            }

            exludedMembers = exclusions.Keys.ToList();
            return contextLookup;
        }

        private void DetermineTestNameAndSetPropertyBuildData(
            Dictionary<CodeTypeMember, MemberBuildContext> contextLookup,
            IEnumerable<CodeTypeMember> initialMembers,
            BuildDataDictionary propertyData)
        {
            foreach (CodeTypeMember typeMember in initialMembers)
            {
                var memberBuildContext = contextLookup[typeMember];

                var builders = this.TestBuilders.GetBuilder(memberBuildContext).ToArray();
                var composedTestName = memberBuildContext.TypeMember.Name;
                foreach (var memberBuilder in builders)
                {
                    composedTestName = this.MemberBuilderGetTestname(memberBuilder, memberBuildContext, composedTestName);
                }

                typeMember.Name = composedTestName;

                // memberBuildContext.TestKey = composedTestName;
                // memberBuildContext.TestKey = CodeNamespace.Name + "." + this.CurrentTestClassDeclaration.Name + "." + composedTestName;

                // Setting the memberinfo for properties in the category 'Property' with the composedTestName as key.
                // Todo: hmmm, this was all about that IBuildData and property get/set detection ... and how to find out
                // a way to store properties .... So IBuildData is code generator internal and IMemberBuilderParameters is
                // for user setup. well. look into the usefullness of this. ... later

                // var storageCategory = "Property" + "." + this.CurrentTestClassDeclaration.Name;
                //var storageCategory = string.Format(BuilderConstants.PropertyStorageCategory, memberBuildContext.TestClassDeclaration.Name);
                var storageCategory = string.Format(BuilderConstants.PropertyStorageCategory, memberBuildContext.TestClassDeclaration.Name);
                //var propertyDataKey = composedTestName;
                var propertyDataKey = memberBuildContext.TestKey;

                if (memberBuildContext.IsProperty)
                {
                    if (memberBuildContext.TestClassDeclaration.Name == "VertexDataTest")
                    {
                        if (composedTestName.Contains("PropertyItem"))
                        {
                        }
                    }

                    IBuilderData propertyDataItem;
                    var found = propertyData.TryGetValue(storageCategory, propertyDataKey, out propertyDataItem);
                    if (found)
                    {
                        propertyDataItem.SetData(memberBuildContext.MemberInfo);
                        if (propertyDataItem is PropertyBuilderData)
                        {
                            var propData = (PropertyBuilderData)propertyDataItem;
                        }
                    }
                    else
                    {
                        var propdata = new PropertyBuilderData();
                        propdata.SetData(memberBuildContext.MemberInfo);
                        propertyData.AddDataItem(storageCategory, propertyDataKey, propdata);
                    }
                }
            }
        }

        /// <summary>
        /// Writes the class file.  This method actually creates the physical
        /// class file and populates it accordingly.
        /// </summary>
        /// <param name="className">Name of the class file to be written.</param>
        /// <param name="codeNamespace">The <see cref="CodeNamespace"/> which represents the
        /// file to be written.</param>
        private void WriteClassFile(string className, CodeNamespace codeNamespace)
        {
            var csharpCodeProvider = new CSharpCodeProvider();
            string sourceFile = this.OutputDirectory + this.buildSystem.DirectorySeparatorChar +
                                className + "." + csharpCodeProvider.FileExtension;
            sourceFile = Utility.ScrubPathOfIllegalCharacters(sourceFile);
            var indentedTextWriter =
                new IndentedTextWriter(this.buildSystem.GetTextWriter(sourceFile, false), "  ");
            var codeGenerationOptions = new CodeGeneratorOptions { BracingStyle = "C" };
            csharpCodeProvider.GenerateCodeFromNamespace(
                codeNamespace,
                indentedTextWriter,
                codeGenerationOptions);
            indentedTextWriter.Flush();
            indentedTextWriter.Close();
        }

        #region Nested type: CodeTypeDeclarationComparer

        /// <summary>
        /// Comparer for test member sorting.
        /// </summary>
        private class CodeTypeDeclarationComparer : IComparer<CodeTypeMember>
        {
            /// <summary>
            /// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first object to compare.</param>
            /// <param name="y">The second object to compare.</param>
            /// <returns>
            /// Value
            /// Condition
            /// Less than zero
            /// <paramref name="x"/> is less than <paramref name="y"/>.
            /// Zero
            /// <paramref name="x"/> equals <paramref name="y"/>.
            /// Greater than zero
            /// <paramref name="x"/> is greater than <paramref name="y"/>.
            /// </returns>
            public int Compare(CodeTypeMember x, CodeTypeMember y)
            {
                int diff = 0;
                CompareNameStart(-2, "Event", x, y, ref diff);
                CompareNameStart(-1, "Property", x, y, ref diff);

                // CompareNameStart(1, "TestConstructor", x, y, ref diff);
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

            private static void CompareNameStart(
                int rank, string name, CodeTypeMember x, CodeTypeMember y, ref int diff)
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

        #endregion

        #region Nested type: CustomCodeMemberMethod

        /// <summary>
        /// Represents a Custom Code Member Method. Used for sorting the test members.
        /// </summary>
        private class CustomCodeMemberMethod : CodeMemberMethod
        {
        }

        #endregion

        #region Nested type: DuplicatedMemberComparer

        /// <summary>
        /// Checks for duplicate class members.
        /// </summary>
        private class DuplicatedMemberComparer : IEqualityComparer<CodeTypeMember>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <paramref name="x"/> to compare.</param>
            /// <param name="y">The second object of type <paramref name="y"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the specified objects are equal; otherwise, <c>false</c>.
            /// </returns>
            public bool Equals(CodeTypeMember x, CodeTypeMember y)
            {
                return x.Name.Equals(y.Name);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object to get the hash for.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            /// <exception cref="T:System.ArgumentNullException">
            /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is <c>null</c>.
            /// </exception>
            public int GetHashCode(CodeTypeMember obj)
            {
                return obj.Name.GetHashCode();
            }
        }

        #endregion

        #region Nested type: GroupMethodsComparer

        /// <summary>
        /// Used for the group ordering of test methods into SetUp and TearDown, Constructors, Properties, etc. 
        /// </summary>
        private class GroupMethodsComparer : IEqualityComparer<string>
        {
            /// <summary>
            /// Determines whether the specified objects are equal.
            /// </summary>
            /// <param name="x">The first object of type <paramref name="x"/> to compare.</param>
            /// <param name="y">The second object of type <paramref name="y"/> to compare.</param>
            /// <returns>
            /// <c>true</c> if the specified objects are equal; otherwise, <c>false</c>.
            /// </returns>
            public bool Equals(string x, string y)
            {
                return x.Equals(y);
            }

            /// <summary>
            /// Returns a hash code for this instance.
            /// </summary>
            /// <param name="obj">The object to get the hash code of.</param>
            /// <returns>
            /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
            /// </returns>
            /// <exception cref="T:System.ArgumentNullException">
            /// The type of <paramref name="obj"/> is a reference type and <paramref name="obj"/> is <c>null</c>.
            /// </exception>
            public int GetHashCode(string obj)
            {
                return obj.GetHashCode();
            }
        }

        #endregion
    }
}