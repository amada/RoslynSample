using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;

namespace RoslynSample
{
    public class CallAnalyzer
    {
        public CallAnalyzer(SyntaxTree tree) { _tree = tree; }

        static void PrintNode(SyntaxNode node)
        {
            Console.WriteLine("[Node  - Type: {0}, Kind: {1}]\n{2}\n", node.GetType().Name, node.Kind, node);
        }


        SyntaxTree _tree;
        SemanticModel _model;
        void EnumerateAllMethodInvocation(SyntaxNode node, int level)
        {
            if (node.Kind == SyntaxKind.InvocationExpression)
            {
                //PrintNode(node);
                var invoke = node as InvocationExpressionSyntax;
                var member = invoke.Expression as MemberAccessExpressionSyntax;
                SymbolInfo info;
                if (member == null)
                {
                    info = _model.GetSymbolInfo(invoke);
                    for (int i = 0; i < level; i++) Console.Write("  ");
                    Console.WriteLine(info.Symbol);
                    foreach (var decl in info.Symbol.DeclaringSyntaxNodes)
                        EnumerateAllMethodInvocation(decl, level + 1);
                }
                else
                {
                    info = _model.GetSymbolInfo(member);
                    if (info.CandidateSymbols.Count > 0)
                    {
                        foreach (var method in info.CandidateSymbols)
                        {
                            for (int i = 0; i < level; i++) Console.Write("  ");
                            Console.WriteLine(method);
                            foreach (var decl in method.DeclaringSyntaxNodes)
                                EnumerateAllMethodInvocation(decl, level + 1);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < level; i++) Console.Write("  ");
                        if (info.Symbol.IsVirtual) Console.Write("virtual ");
                        Console.WriteLine(info.Symbol);
                        foreach (var decl in info.Symbol.DeclaringSyntaxNodes)
                            EnumerateAllMethodInvocation(decl, level + 1);
                    }
                }
            }
            foreach (var child in node.ChildNodes())
            {
                EnumerateAllMethodInvocation(child, level);
            }
        }

        public void Analyze(string methodName)
        {
            var tree = _tree;
            MethodDeclarationSyntax methodDecl = tree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First().ChildNodes()
                .OfType<MethodDeclarationSyntax>().First(node => node.Identifier.ValueText == methodName);

            Compilation compilation = Compilation.Create("SimpleMethod").AddSyntaxTrees(tree);

            var model = compilation.GetSemanticModel(tree);
            _model = model;

            Console.WriteLine(methodName); //_model.G.GetSymbolInfo(methodDecl.Identifier)); //.Identifier);
            foreach (var node in methodDecl.Body.ChildNodes())
            {
                EnumerateAllMethodInvocation(node, 1);
            }

            var methodSymbol = model.GetDeclaredSymbol(methodDecl);
        }
    }
}
