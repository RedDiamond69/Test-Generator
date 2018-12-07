using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TestGenerator.CSCodeAnalyzer;
using TestGenerator.CSCodeDataStructures;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestGenerator.PatternGenerator
{
    public class PatternGenerator : IPatternGenerator
    {
        private const string _actual = "actual";
        private const string _expected = "expected";
        private readonly ICSCodeAnalyzer _csCodeAnalyzer;
        private readonly SyntaxToken _emptyLineToken;
        private readonly ExpressionStatementSyntax _expressionStatementSyntax;

        public IEnumerable<PathInformation> GenerateCode(string source)
        {
            if (source == null)
                throw new ArgumentException("Source can't be null!");
            List<PathInformation> resultList = new List<PathInformation>();
            FileInformation fileInformation = _csCodeAnalyzer.AnalyzeCode(source);
            List<UsingDirectiveSyntax> usings = 
                fileInformation.UsingsDeclaration.Select((usingStr) => 
                UsingDirective(IdentifierName(usingStr))).ToList();
            usings.Add(UsingDirective(IdentifierName("Moq")));
            usings.Add(UsingDirective(IdentifierName("Microsoft.VisualStudio.TestTools.UnitTesting")));
            foreach (ClassInformation classInfo in fileInformation.ClassesDeclaration)
            {
                resultList.Add(new PathInformation(classInfo.NamesDeclaration + "Test.cs", CompilationUnit()
                    .WithUsings(
                        List(
                            CreateUsings(classInfo, usings)))
                    .WithMembers(
                        SingletonList(
                            CreateTestClassWithNamespaceDeclaration(classInfo)))
                    .NormalizeWhitespace().ToFullString()));
            }
            return resultList;
        }

        protected UsingDirectiveSyntax[] CreateUsings(ClassInformation classInformation, List<UsingDirectiveSyntax> fileUsings)
        {
            return new List<UsingDirectiveSyntax>(fileUsings)
            {
                UsingDirective(IdentifierName(classInformation.NamespacesDeclaration))
            }.ToArray();
        }

        protected MemberDeclarationSyntax CreateTestClassWithNamespaceDeclaration(ClassInformation classInformation)
        {
            return NamespaceDeclaration(
                IdentifierName(classInformation.NamespacesDeclaration + ".Test"))
            .WithMembers(
                SingletonList<MemberDeclarationSyntax>(
                    CreateClassDeclaration(classInformation)));
        }

        protected ClassDeclarationSyntax CreateClassDeclaration(ClassInformation classInformation)
        {
            return ClassDeclaration(classInformation.NamesDeclaration + "Test")
            .WithAttributeLists(
                SingletonList(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("TestClass"))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithMembers(
                List<MemberDeclarationSyntax>(
                    new MemberDeclarationSyntax[]{
                        CreateTestClassInstanceFieldDeclaration(classInformation).WithSemicolonToken(_emptyLineToken),
                        CreateTestInitializeMethodDeclaration(classInformation)
                    }.Concat(classInformation.MethodsDeclaration.Select(
                        (methodInfo) => CreateTestMethodDeclaration(methodInfo, classInformation)))));
        }

        protected MethodDeclarationSyntax CreateTestMethodDeclaration(MethodInformation methodInformation, ClassInformation classInformation)
        {
            List<LocalDeclarationStatementSyntax> body = methodInformation.MethodParameters.Select(
                (parameter) => CreateVariableInitializeExpression(parameter)).ToList();
            if (body.Count != 0)
                body[body.Count - 1] = body[body.Count - 1].WithSemicolonToken(_emptyLineToken);
            List<StatementSyntax> assertBody = new List<StatementSyntax>();
            if (methodInformation.MethodReturnType.Typename != "void")
            {
                assertBody.Add(CreateActualDeclaration(methodInformation, classInformation).WithSemicolonToken(_emptyLineToken));
                assertBody.Add(CreateExpectedDeclaration(methodInformation.MethodReturnType));
                assertBody.Add(CreateAreEqualExpression(methodInformation.MethodReturnType));
            }
            assertBody.Add(_expressionStatementSyntax);
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.VoidKeyword)),
                Identifier(methodInformation.MethodName + "Test"))
            .WithAttributeLists(
                SingletonList(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("TestMethod"))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithBody(
                Block(
                    body.Concat(assertBody)));
        }

        protected string CreateVariableName(string parameterName, bool isTestInstance = false) => char.ToLower(
            parameterName[0]) + (parameterName.Length == 1 ? string.Empty : parameterName.Substring(1)) + 
            (isTestInstance ? "TestInstance" : string.Empty);

        protected FieldDeclarationSyntax CreateTestClassInstanceFieldDeclaration(ClassInformation classInformation)
        {
            return FieldDeclaration(
                VariableDeclaration(
                    IdentifierName(classInformation.NamesDeclaration))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(CreateVariableName(classInformation.NamesDeclaration, true))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PrivateKeyword)));
        }

        protected MethodDeclarationSyntax CreateTestInitializeMethodDeclaration(ClassInformation classInformation)
        {
            return MethodDeclaration(
                PredefinedType(
                    Token(SyntaxKind.VoidKeyword)),
                Identifier("TestInitialize"))
            .WithAttributeLists(
                SingletonList(
                    AttributeList(
                        SingletonSeparatedList(
                            Attribute(
                                IdentifierName("TestInitialize"))))))
            .WithModifiers(
                TokenList(
                    Token(SyntaxKind.PublicKeyword)))
            .WithBody(
                Block(
                    classInformation.ConstructorsDeclaration.ParametersDeclaration.Select(
                        (parameter) => CreateVariableInitializeExpression(parameter))
                        .Concat(new StatementSyntax[] { CreateTestClassInitializeExpression(classInformation) })
                    ));
        }

        protected LocalDeclarationStatementSyntax CreateVariableInitializeExpression(ParameterInformation parameterInformation)
        {
            ExpressionSyntax initializer;
            if (parameterInformation.Type.IsInterface)
            {
                initializer = ObjectCreationExpression(
                    GenericName(
                        Identifier("Mock"))
                    .WithTypeArgumentList(
                        TypeArgumentList(
                            SingletonSeparatedList<TypeSyntax>(
                                IdentifierName(parameterInformation.Type.Typename)))))
                .WithArgumentList(
                    ArgumentList());
            }
            else
                initializer = DefaultExpression(IdentifierName(parameterInformation.Type.Typename));
            return LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName(parameterInformation.Type.Typename))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(CreateVariableName(parameterInformation.Name)))
                        .WithInitializer(
                            EqualsValueClause(
                                initializer)))));
        }

        protected ExpressionStatementSyntax CreateTestClassInitializeExpression(ClassInformation classInformation)
        {
            return ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(CreateVariableName(classInformation.NamesDeclaration, true)),
                    ObjectCreationExpression(
                        IdentifierName(classInformation.NamesDeclaration))
                    .WithArgumentList(
                        ArgumentList(
                            SeparatedList<ArgumentSyntax>(
                                CreateArguments(classInformation.ConstructorsDeclaration.ParametersDeclaration))))));
        }

        protected List<SyntaxNodeOrToken> CreateArguments(IList<ParameterInformation> parametersList)
        {
            SyntaxToken commaToken = Token(SyntaxKind.CommaToken);
            List<SyntaxNodeOrToken> arguments = new List<SyntaxNodeOrToken>();
            if (parametersList.Count > 0)
                arguments.Add(CreateArgument(parametersList[0]));
            for (int i = 1; i < parametersList.Count; ++i)
            {
                arguments.Add(commaToken);
                arguments.Add(CreateArgument(parametersList[i]));
            }
            return arguments;
        }

        protected SyntaxNodeOrToken CreateArgument(ParameterInformation parameterInfo)
        {
            if (parameterInfo.Type.IsInterface)
                return Argument(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(CreateVariableName(parameterInfo.Name)),
                        IdentifierName("Object")));
            else
                return Argument(IdentifierName(CreateVariableName(parameterInfo.Name)));
        }

        protected LocalDeclarationStatementSyntax CreateActualDeclaration(MethodInformation methodInformation, ClassInformation classInformation)
        {
            return LocalDeclarationStatement(
                VariableDeclaration(
                    IdentifierName(methodInformation.MethodReturnType.Typename))
                .WithVariables(
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(_actual))
                        .WithInitializer(
                            EqualsValueClause(
                                InvocationExpression(
                                    MemberAccessExpression(
                                        SyntaxKind.SimpleMemberAccessExpression,
                                        IdentifierName(CreateVariableName(classInformation.NamesDeclaration, true)),
                                        IdentifierName(methodInformation.MethodName)))
                                .WithArgumentList(
                                    ArgumentList(
                                        SeparatedList<ArgumentSyntax>(
                                            CreateArguments(methodInformation.MethodParameters)))))))));
        }

        protected LocalDeclarationStatementSyntax CreateExpectedDeclaration(TypeInformation methodReturnType) => 
            CreateVariableInitializeExpression(new ParameterInformation(_expected, methodReturnType));

        protected ExpressionStatementSyntax CreateAreEqualExpression(TypeInformation methodReturnType)
        {
            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Assert"),
                        IdentifierName("AreEqual")))
                .WithArgumentList(
                    ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            new SyntaxNodeOrToken[]{
                                CreateArgument(new ParameterInformation(_expected, methodReturnType)),
                                Token(SyntaxKind.CommaToken),
                                Argument(IdentifierName(_actual))}))));
        }

        protected ExpressionStatementSyntax CreateFailExpression()
        {
            return ExpressionStatement(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Assert"),
                        IdentifierName("Fail")))
                .WithArgumentList(
                    ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                            Argument(
                                LiteralExpression(
                                    SyntaxKind.StringLiteralExpression,
                                    Literal("autogenerated")))))));
        }

        protected SyntaxToken CreateEmptyLineToken()
        {
            return Token(
                TriviaList(),
                SyntaxKind.SemicolonToken,
                TriviaList(
                    Trivia(
                        SkippedTokensTrivia()
                        .WithTokens(
                            TokenList(
                                BadToken(
                                    TriviaList(),
                                    "\n",
                                    TriviaList()))))));
        }

        public PatternGenerator(ICSCodeAnalyzer csCodeAnalyzer)
        {
            _csCodeAnalyzer = csCodeAnalyzer ?? throw new ArgumentException("Code analyzer cadn't be null!");
            _emptyLineToken = CreateEmptyLineToken();
            _expressionStatementSyntax = CreateFailExpression();
        }
    }
}