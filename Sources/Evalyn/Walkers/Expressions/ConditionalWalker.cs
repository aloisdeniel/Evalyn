using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class ConditionalWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.ConditionalExpression,
		};

		public ConditionalWalker(Context context) : base(SupportedStatements,context)
		{
		}

		public override void VisitConditionalExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax node)
		{
			var condition = new ExpressionWalker(this.Context).BuildWithResult(node.Condition);
			var whenTrue = new ExpressionWalker(this.Context).BuildWithResult(node.WhenTrue);
			var whenFalse = new ExpressionWalker(this.Context).BuildWithResult(node.WhenFalse);

			this.Result = Expression.Condition(condition,whenTrue,whenFalse);
		}
	}
}
