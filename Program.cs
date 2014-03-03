using Roslyn.Compilers;
using Roslyn.Compilers.CSharp;
using Roslyn.Scripting.CSharp;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace RoslynSample
{
    class Walker : SyntaxWalker
    {
        public override void Visit(SyntaxNode node)
        {
            if (node != null)
                Console.WriteLine("[Node  - Type: {0}, Kind: {1}]\n{2}\n", node.GetType().Name, node.Kind, node);

            base.Visit(node);
        }
    }

    class Program
    {
        static Func<int, int, int> AddByRoslynScriptEngine()
        {
            var engine = new ScriptEngine();
            var session = engine.CreateSession();
            session.ImportNamespace("System");

            return (Func<int, int, int>)session.Execute(code: "(Func<int, int, int>)((x, y) => x + y)");
        }

        static void TestScriptEngine()
        {
            var addFunc = AddByRoslynScriptEngine();
            System.Console.WriteLine(addFunc(120, 80));
        }

        static void TestVectorGenerator()
        {
            var syntaxTree = SyntaxTree.ParseFile("../../vector_test.cs");
            var rootNode = syntaxTree.GetRoot();
            var generator = new VectorGenerator();
            var newRootNode = generator.Generate(rootNode);
            Console.WriteLine(newRootNode.NormalizeWhitespace());
        }

        static void TestCallAnalysis()
        {
            var tree = SyntaxTree.ParseFile("../../semantics_test.cs");
            var analyzer = new CallAnalyzer(tree);
            analyzer.Analyze("FakeMain");
        }

        static void Main(string[] args)
        {
            //TestScriptEngine();
            TestCallAnalysis();
            //TestVectorGenerator();

//            new Walker().Visit(rootNode);
        }
    }
}
