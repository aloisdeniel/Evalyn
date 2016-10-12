using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class ClassWalker : WalkerBase
	{
		public ClassWalker(Context context) : base(SyntaxKind.ClassDeclaration, context)
		{
			
		}

		private void VisitConstructorOrMethodDeclaration(SyntaxNode node)
		{
			var walker = new MethodWalker(this.Context);
			var body = walker.BuildWithResult(node);

			var symbol = this.SemanticModel.GetDeclaredSymbol(node) as IMethodSymbol;
			var method = this.Context.ResultAssembly.GetEvalMethod(symbol);
			method.Compile(body);
		}

		public override void VisitConstructorDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.ConstructorDeclarationSyntax node)
		{
			this.VisitConstructorOrMethodDeclaration(node);
		}

		public override void VisitMethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax node)
		{
			this.VisitConstructorOrMethodDeclaration(node);
		}

		public override void VisitPropertyDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.PropertyDeclarationSyntax node)
		{
			var walker = new PropertyWalker(this.Context);
			walker.Build(node);
		}
	}
}
