namespace NStub.CSharp.Tests.ObjectGeneration
{
    using global::MbUnit.Framework;
    using System.CodeDom;
    using NStub.CSharp.ObjectGeneration;
    using NStub.CSharp.ObjectGeneration.FluentCodeBuild;
    using NStub.CSharp.Tests.FluentChecking;
    using System;
    using System.Linq;

    public partial class CodeTypeReferenceBinderTest
    {

        private CodeMemberMethod method;
        private CodeTypeReferenceBinder testObject;
        
        [SetUp()]
        public void SetUp()
        {
            //var ctdecl = new CodeTypeDeclaration("MyClass");
            this.method = new CodeMemberMethod();
            this.testObject = this.method.StaticClass("ReferencedClass");
        }

        [Test()]
        public void ConstructedFromStaticClass()
        {
            Assert.IsNotNull(testObject.TypeReference);
            Assert.IsInstanceOfType<CodeTypeReferenceExpression>(testObject.TypeReference);
            Assert.IsEmpty(method.Statements);
            // cm.StaticClass("Assert").Invoke("Inconclusive").With("Thisone").Commit();
        }

        [Test()]
        public void PropertyTypeReferenceNormalBehavior()
        {
            // Test read access of 'TypeReference' Property.
            var expected = "ReferencedClass";
            var actual = testObject.TypeReference.Type.BaseType;
            Assert.AreEqual(expected, actual);
        }

        [Test()]
        public void PropertyIsTypeReferenceNormalBehavior()
        {
            // Test read access of 'IsTypeReference' Property.
            var actual = testObject.IsTypeReference;
            Assert.IsTrue(actual);

            testObject = new CodeTypeReferenceBinder(this.method, new CodeFieldReferenceExpression());
            actual = testObject.IsTypeReference;
            Assert.IsFalse(actual);
        }

        [Test()]
        public void AssignFieldTest()
        {
            var result = testObject.AssignField("memberField");
            Assert.AreSame(method, result);
            Assert.IsNotEmpty(method.Statements);
            Assert.AreEqual(1, method.Statements.Count);

            AssertEx.That(method.StatementsOfType<CodeAssignStatement>()
                .Where().ExpressionLeft<CodeFieldReferenceExpression>(Is.FieldNamed("memberField"))
                .Assert());

            result = testObject.AssignField("otherField");
            Assert.AreSame(method, result);
            Assert.AreEqual(2, method.Statements.Count);

            AssertEx.That(method.StatementsOfType<CodeAssignStatement>()
                .Where().ExpressionLeft<CodeFieldReferenceExpression>(
                Is.Named<CodeFieldReferenceExpression>(e => CheckName(e, "otherField"))
                ).Assert());

        }

        public CompareResult CheckName(CodeFieldReferenceExpression e, string fieldName)
        {
            return new CompareResult(e.FieldName == fieldName, e.FieldName, fieldName);
        }

        [Test()]
        public void AssignLocalTest()
        {
            var result = testObject.AssignLocal("localVar", false);
            Assert.AreSame(method, result);
            Assert.IsNotEmpty(method.Statements);
            Assert.AreEqual(1, method.Statements.Count);

            AssertEx.That(method.StatementsOfType<CodeAssignStatement>()
                .Where().ExpressionLeft<CodeVariableReferenceExpression>(Is.VarRefNamed("localVar"))
                .Assert());

            result = testObject.AssignLocal("otherVar", true);
            Assert.AreSame(method, result);
            Assert.AreEqual(2, method.Statements.Count);

            Assert.IsNotEmpty(method.StatementsOfType<CodeVariableDeclarationStatement>().Where(e => e.Name == "otherVar"));
            Assert.IsEmpty(method.StatementsOfType<CodeVariableDeclarationStatement>().Where(e => e.InitExpression != null));

            // Todo: that on a assignment, but here is nothing assigned to the InitExpression of the CodeVariableDeclarationStatement.
            //AssertEx.That(method.StatementsOfType<CodeVariableDeclarationStatement>()
            //    .Where().Expression<CodePrimitiveExpression>(Is.Primitve("otherVar"))
            //    .IsEmpty() / .IsNull() / .IsNotEmpty() .Times(3) .Times(1,10)
            //  .Assert()); <- .IsNotEmpty()
            // successCount in FluentCodeStatementBinder<T>
        }

        [Test()]
        public void CommitTest()
        {
            var result = testObject.Invoke("MethodName").With("Thisone").Commit();
            //testObject.Invoke("Arsch").With("Thisone").Commit();
            //testObject.Invoke("Maulaff").With("Thisone").Commit();
            //testObject.Invoke("Nase").With("Thisone").Commit();
            Assert.AreSame(method, result);
            Assert.IsNotEmpty(method.Statements);
            Assert.AreEqual(1, method.Statements.Count);

            //AssertEx.That(method.ContainsStatement<CodeExpressionStatement>(), "Attribute 'Test' not found in: {0}", method.ContainsAttributeMsg());
            //AssertEx.That(method.ContainsStatement<CodeMethodInvokeExpression>());
            //AssertEx.That(method.ContainsStatement<CodeExpressionStatement>().Where().Commit());
            //AssertEx.That(method.StatementsOf<CodeExpressionStatement>().Where().Expressions<CodeMethodInvokeExpression>("bla").Commit());

            AssertEx.That(method.StatementsOfType<CodeExpressionStatement>()
                .Where().Expression<CodeMethodInvokeExpression>(Is.MethodNamed("MethodName"))
                .Assert());

            result = testObject.Invoke("OtherMethodName").With("None").Commit();
            Assert.AreSame(method, result);
            Assert.AreEqual(2, method.Statements.Count);

            AssertEx.That(method.StatementsOfType<CodeExpressionStatement>()
                .Where().Expression<CodeMethodInvokeExpression>(Is.MethodNamed("MethodName"))
                .Assert());

            AssertEx.That(method.StatementsOfType<CodeExpressionStatement>()
                .Where().Expression<CodeMethodInvokeExpression>(Is.MethodNamed("OtherMethodName"))
                .Assert());

            testObject.Invoke("WithoutCommit").With("None");

            AssertEx.That(method.StatementsOfType<CodeExpressionStatement>()
                .Where().Expression<CodeMethodInvokeExpression>(Is.MethodNamed("WithoutCommit"))
                .Assert());

            //method.StatementsOf<CodeExpressionStatement>().Where();
            // Assert.AreSame(testObject.TypeReference, actualCodeExpression.Expression);
        }
        
        [Test()]
        public void InvokeTest()
        {
            var result = testObject.Invoke("MethodName");
            Assert.AreSame(testObject, result);
            Assert.IsEmpty(method.Statements);
        }

        [Test()]
        public void WithWithoutPreceedingInvokeShouldThrow()
        {
            Assert.Throws<CodeTypeReferenceException>(() => testObject.With("Thisone"));
            Assert.IsEmpty(method.Statements);
        }

        [Test()]
        public void WithTest()
        {
            var result = testObject.Invoke("MethodName").With("Thisone");
            Assert.AreSame(testObject, result);
            Assert.IsEmpty(method.Statements);
        }
    }
}
