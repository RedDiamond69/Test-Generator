using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestGenerator;
using TestGenerator.AsyncReaderWriter.Writer;

namespace TestGeneratorUnitTests
{
    [TestClass]
    public class TestGeneratorUnitTests
    {
        private CompilationUnitSyntax class1Root, class2Root;

        [TestInitialize]
        public void TestInit()
        {
            string directory = "..\\..\\";
            string testFilePath = directory + "NotSimpleClass.cs";
            string testClass1FilePath = directory + "NotSimpleClass1Test.cs";
            string testClass2FilePath = directory + "NotSimpleClass2Test.cs";

            GeneratorConfig config = new GeneratorConfig
            {
                Paths = new List<string>
                {
                    testFilePath
                },
                AsyncWriter = new AsyncFileDataWriter()
                {
                    Directory = directory
                }
            };
            new TestGenerator.TestGenerator(config).Generate().Wait();
            class1Root = CSharpSyntaxTree.ParseText(File.ReadAllText(testClass1FilePath)).GetCompilationUnitRoot();
            class2Root = CSharpSyntaxTree.ParseText(File.ReadAllText(testClass2FilePath)).GetCompilationUnitRoot();
        }

        [TestMethod]
        public void ExceptionThrowingTest()
        {
            GeneratorConfig config = new GeneratorConfig
            {
                Paths = new List<string>
                {
                    "NonExistingFile.cs"
                }
            };
            Assert.ThrowsException<AggregateException>(() => new TestGenerator.TestGenerator(config).Generate().Wait());
        }

        [TestMethod]
        public void UsingTests()
        {
            List<string> expected = new List<string>
            {
                "Microsoft.VisualStudio.TestTools.UnitTesting",
                "Moq",
                "System",
                "TestGeneratorConsole.TestSourceFiles1"
            };
            CollectionAssert.IsSubsetOf(expected, class1Root.Usings.Select(x => x.Name.ToString()).ToList());
            Assert.AreEqual(1, class2Root.Usings.Where((usingEntry) => usingEntry.Name.ToString() == "TestGeneratorConsole.TestSourceFiles2").Count());
        }

        [TestMethod]
        public void NamespaceTest()
        {
            IEnumerable<NamespaceDeclarationSyntax> namespaces;
            namespaces = class1Root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            Assert.AreEqual(1, namespaces.Count());
            Assert.AreEqual("TestGeneratorConsole.TestSourceFiles1.Test", namespaces.First().Name.ToString());
            namespaces = class2Root.DescendantNodes().OfType<NamespaceDeclarationSyntax>();
            Assert.AreEqual(1, namespaces.Count());
            Assert.AreEqual("TestGeneratorConsole.TestSourceFiles2.Test", namespaces.First().Name.ToString());
        }

        [TestMethod]
        public void ClassTest()
        {
            IEnumerable<ClassDeclarationSyntax> classes;
            classes = class1Root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            Assert.AreEqual(1, classes.Count());
            Assert.AreEqual("NotSimpleClass1Test", classes.First().Identifier.ToString());
            classes = class2Root.DescendantNodes().OfType<ClassDeclarationSyntax>();
            Assert.AreEqual(1, classes.Count());
            Assert.AreEqual("NotSimpleClass2Test", classes.First().Identifier.ToString());
        }

        [TestMethod]
        public void ClassAttributeTest()
        {
            Assert.AreEqual(1, class1Root.DescendantNodes().OfType<ClassDeclarationSyntax>()
                .Where((classDeclaration) => classDeclaration.AttributeLists.Any((attributeList) => attributeList.Attributes
                .Any((attribute) => attribute.Name.ToString() == "TestClass"))).Count());
        }

        [TestMethod]
        public void MethodsTest()
        {
            List<string> expected = new List<string>
            {
                "TestInitialize",
                "GetInterfaceTest",
                "SetInterfaceTest"
            };
            List<string> actual = class2Root.DescendantNodes().OfType<MethodDeclarationSyntax>().Select((method) => method.Identifier.ToString()).ToList();

            CollectionAssert.AreEquivalent(expected, class2Root.DescendantNodes().OfType<MethodDeclarationSyntax>().Select((method) => method.Identifier.ToString()).ToList());
            Assert.IsFalse(actual.Contains("GetProtectedInterfaceTest"));
        }

        [TestMethod]
        public void MethodAttributeTest()
        {
            IEnumerable<MethodDeclarationSyntax> methods = class2Root.DescendantNodes().OfType<MethodDeclarationSyntax>();

            Assert.AreEqual(2, methods.Where((methodDeclaration) => methodDeclaration.AttributeLists
                .Any((attributeList) => attributeList.Attributes.Any((attribute) => attribute.Name.ToString() == "TestMethod")))
                .Count());
            Assert.AreEqual(1, methods.Where((methodDeclaration) => methodDeclaration.AttributeLists
                .Any((attributeList) => attributeList.Attributes.Any((attribute) => attribute.Name.ToString() == "TestInitialize")))
                .Count());
        }

        [TestMethod]
        public void MockTest()
        {
            Assert.AreEqual(1, class2Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "TestInitialize").Body.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .Where((statement) => statement.ToFullString().Contains("new Mock")).Count());
        }

        [TestMethod]
        public void ClassInitTest()
        {
            Assert.AreEqual(1, class2Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "TestInitialize").Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Where((statement) => statement.ToFullString().Contains("new NotSimpleClass2")).Count());
        }

        [TestMethod]
        public void ActualTest()
        {
            Assert.AreEqual(1, class1Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "GetIntTest").Body.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .Where((statement) => statement.Declaration.Variables.Any((variable) => variable.Identifier.ToString() == "actual")).Count());
        }

        [TestMethod]
        public void ExpectedTest()
        {
            Assert.AreEqual(1, class1Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "GetIntTest").Body.Statements
                .OfType<LocalDeclarationStatementSyntax>()
                .Where((statement) => statement.Declaration.Variables.Any((variable) => variable.Identifier.ToString() == "expected")).Count());
        }

        [TestMethod]
        public void AreEqualTest()
        {
            Assert.AreEqual(1, class1Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "GetIntTest").Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Where((statement) => statement.ToString().Contains("Assert.AreEqual(expected, actual)")).Count());
        }

        [TestMethod]
        public void FailTest()
        {
            Assert.AreEqual(1, class1Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "GetIntTest").Body.Statements
                .OfType<ExpressionStatementSyntax>()
                .Where((statement) => statement.ToString().Contains("Assert.Fail")).Count());
        }

        [TestMethod]
        public void ArgumentsInitializationTest()
        {
            List<string> expected = new List<string>()
            {
                "param1",
                "param2"
            };
            List<string> actual = class1Root.DescendantNodes().OfType<MethodDeclarationSyntax>()
                .FirstOrDefault((method) => method.Identifier.ToString() == "DoSomethingTest").Body.Statements
                .OfType<LocalDeclarationStatementSyntax>().Select((declaration) => declaration.Declaration.Variables)
                .SelectMany((declaration) => declaration.ToList()).Select((variableDeclaration) => variableDeclaration.Identifier.ToString()).ToList();

            CollectionAssert.AreEquivalent(expected, actual);
        }
    }
}
