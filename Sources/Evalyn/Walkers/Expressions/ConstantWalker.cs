using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class ConstantWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.StringLiteralExpression,
			SyntaxKind.CharacterLiteralExpression,
			SyntaxKind.NumericLiteralExpression,
			SyntaxKind.NullLiteralExpression,
			SyntaxKind.TrueLiteralExpression,
			SyntaxKind.FalseLiteralExpression,
			SyntaxKind.DefaultExpression,
		};

		public ConstantWalker(Context context) : base(SupportedStatements,context)
		{
		}

		public override void VisitDefaultExpression(Microsoft.CodeAnalysis.CSharp.Syntax.DefaultExpressionSyntax node)
		{
			var typeinfo = this.SemanticModel.GetTypeInfo(node.Type);
			var type = this.Context.ResultAssembly.GetEvalType(typeinfo.Type).Runtime;
			this.Result = Expression.Default(type);
		}

		public override void VisitLiteralExpression(Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax node)
		{
			var value = this.SemanticModel.GetConstantValue(node);
			this.Result = Expression.Constant(value.Value);
		}
	}
}
