using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class PropertyWalker: WalkerBase
	{
		public PropertyWalker(Context context) : base(SyntaxKind.PropertyDeclaration, context)
		{

		}

		public override void VisitPropertyDeclaration(PropertyDeclarationSyntax node)
		{
			var symbol = this.SemanticModel.GetDeclaredSymbol(node) as IPropertySymbol;
			var property = this.Context.ResultAssembly.GetEvalProperty(symbol);

			// Bodied properties

			if (node.ExpressionBody?.Expression != null)
			{
				this.Context.This = Expression.Parameter(typeof(EvalInstance));
				var walker = new ExpressionWalker(this.Context);
				var getterExpr = walker.BuildWithResult(node.ExpressionBody.Expression);
				var getterBody = Expression.Lambda(getterExpr, this.Context.This as ParameterExpression);
				property.Getter.Compile(getterBody);
				this.Context.This = null;
			}
			else
			{
				var accessors = node.AccessorList.Accessors;

				if (!symbol.IsWriteOnly)
				{
					var getter = accessors.FirstOrDefault((a) => a.Kind() == SyntaxKind.GetAccessorDeclaration);

					if (getter.Body != null)
					{
						var walker = new MethodWalker(this.Context);
						var getterBody = walker.BuildWithResult(getter.Body);
						property.Getter.Compile(getterBody);
					}
				}

				if (!symbol.IsReadOnly)
				{
					var setter = accessors.FirstOrDefault((a) => a.Kind() == SyntaxKind.SetAccessorDeclaration);

					if (setter.Body != null)
					{
						var walker = new MethodWalker(this.Context);
						var setterBody = walker.BuildWithResult(setter.Body);
						property.Setter.Compile(setterBody);
					}
				}
			}

			// Todo, call initializer
		}

	}
}
