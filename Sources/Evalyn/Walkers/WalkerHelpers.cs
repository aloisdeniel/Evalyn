using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public static class WalkerHelpers
	{
		public static Expression GetInvokingInstance(this Context context, SyntaxNode expression)
		{
			var memberAccess = expression as MemberAccessExpressionSyntax;

			Expression instanceExpression = null;

			if (memberAccess != null && memberAccess.Expression != null)
			{
				var walker = new ExpressionWalker(context);
				instanceExpression = walker.BuildWithResult(memberAccess.Expression);
			}

			return instanceExpression;
		}

		public static Expression InvokeGetProperty(this Context context, IPropertySymbol property, Expression instance)
		{
			var prop = context.ResultAssembly.GetEvalProperty(property);
			var exp = Expression.Invoke(prop.Getter.Body, new[] { instance });
			return Expression.Convert(exp, prop.Type.Runtime);
		}

		public static Expression InvokeSetProperty(this Context context, IPropertySymbol property, Expression instance, Expression value)
		{
			var prop = context.ResultAssembly.GetEvalProperty(property);
			return Expression.Invoke(prop.Setter.Body, new[] { instance, Expression.Convert(value, typeof(object)) });
		}

		public static Expression InvokeGetField(this Context context, IFieldSymbol field, Expression instance)
		{
			var evalfield = context.ResultAssembly.GetEvalField(field);

			var methodinfo = typeof(EvalInstance).GetRuntimeMethod(nameof(EvalInstance.GetField), new[] { typeof(EvalField)  });
			var exp = Expression.Call(instance, methodinfo, new Expression[] { Expression.Constant(evalfield) });
			return Expression.Convert(exp, evalfield.Type.Runtime);
		}

		public static Expression InvokeSetField(this Context context, IFieldSymbol field, Expression instance, Expression value)
		{
			var evalfield = context.ResultAssembly.GetEvalField(field);

			var methodinfo = typeof(EvalInstance).GetRuntimeMethod(nameof(EvalInstance.SetField), new[] { typeof(EvalField), typeof(object) });
			return Expression.Call(instance, methodinfo, new Expression[] { Expression.Constant(evalfield), Expression.Convert(value, typeof(object)) });
		}


	}
}
