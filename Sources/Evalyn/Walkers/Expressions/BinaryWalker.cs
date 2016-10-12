using System;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class BinaryWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.MultiplyExpression,
			SyntaxKind.AddExpression,
			SyntaxKind.DivideExpression,
			SyntaxKind.ModuloExpression,
			SyntaxKind.LogicalAndExpression,
			SyntaxKind.LogicalOrExpression,
			SyntaxKind.EqualsExpression,
			SyntaxKind.NotEqualsExpression,
			SyntaxKind.LessThanExpression,
			SyntaxKind.LessThanOrEqualExpression,
			SyntaxKind.GreaterThanExpression,
			SyntaxKind.GreaterThanOrEqualExpression,
			SyntaxKind.LeftShiftExpression,
			SyntaxKind.RightShiftExpression,
			SyntaxKind.ExclusiveOrExpression,
			SyntaxKind.CoalesceExpression,
		};

		public BinaryWalker(Context context) : base(SupportedStatements, context)
		{
			
		}

		private Expression ConvertToString(Expression exp)
		{
			var toString = exp.Type.GetRuntimeMethod(nameof(string.ToString), new Type[0]);

			if (!exp.Type.GetTypeInfo().IsValueType)
			{
				var constantNull = Expression.Constant(null, exp.Type);
				var condition = Expression.Equal(exp, constantNull);
				return Expression.Condition (condition, Expression.Constant(string.Empty, typeof(string)), Expression.Call(exp, toString));
			}

			return Expression.Call(exp, toString);
		}

		private Expression CreateConcat(Expression left, Expression right)
		{
			var concat = typeof(string).GetRuntimeMethod(nameof(string.Concat), new Type[] { typeof(string), typeof(string) });
			return Expression.Call(concat, left, right);
		}

		public override void VisitBinaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax node)
		{
			var left = new ExpressionWalker(this.Context).BuildWithResult(node.Left);
			var right = new ExpressionWalker(this.Context).BuildWithResult(node.Right);

			switch (node.Kind())
			{
				case SyntaxKind.MultiplyExpression:
					this.Result = Expression.Multiply(left,right);
					break;
					
				case SyntaxKind.AddExpression:

					// Generates string concatenation from addition (no + operation overload is available from string)
					if (left.Type == typeof(string) || left.Type == typeof(string))
					{
						if (left.Type != typeof(string)) left = ConvertToString(left);
						if (right.Type != typeof(string)) right = ConvertToString(right);

						this.Result = CreateConcat(left, right);
					}
					else
					{
						this.Result = Expression.Add(left, right);
					}

					break;
					
				// Maths operators
					
				case SyntaxKind.DivideExpression:
					this.Result = Expression.Divide(left, right);
					break;

				case SyntaxKind.ModuloExpression:
					this.Result = Expression.Modulo(left, right);
					break;

				// Logic
					
				case SyntaxKind.LogicalAndExpression:
					this.Result = Expression.And(left, right);
					break;

				case SyntaxKind.LogicalOrExpression:
					this.Result = Expression.Or(left, right);
					break;

				// Bit comparators

				case SyntaxKind.LeftShiftExpression:
					this.Result = Expression.LeftShift(left, right);
					break;

				case SyntaxKind.RightShiftExpression:
					this.Result = Expression.RightShift(left, right);
					break;

				case SyntaxKind.ExclusiveOrExpression:
					this.Result = Expression.ExclusiveOr(left, right);
					break;
					
				// Maths comparators

				case SyntaxKind.EqualsExpression:
					this.Result = Expression.Equal(left, right);
					break;

				case SyntaxKind.NotEqualsExpression:
					this.Result = Expression.NotEqual(left, right);
					break;
					
				case SyntaxKind.LessThanExpression:
					this.Result = Expression.LessThan(left, right);
					break;

				case SyntaxKind.LessThanOrEqualExpression:
					this.Result = Expression.LessThanOrEqual(left, right);
					break;

				case SyntaxKind.GreaterThanExpression:
					this.Result = Expression.GreaterThan(left, right);
					break;

				case SyntaxKind.GreaterThanOrEqualExpression:
					this.Result = Expression.GreaterThanOrEqual(left, right);
					break;

				// Other

				case SyntaxKind.CoalesceExpression:
					this.Result = Expression.Coalesce(left, right);
					break;
					
				default:
					break;
			}
		}
	}
}
