using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class UnaryWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.LogicalNotExpression,
			SyntaxKind.PreIncrementExpression,
			SyntaxKind.PreDecrementExpression,
			SyntaxKind.PostIncrementExpression,
			SyntaxKind.PostDecrementExpression,
			SyntaxKind.UnaryPlusExpression,
			SyntaxKind.UnaryMinusExpression,
			SyntaxKind.CastExpression,
		};

		public UnaryWalker(Context context) : base(SupportedStatements, context)
		{
			
		}

		public override void VisitPrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax node)
		{
			var operand = new ExpressionWalker(this.Context).BuildWithResult(node.Operand);

			switch (node.Kind())
			{
				case SyntaxKind.LogicalNotExpression:
					this.Result = Expression.Not(operand);
					break;
					
				case SyntaxKind.UnaryPlusExpression:
					this.Result = Expression.UnaryPlus(operand);
					break;
					
				case SyntaxKind.UnaryMinusExpression:
					this.Result = Expression.Negate(operand);
					break;
					
				case SyntaxKind.PreIncrementExpression:
					this.Result = Expression.PreIncrementAssign(operand);
					break;
					
				case SyntaxKind.PreDecrementExpression:
					this.Result = Expression.PreDecrementAssign(operand);
					break;

				default:
					break;
			}
		}

		public override void VisitCastExpression(Microsoft.CodeAnalysis.CSharp.Syntax.CastExpressionSyntax node)
		{
			var typeinfo = this.SemanticModel.GetTypeInfo(node.Type);
			var type = this.Context.ResultAssembly.GetEvalType(typeinfo.Type);

			var operand = new ExpressionWalker(this.Context).BuildWithResult(node.Expression);
			this.Result = Expression.ConvertChecked(operand,type.Runtime);
		}

		public override void VisitPostfixUnaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax node)
		{
			var operand = new ExpressionWalker(this.Context).BuildWithResult(node.Operand);

			switch (node.Kind())
			{
				case SyntaxKind.PostIncrementExpression:
					this.Result = Expression.PostIncrementAssign(operand);
					break;

				case SyntaxKind.PostDecrementExpression:
					this.Result = Expression.PreIncrementAssign(operand);
					break;

				default:
					break;
			}
		}

	}
}
