using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TestGenerator.CSCodeDataStructures;

namespace TestGenerator.CSCodeAnalyzer
{
    public class CSCodeAnalyzer : ICSCodeAnalyzer
    {
        public CSCodeAnalyzer()
        {
        }

        public FileInformation AnalyzeCode(string code)
        {
            FileInformation fileInformation = new FileInformation();
            CompilationUnitSyntax compilationUnit = CSharpSyntaxTree.ParseText(code).GetCompilationUnitRoot();
            foreach (UsingDirectiveSyntax usingDeclaration in compilationUnit.Usings)
                fileInformation.UsingsDeclaration.Add(usingDeclaration.Name.ToString());
            foreach (ClassDeclarationSyntax classDeclaration in 
                compilationUnit.DescendantNodes().OfType<ClassDeclarationSyntax>())
                fileInformation.ClassesDeclaration.Add(CreateClassInformation(classDeclaration));
            return fileInformation;
        }

        internal MethodInformation CreateMethodInformation(MethodDeclarationSyntax methodDeclaration)
        {
            MethodInformation result = 
                new MethodInformation(methodDeclaration.Identifier.ValueText, 
                new TypeInformation(methodDeclaration.ReturnType.ToString()));
            foreach (ParameterSyntax parameter in methodDeclaration.ParameterList.Parameters)
                result.MethodParameters.Add(new ParameterInformation(parameter.Identifier.ValueText, 
                    new TypeInformation(parameter.Type.ToString())));
            return result;
        }

        internal ConstructorInformation GetExtendedConstructor(ClassDeclarationSyntax classDeclaration)
        {
            ConstructorInformation result = new ConstructorInformation();
            ConstructorDeclarationSyntax extendConstructorDeclaration = classDeclaration.DescendantNodes()
                .OfType<ConstructorDeclarationSyntax>()
                .Where((constructor) => constructor.Modifiers.Any((modifier) => modifier.IsKind(SyntaxKind.PublicKeyword)))
                .OrderByDescending((constructor) => constructor.ParameterList.Parameters.Count)
                .FirstOrDefault();
            if (extendConstructorDeclaration != null)
                foreach (ParameterSyntax parameter in extendConstructorDeclaration.ParameterList.Parameters)
                    result.ParametersDeclaration.Add(new ParameterInformation(parameter.Identifier.ValueText, 
                        new TypeInformation(parameter.Type.ToString())));
            return result;
        }

        internal ClassInformation CreateClassInformation(ClassDeclarationSyntax classDeclaration)
        {
            ClassInformation classInformation =
                new ClassInformation(classDeclaration.Identifier.ValueText,
                ((NamespaceDeclarationSyntax)classDeclaration.Parent).Name.ToString(),
                GetExtendedConstructor(classDeclaration));
            foreach (MethodDeclarationSyntax methodDeclaration in classDeclaration.DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where((methodDeclaration) => methodDeclaration.Modifiers.Any((modifier) =>
                modifier.IsKind(SyntaxKind.PublicKeyword))))
            {
                classInformation.MethodsDeclaration.Add(CreateMethodInformation(methodDeclaration));
            }
            return classInformation;
        }
    }
}
