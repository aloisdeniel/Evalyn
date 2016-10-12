using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class Interpreter
	{

		public class PrintWalker : CSharpSyntaxWalker
		{
			static int Tabs = 0;
			string indents;
			public override void Visit(SyntaxNode node)
			{
				Tabs++;
				indents = new String('\t', Tabs);
				Debug.WriteLine(indents + node.Kind() + "("+node.GetType().Name+")");
				base.Visit(node);
				Tabs--;
			}
		}

		public Interpreter(string[] assemblyReferences = null)
		{
			this.references = assemblyReferences.Select(a => MetadataReference.CreateFromFile (a)).ToArray();
		}

		readonly MetadataReference[] references;

		public Context CreateContext(string code)
		{
			var tree = CSharpSyntaxTree.ParseText(code);

			var compilation = CSharpCompilation.Create(
					"Generated." + Guid.NewGuid(),
					syntaxTrees: new[] { tree },
					references: this.references,
					options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
			
			var semanticModel = compilation.GetSemanticModel(tree);

			var root = tree.GetRoot();
			new PrintWalker().Visit(root);

			using (var ms = new MemoryStream())
			{
				var result = compilation.Emit(ms);

				if (result.Success)
				{
					return new Context(compilation.Assembly,compilation.GetSemanticModel(tree)) { DebugTree = tree };
				}

				throw new InvalidOperationException("Compilation failed");
			}
		}
	}
}
