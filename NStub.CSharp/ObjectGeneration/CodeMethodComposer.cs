﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeMethodComposer.cs" company="EvePanix">
//   Copyright (c) Jedzia 2001-2012, EvePanix. All rights reserved.
//   See the license notes shipped with this source and the GNU GPL.
// </copyright>
// <author>Jedzia</author>
// <email>jed69@gmx.de</email>
// <date>$date$</date>
// --------------------------------------------------------------------------------------------------------------------

namespace NStub.CSharp.ObjectGeneration
{
    using System;
    using System.Linq;
    using System.CodeDom;
    using NStub.Core;
    using NStub.CSharp.ObjectGeneration.FluentCodeBuild;

    /// <summary>
    /// Provides helper methods for composition of <see cref="CodeMemberMethod"/> method members.
    /// </summary>
    public static class CodeMethodComposer
    {
        /// <summary>
        /// Appends a 'Assert.Inconclusive("Text")' statement to the specified method.
        /// </summary>
        /// <param name="codeMemberMethod">The code member method.</param>
        /// <param name="inconclusiveText">The warning parameter text of the Assert.Inconclusive call.</param>
        public static void AppendAssertInconclusive(CodeMemberMethod codeMemberMethod, string inconclusiveText)
        {
            // add a blank line and Assert.Inconclusive(" specified text ..."); to the method body.
            codeMemberMethod
                .AddBlankLine()
                .StaticClass("Assert").Invoke("Inconclusive").With(inconclusiveText).Commit();
        }

        /// <summary>
        /// Creates a reference to a member field and initializes it with a new instance of the specified parameter type.
        /// Sample values are used as the initializing expression.
        /// </summary>
        /// <param name="type">Defines the type of the new object.</param>
        /// <param name="memberField">Name of the referenced member field.</param>
        /// <returns>An assignment statement for the specified member field.</returns>
        /// <remarks>With a custom Type, this method produces a statement with a initializer like: 
        /// <code>this.project = new Microsoft.Build.BuildEngine.Project();</code>.
        /// or let myInt be of type int:
        /// <code>this.myInt = 1234;</code>.
        /// myType of type System.Type:
        /// <code>this.myType = typeof(System.Object);</code>.
        /// </remarks>
        public static CodeAssignStatement CreateAndInitializeMemberField(Type type, string memberField)
        {
            if (type == typeof(object))
            {

            }
            var fieldRef1 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberField);
            CodeExpression assignExpr = CreateExpressionByType(type, memberField);

            return new CodeAssignStatement(fieldRef1, assignExpr);
        }

        /// <summary>
        /// Creates a reference to a collection based member field and initializes it with a new instance of the
        /// specified parameter type and adds a collection item to it.
        /// Sample values are used as the initializing expression.
        /// </summary>
        /// <param name="memberCollectionField">Name of the referenced collection field.</param>
        /// <param name="collectionInitializers">Defines the types of the new object list.</param>
        /// <returns>
        /// An assignment statement for the specified collection member field.
        /// </returns>
        /// <remarks>
        /// With a custom Type, this method produces a statement with a initializer like:
        /// <code>this.paths = new[] { pathsItem };</code>.
        /// where the item is defined like:
        /// <code>this.pathsItem = new PathItemType();</code>.
        /// myType of type System.Type:
        /// <code>this.pathsItem = "An Item";</code>.
        /// </remarks>
        public static CodeAssignStatement CreateAndInitializeCollectionField(
            //Type type, 
            string memberCollectionField, 
            params string[] collectionInitializers)
        {
            /*if (type == typeof(object))
            {

            }*/
            var fieldRef1 = new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), memberCollectionField);
            //CodeExpression assignExpr = CreateExpressionByType(type, memberCollectionField);
            var para = collectionInitializers.Aggregate((x, y) => x += "," + y); 
            CodeExpression assignExpr = new CodeSnippetExpression("new[] { " + para + " }");

            return new CodeAssignStatement(fieldRef1, assignExpr);
        }

        public static CodeExpression CreateExpressionByType(Type type, string memberField)
        {
            CodeExpression assignExpr;
            if (type.IsAssignableFrom(typeof(string)))
            {
                assignExpr = new CodePrimitiveExpression("Value of " + memberField);
            }
            else if (type.IsAssignableFrom(typeof(bool)))
            {
                assignExpr = new CodePrimitiveExpression(true);
            }
            else if (type.IsAssignableFrom(typeof(Type)))
            {
                assignExpr = new CodeTypeOfExpression(typeof(object));
            }
            else if (type.IsAssignableFrom(typeof(int)) || type.IsAssignableFrom(typeof(uint)) ||
                     type.IsAssignableFrom(typeof(short)))
            {
                assignExpr = new CodePrimitiveExpression(1234);
            }
            else
            {
                assignExpr = new CodeObjectCreateExpression(type.FullName, new CodeExpression[] { });
            }
            return assignExpr;
        }

        /// <summary>
        /// Creates the stub for the specified code member method.  This method actually
        /// implements the method body for the test method.
        /// </summary>
        /// <param name="codeMemberMethod">The code member method.</param>
        public static void CreateTestStubForMethod(CodeMemberMethod codeMemberMethod)
        {
            // Clean the member name and append 'Test' to the end of it
            var origName = Utility.ScrubPathOfIllegalCharacters(codeMemberMethod.Name);

            // Standard test methods accept no parameters and return void.
            codeMemberMethod
                .SetName(origName + "Test")
                .WithReturnType(typeof(void))
                .ClearParameters()
                .AddMethodAttribute("Test")
                .AddComment("TODO: Implement unit test for " + origName);
        }
    }
}