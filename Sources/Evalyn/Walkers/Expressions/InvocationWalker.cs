using System;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class InvocationWalker : WalkerBase<Expression>
	{
		public InvocationWalker(Context context) : base(SyntaxKind.InvocationExpression, context)
		{
		}

		public override void VisitInvocationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax node)
		{
			var sinfo = this.SemanticModel.GetSymbolInfo(node);

			// nameof
			if (sinfo.Symbol == null && sinfo.CandidateSymbols.IsEmpty && sinfo.CandidateReason == CandidateReason.None)
			{
				var identifier = node.Expression as IdentifierNameSyntax;
				if (identifier != null && identifier.Identifier.Kind() == SyntaxKind.IdentifierToken && identifier.Identifier.Text == "nameof")
				{
					var name = node.ArgumentList.Arguments.First();
					var nameinfo = this.SemanticModel.GetSymbolInfo(name.Expression);
					this.Result = Expression.Constant(nameinfo.Symbol.Name);
					return;
				}
			}

			// Methods
			var arguments = node.ArgumentList.Arguments.Select(a =>
			{
				var walker = new ExpressionWalker(this.Context);
				return walker.BuildWithResult(a.Expression);
			});

			var info = sinfo.Symbol as IMethodSymbol;
			var type = this.Context.ResultAssembly.GetEvalType(info.ContainingType);
			var returntype = this.Context.ResultAssembly.GetEvalType(info.ReturnType);

			if (info.MethodKind == MethodKind.DelegateInvoke)
			{
				var identitier = node.Expression as IdentifierNameSyntax;
				var expression = this.Context.Locals[identitier.Identifier.Text];
				this.Result = Expression.Invoke(expression, arguments);
			}
			else if (!info.IsStatic)
			{
				var instanceExpression = this.Context.GetInvokingInstance(node.Expression) ?? this.Context.This;

				if (type.IsRuntime())
				{
					var methodInfo = type.Runtime.GetMethodInfo(this.Context, info);
					this.Result = Expression.Call(instanceExpression, methodInfo, arguments);
				}
				else
				{
					var evalmethod = this.Context.ResultAssembly.GetEvalMethod(info);

					// 1:  EvalMethod.Invoke(EvalInstance, Array(EvalParameters)); (Not working)
					var argsArray = Expression.NewArrayInit(typeof(object), arguments.Select(a => Expression.Convert(a, typeof(object))));
					var methodinfo = typeof(EvalMethod).GetRuntimeMethod( nameof(EvalMethod.Invoke), new[] { typeof(EvalInstance), typeof(object[]) });
					this.Result = Expression.Convert(Expression.Call(Expression.Constant(evalmethod),methodinfo, new Expression [] { instanceExpression, argsArray }), returntype.Runtime );
				}
			}
			else 
			{
				if (type.IsRuntime())
				{
					var methodInfo = type.Runtime.GetMethodInfo(this.Context, info);
					this.Result = Expression.Call(methodInfo, arguments);
				}
				else
				{
					// EvalMethod.Invoke(Array(EvalParameters));
					var argsArray = Expression.NewArrayInit(typeof(object), arguments.Select(a => Expression.Convert(a, typeof(object))));
					var evalmethod = this.Context.ResultAssembly.GetEvalMethod(info);
					var methodinfo = typeof(EvalMethod).GetRuntimeMethod(nameof(EvalMethod.Invoke), new[] { typeof(EvalInstance), typeof(object[]) });
					this.Result = Expression.Convert(Expression.Call(Expression.Constant(evalmethod), methodinfo, argsArray), returntype.Runtime );
				}
			}
		}
	}
}
