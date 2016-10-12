using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class LamdaWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.SimpleLambdaExpression,
			SyntaxKind.ParenthesizedLambdaExpression,
		};

		public LamdaWalker(Context context) : base(SupportedStatements, context)
		{
		}

		private Expression CreateResult(Type returnType, SyntaxNode bodyNode, ParameterSyntax[] parameterNodes)
		{

			this.Context.PushLocals();

			// Parameterss

			var parameterList = new List<ParameterExpression>();
			foreach (var attribute in parameterNodes)
			{
				var parameter = attribute.ToParameter(this.Context);
				parameterList.Add(parameter.Expression);
				this.Context.Locals[parameter.Name] = parameter.Expression;
			}

			Expression body;

			if (bodyNode.Kind() == SyntaxKind.Block)
			{

				LabelTarget returnTarget;

				//Return type

				if (returnType == null)
				{
					returnTarget = Expression.Label();
				}
				else
				{
					returnTarget = Expression.Label(returnType);
				}

				// With body block

				var blockWalker = new BlockWalker(returnTarget, this.Context, true);
				body = blockWalker.BuildWithResult(bodyNode);
			}
			else
			{
				// With body expression

				var walker = new ExpressionWalker(this.Context);
				body = walker.BuildWithResult(bodyNode);

				if (returnType == null || returnType == typeof(void))
					body = Expression.Block(typeof(void), new[] { body });
			}

			this.Context.PopLocals();

			return Expression.Lambda(body, parameterList);
		}

		public override void VisitSimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax node)
		{
			var info = this.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			var returnType = this.Context.ResultAssembly.GetEvalType(info.ReturnType);

			this.Context.PushLocals();


			this.Result = this.CreateResult(returnType.Runtime, node.Body, new[] { node.Parameter });

			this.Context.PopLocals();
		}

		public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
		{
			var info = this.SemanticModel.GetSymbolInfo(node).Symbol as IMethodSymbol;
			var returnType = this.Context.ResultAssembly.GetEvalType(info.ReturnType);

			this.Context.PushLocals();

			this.Result = this.CreateResult(returnType.Runtime, node.Body, node.ParameterList.Parameters.ToArray());

			this.Context.PopLocals();
		}
	}
}
