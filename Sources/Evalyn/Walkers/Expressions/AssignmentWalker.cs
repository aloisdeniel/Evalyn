using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class AssignmentWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.SimpleAssignmentExpression,
			SyntaxKind.OrAssignmentExpression,
			SyntaxKind.AndAssignmentExpression,
			SyntaxKind.AddAssignmentExpression,
			SyntaxKind.ModuloAssignmentExpression,
			SyntaxKind.DivideAssignmentExpression,
			SyntaxKind.MultiplyAssignmentExpression,
			SyntaxKind.SubtractAssignmentExpression,
			SyntaxKind.LeftShiftAssignmentExpression,
			SyntaxKind.RightShiftAssignmentExpression,
			SyntaxKind.ExclusiveOrAssignmentExpression,
		};

		public AssignmentWalker(Context context) : base(SupportedStatements, context)
		{

		}

		private ExpressionType GetExpressionType(SyntaxKind assigmenetKind)
		{
			switch (assigmenetKind)
			{
				case SyntaxKind.MultiplyAssignmentExpression:
					return ExpressionType.Multiply;

				case SyntaxKind.AddAssignmentExpression:
					return ExpressionType.Add;

				case SyntaxKind.SubtractAssignmentExpression:
					return ExpressionType.Subtract;

				case SyntaxKind.ModuloAssignmentExpression:
					return ExpressionType.Modulo;

				case SyntaxKind.OrAssignmentExpression:
					return ExpressionType.Or;

				case SyntaxKind.AndAssignmentExpression:
					return ExpressionType.And;

				case SyntaxKind.DivideAssignmentExpression:
					return ExpressionType.Divide;

				case SyntaxKind.LeftShiftAssignmentExpression:
					return ExpressionType.LeftShift;

				case SyntaxKind.RightShiftAssignmentExpression:
					return ExpressionType.RightShift;

				case SyntaxKind.ExclusiveOrAssignmentExpression:
					return ExpressionType.ExclusiveOr;
					
				default:
					throw new ArgumentException("Unsupported assigment operator conversion : " + assigmenetKind);
			}
		}

		/// <summary>
		/// Fore an eval property, the setter method is invoked after calculation.
		/// </summary>
		/// <returns>The eval property assign.</returns>
		/// <param name="node">Node.</param>
		/// <param name="symbol">Symbol.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		private Expression CreateEvalPropertyAssign(AssignmentExpressionSyntax node, IPropertySymbol symbol, Expression left, Expression right)
		{
			var instanceExpression = this.Context.GetInvokingInstance(node.Left);
			var kind = node.Kind();
			var operation = kind == SyntaxKind.SimpleAssignmentExpression ? right : Expression.MakeBinary(GetExpressionType(kind), left, right);
			return this.Context.InvokeSetProperty(symbol as IPropertySymbol,instanceExpression, operation);
		}

		/// <summary>
		/// Fore an eval field, the setter method is invoked after calculation.
		/// </summary>
		/// <returns>The eval property assign.</returns>
		/// <param name="node">Node.</param>
		/// <param name="symbol">Symbol.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		private Expression CreateEvalFieldAssign(AssignmentExpressionSyntax node, IFieldSymbol symbol, Expression left, Expression right)
		{
			var instanceExpression = this.Context.GetInvokingInstance(node.Left);
			var kind = node.Kind();
			var operation = kind == SyntaxKind.SimpleAssignmentExpression ? right : Expression.MakeBinary(GetExpressionType(kind), left, right);
			return this.Context.InvokeSetField(symbol, instanceExpression, operation);
		}

		/// <summary>
		/// Creates the runtime assignment from runtime types.
		/// </summary>
		/// <returns>The runtime assign.</returns>
		/// <param name="node">Node.</param>
		/// <param name="left">Left.</param>
		/// <param name="right">Right.</param>
		private Expression CreateRuntimeAssign(AssignmentExpressionSyntax node, Expression left, Expression right)
		{
			switch (node.Kind())
			{
				case SyntaxKind.SimpleAssignmentExpression:
					return Expression.Assign(left, right);

				case SyntaxKind.MultiplyAssignmentExpression:
					return Expression.MultiplyAssign(left, right);

				case SyntaxKind.AddAssignmentExpression:
					return Expression.AddAssign(left, right);
	
				case SyntaxKind.SubtractAssignmentExpression:
					return Expression.SubtractAssign(left, right);

				case SyntaxKind.ModuloAssignmentExpression:
					return Expression.ModuloAssign(left, right);

				case SyntaxKind.OrAssignmentExpression:
					return Expression.OrAssign(left, right);

				case SyntaxKind.AndAssignmentExpression:
					return Expression.AndAssign(left, right);

				case SyntaxKind.DivideAssignmentExpression:
					return Expression.DivideAssign(left, right);

				case SyntaxKind.LeftShiftAssignmentExpression:
					return Expression.LeftShiftAssign(left, right);

				case SyntaxKind.RightShiftAssignmentExpression:
					return Expression.RightShiftAssign(left, right);

				case SyntaxKind.ExclusiveOrAssignmentExpression:
					return Expression.ExclusiveOrAssign(left, right);

				default:
					throw new ArgumentException("Unsupported assigment expresion : " + node.Kind());
			}
		}

		public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
		{
			var left = new ExpressionWalker(this.Context).BuildWithResult(node.Left);
			var right = new ExpressionWalker(this.Context).BuildWithResult(node.Right);
			var symbol =  this.SemanticModel.GetSymbolInfo(node.Left).Symbol;

			if (symbol.Kind == SymbolKind.Property)
			{
				var propertySymbol = symbol as IPropertySymbol;
				var propertyEval = this.Context.ResultAssembly.GetEvalProperty(propertySymbol);
				if (propertyEval != null)
				{
					this.Result = this.CreateEvalPropertyAssign(node, propertySymbol, left, right);
				}
			}

			if (symbol.Kind == SymbolKind.Field)
			{
				var fieldSymbol = symbol as IFieldSymbol;
				var fieldEval = this.Context.ResultAssembly.GetEvalField(fieldSymbol);
				if (fieldEval != null)
				{
					this.Result = this.CreateEvalFieldAssign(node, fieldSymbol, left, right);
				}
			}

			if (this.Result == null)
			{
				this.Result = CreateRuntimeAssign(node, left, right);
			}
		}
	}
}
