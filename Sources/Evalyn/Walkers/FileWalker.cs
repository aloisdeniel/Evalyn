using System;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class FileWalker : WalkerBase
	{
		public FileWalker(Context context) : base(SyntaxKind.CompilationUnit, context)
		{


		}

		public override void VisitClassDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ClassDeclarationSyntax node)
		{
			var walker = new ClassWalker(this.Context);
			walker.Build(node);
		}
	}
}
