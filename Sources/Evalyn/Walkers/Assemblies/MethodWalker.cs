using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class MethodWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.MethodDeclaration,
			SyntaxKind.ConstructorDeclaration,
		};

		public MethodWalker(Context context) : base(SupportedStatements, context)
		{
			
		}

		public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
		{
			this.VisitMethodOrConstructor(node, node.Body);
		}

		public override void VisitMethodDeclaration(Microsoft.CodeAnalysis.CSharp.Syntax.MethodDeclarationSyntax node)
		{
			this.VisitMethodOrConstructor(node, node.Body, node.ExpressionBody, node.ReturnType);
		}

		private void VisitMethodOrConstructor(SyntaxNode node, BlockSyntax bodySyntax, ArrowExpressionClauseSyntax expressionBodySyntax = null, TypeSyntax returnTypeSyntax = null)
		{
			var symbol = this.SemanticModel.GetDeclaredSymbol(node) as IMethodSymbol;
			var method = this.Context.ResultAssembly.GetEvalMethod(symbol);

			var name = method.Name;
			var typeinfo = returnTypeSyntax != null ? this.SemanticModel.GetTypeInfo(returnTypeSyntax).Type : null;
			var returnType = this.Context.ResultAssembly.GetEvalType(typeinfo);

			this.Context.PushLocals();

			//Creating return label from return type

			LabelTarget returnTarget;

			if (returnType == null)
			{
				returnTarget = Expression.Label();
			}
			else if(returnType.IsRuntime())
			{
				returnTarget = Expression.Label(returnType.Runtime);
			}
			else
			{
				returnTarget = Expression.Label(typeof(EvalInstance));
			}

			// Adding parameters to locals

			foreach (var param in method.Parameters)
			{
				this.Context.Locals[param.Name] = param.Expression;
			}

			if (!method.IsStatic)
			{
				this.Context.This = method.Parameters.First().Expression;
			}

			// Body

			Expression body = null;

			if (bodySyntax != null)
			{
				var blockWalker = new BlockWalker(returnTarget, this.Context, true);
				body = blockWalker.BuildWithResult(bodySyntax);
			}
			else if (expressionBodySyntax?.Expression != null)
			{
				var blockWalker = new ExpressionWalker(this.Context);
				body = blockWalker.BuildWithResult(expressionBodySyntax.Expression);
			}

			this.Context.PopLocals();

			this.Result = body;

			if (!method.IsStatic)
			{
				this.Context.This = null;
			}
		}
	}
}
