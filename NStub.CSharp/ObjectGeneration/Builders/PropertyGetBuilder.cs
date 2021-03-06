﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyGetBuilder.cs" company="EvePanix">
//   Copyright (c) Jedzia 2001-2012, EvePanix. All rights reserved.
//   See the license notes shipped with this source and the GNU GPL.
// </copyright>
// <author>Jedzia</author>
// <email>jed69@gmx.de</email>
// <date>$date$</date>
// --------------------------------------------------------------------------------------------------------------------

namespace NStub.CSharp.ObjectGeneration.Builders
{
    using System;
    using System.CodeDom;
    using NStub.CSharp.BuildContext;

    /// <summary>
    /// Test method generator for the 'get' part of property type members.
    /// </summary>
    public class PropertyGetBuilder : MemberBuilder
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyGetBuilder"/> class.
        /// </summary>
        /// <param name="context">The build context of the test method member.</param>
        public PropertyGetBuilder(IMemberSetupContext context)
            : base(context)
        {
        }

        #endregion

        /// <summary>
        /// Determines whether this instance can handle a specified build context.
        /// </summary>
        /// <param name="context">The build context of the test method member.</param>
        /// <returns>
        /// <c>true</c> if this instance can handle the specified context; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanHandleContext(IMemberBuildContext context)
        {
            return context.IsProperty;
        }

        /// <summary>
        /// Builds the specified context.
        /// </summary>
        /// <param name="context">The build context of the test method member.</param>
        /// <returns>
        /// <c>true</c> on success.
        /// </returns>
        protected override bool BuildMember(IMemberBuildContext context)
        {
            var typeMember = context.TypeMember;
            var typeMemberName = typeMember.Name;
            var propertyName = typeMemberName;

            // var propertyName = typeMemberName.Replace("get_", string.Empty).Replace("set_", string.Empty);
            // BaseCSharpCodeGenerator.ReplaceTestInTestName(typeMember, "XX_Norm_XX");
            var storageCategory = string.Format(BuilderConstants.PropertyStorageCategory, context.TestClassDeclaration.Name);
            var propertyData = context.GetBuilderData(storageCategory);

            // var testName = DetermineTestName(context);
            // hmm Generate to generate new and compute to process existing !?!
            const string TestObjectName = "testObject";

            this.ComputeCodeMemberProperty(
                context, typeMember as CodeMemberMethod, propertyData, TestObjectName, propertyName);
            return true;
        }

        /// <summary>
        /// Handle property related stuff before type generation.
        /// </summary>
        /// <param name="context">The context. Todo: remove it with specialized parameters after development.</param>
        /// <param name="typeMember">The type member.</param>
        /// <param name="builderData">The builder data.</param>
        /// <param name="testObjectName">Name of the test object member field.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="NotImplementedException">The context of the builder does not supply a valid 
        /// <see cref="ISetupAndTearDownCreationContext"/>.</exception>
        /// <exception cref="NotImplementedException">The context of the builder does not supply a valid 
        /// <see cref="TestObjectComposer"/>.</exception>
        protected virtual void ComputeCodeMemberProperty(
            IMemberBuildContext context,
            CodeMemberMethod typeMember,
            IBuilderData builderData,
            string testObjectName,
            string propertyName)
        {
            var propertyData = builderData as PropertyBuilderData;

            if (propertyData == null)
            {
                return;
            }

            var setAccessor = propertyData.SetAccessor;
            var getAccessor = propertyData.GetAccessor;

            if (getAccessor == null)
            {
                return;
            }

            if (setAccessor == null)
            {
                // create the actual and expected var's here.
                // actualRef
                // expectedRef
            }

            var propName = propertyData.PropertyName;

            // devel: how to create a string initializer from possible constructor setups of the 'SetUp' method.
            var co = context.SetUpTearDownContext as ISetupAndTearDownCreationContext;
            if (co == null)
            {
                throw new NotImplementedException("The context of the builder does not supply a valid ISetupAndTearDownCreationContext.");
            }

            // Todo: put these used methods into the interfaces of TestObjectComposer and maybe modify 
            // ISetupAndTearDownContext that it provides the co.TestObjectCreator. ... or fix all this in the
            // pre-build?!!
            var creator = co.TestObjectCreator as TestObjectComposer;
            if (creator == null)
            {
                throw new NotImplementedException("The context of the builder does not supply a valid TestObjectBuilder.");
            }

            ConstructorAssignment ctorAssignment;
            var found = creator.TryFindConstructorAssignment(propName, out ctorAssignment, false);


            CodeExpression ctorAssignmentRight = CodeMethodComposer.CreateExpressionByType(getAccessor.ReturnType, propertyData.PropertyName);
            // CodeExpression ctorAssignmentRight = new CodePrimitiveExpression("Insert expected object here");
            var assertionKind = "AreEqual";
            if (found)
            {
                // use the value of the ctor initializer.
                //ctorAssignmentRight = ctorAssignment.AssignStatement.Right;
                // use the field reference itself.
                ctorAssignmentRight = ctorAssignment.AssignStatement.Left;
                assertionKind = "AreSame";
            }

            typeMember.Statements.Add(new CodeSnippetStatement(string.Empty));
            typeMember.Statements.Add(new CodeCommentStatement("Test read access of '" + propName + "' Property."));

            var expectedAsign = new CodeVariableDeclarationStatement(
                "var",
                "expected",
                ctorAssignmentRight);
            typeMember.Statements.Add(expectedAsign);

            var testObjRef = new CodeTypeReferenceExpression(testObjectName);
            var testPropRef = new CodePropertyReferenceExpression(testObjRef, propName);
            var actualAsign = new CodeVariableDeclarationStatement("var", "actual", testPropRef);
            typeMember.Statements.Add(actualAsign);

            var assertExpr = new CodeMethodInvokeExpression(
                new CodeTypeReferenceExpression("Assert"),
                assertionKind,
                new CodeVariableReferenceExpression("expected"),
                new CodeVariableReferenceExpression("actual"));
            typeMember.Statements.Add(assertExpr);
        }
    }
}