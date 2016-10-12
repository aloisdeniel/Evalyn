using System;
using System.Linq;
using System.Reflection;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class ObjectCreationWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.ObjectCreationExpression,
			SyntaxKind.ArrayCreationExpression,
			SyntaxKind.ImplicitArrayCreationExpression,
		};

		public ObjectCreationWalker(Context context) : base(SupportedStatements, context)
		{
		}

		private Expression CreateArray(IArrayTypeSymbol type, ArrayTypeSyntax syntax, InitializerExpressionSyntax initializer)
		{
			// TODO multiple dimensions

			var elementtype = this.Context.ResultAssembly.GetEvalType(type.ElementType);

			if (initializer != null)
			{
				// new int[0] {...};
				return Expression.NewArrayInit(elementtype.Runtime, initializer.Expressions.Select(e => new ExpressionWalker(this.Context).BuildWithResult(e)));
			}
			else
			{
				// new int[0];
				var bounds = syntax.RankSpecifiers.Select((rank) => rank.Sizes.Select(size => new ExpressionWalker(this.Context).BuildWithResult(size)));
				return Expression.NewArrayBounds(elementtype.Runtime, bounds.First());
			}

			throw new InvalidOperationException("Failed to initialize array");
		}

		public override void VisitImplicitArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax node)
		{
			System.Diagnostics.Debug.WriteLine("ARR1>" + node);
			// new int[] {}
			var typeinfo = this.SemanticModel.GetTypeInfo(node).Type as IArrayTypeSymbol;
			this.Result = this.CreateArray(typeinfo, null, node.Initializer);
		}

		public override void VisitArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax node)
		{
			System.Diagnostics.Debug.WriteLine("ARR2>" + node);
			var typeinfo = this.SemanticModel.GetTypeInfo(node.Type).Type as IArrayTypeSymbol;
			this.Result = this.CreateArray(typeinfo, node.Type, node.Initializer);
		}

		public override void VisitObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax node)
		{
			System.Diagnostics.Debug.WriteLine("OBJCREA>" + node);
			var info = this.SemanticModel.GetSymbolInfo(node);
			var typeinfo = this.SemanticModel.GetTypeInfo(node);
			var type = this.Context.ResultAssembly.GetEvalType(typeinfo.Type);

			var constructorargs = node.ArgumentList.Arguments.Select(a =>
			{
				var walker = new ExpressionWalker(this.Context);
				return walker.BuildWithResult(a.Expression);
			});

			var methodSymbol = info.Symbol as IMethodSymbol;
			var constructorInfo = type.GetConstructorInfo(this.Context, methodSymbol);

			if (!type.IsRuntime())
			{
				var evalConstructor =  this.Context.ResultAssembly.GetEvalMethod(methodSymbol);
				constructorargs = new Expression [] { Expression.Constant(type.Eval), Expression.Constant(evalConstructor), Expression.NewArrayInit(typeof(object) ,constructorargs.Select(c=> Expression.Convert(c,typeof(object))))  };
			}

			this.Result = Expression.New(constructorInfo, constructorargs);
		}
	}
}
