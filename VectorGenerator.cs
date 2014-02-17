using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace RoslynSample
{
    public class VectorGenerator
    {
        public VectorGenerator()
        {
        }

        const string VectorPrefix = "__Vector";

        public SyntaxNode Generate(SyntaxNode node)
        {
            // Find vector class
            while (true)
            {
                ClassDeclarationSyntax vectorNode;
                try
                {
                    vectorNode = node.DescendantNodes().OfType<ClassDeclarationSyntax>().First(tnode => tnode.Identifier.ValueText.IndexOf(VectorPrefix) >= 0);
                }
                catch (InvalidOperationException)
                {
                    break;
                }

                var extractedNode = ExtractVectorClass(vectorNode);
                node = node.ReplaceNode(vectorNode, extractedNode);
            }

            return node;
        }

        static readonly string[] fieldIds = new string[] { "X", "Y", "Z", "W" };

        MethodDeclarationSyntax GenerateVectorMethod(string name, TypeSyntax returnType, ClassDeclarationSyntax vectorClass, int numElements, SyntaxKind binaryKind)
        {
            var method = Syntax.MethodDeclaration(returnType, name);
            var type = Syntax.ParseTypeName(vectorClass.Identifier.ValueText);
            var identifier = Syntax.Identifier("v");
            var p = Syntax.Parameter(
                new SyntaxList<AttributeListSyntax>(),
                new SyntaxTokenList(),
                type,
                identifier,
                null);
            method = method.AddParameterListParameters(p);

            // Generate add method
            for (int i = 0; i < numElements; i++)
            {
                var addStatement = Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(SyntaxKind.AssignExpression, Syntax.IdentifierName(fieldIds[i]),
                    Syntax.BinaryExpression(binaryKind, Syntax.IdentifierName(fieldIds[i]),
                    Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, Syntax.IdentifierName("v"), Syntax.IdentifierName(fieldIds[i])))));

                method = method.AddBodyStatements(addStatement);
            }
            return method;
        }

        SyntaxNode ExtractVectorClass(ClassDeclarationSyntax vectorNode)
        {
            // Find vector class

            int numElements = vectorNode.Identifier.ValueText[VectorPrefix.Length] - '0';
            TypeSyntax elementType = null;// SyntaxKind elementType = SyntaxKind.None;
            switch (vectorNode.Identifier.ValueText[VectorPrefix.Length + 1])
            {
                case 'f': elementType = Syntax.PredefinedType(Syntax.Token(SyntaxKind.FloatKeyword)); break;
                case 'i': elementType = Syntax.PredefinedType(Syntax.Token(SyntaxKind.IntKeyword)); break;
            }

            // Extract vector generation parameter
            var vectorClass = Syntax.ClassDeclaration("Vector" + vectorNode.Identifier.ValueText.Substring(VectorPrefix.Length, 2));

            for (int i = 0; i < numElements; i++)
            {
                var fieldId = fieldIds[i];
                var field = Syntax.FieldDeclaration(
                    Syntax.VariableDeclaration(elementType))
                    .WithModifiers(Syntax.Token(SyntaxKind.PublicKeyword))
                    .AddDeclarationVariables(Syntax.VariableDeclarator(fieldId));

                vectorClass = vectorClass.AddMembers(field);
            }

            // Generate constructor
            var constructor = Syntax.ConstructorDeclaration(vectorClass.Identifier.ValueText);
            for (int i = 0; i < numElements; i++)
            {
                //var type = Syntax.ParseTypeName(vectorClass.Identifier.ValueText);
                var paramId = Syntax.Identifier(fieldIds[i].ToLower());
                var p = Syntax.Parameter(
                    new SyntaxList<AttributeListSyntax>(),
                    new SyntaxTokenList(),
                    elementType,
                    paramId,
                    null);
                constructor = constructor.AddParameterListParameters(p);
                constructor = constructor.AddBodyStatements(
                    Syntax.ExpressionStatement(
                    Syntax.BinaryExpression(SyntaxKind.AssignExpression, Syntax.IdentifierName(fieldIds[i]), Syntax.IdentifierName(paramId))));
            }
            vectorClass = vectorClass.AddMembers(constructor.WithModifiers(Syntax.Token(SyntaxKind.PublicKeyword)));

            var voidType = Syntax.PredefinedType(Syntax.Token(SyntaxKind.VoidKeyword));
            // Generate add method
            var addMethod = GenerateVectorMethod("Add", voidType, vectorClass, numElements, SyntaxKind.AddExpression);
            vectorClass = vectorClass.AddMembers(addMethod);

            var subtractMethod = GenerateVectorMethod("Sub", voidType, vectorClass, numElements, SyntaxKind.SubtractExpression);
            vectorClass = vectorClass.AddMembers(subtractMethod);

            return vectorClass;
        }
    }
}
