using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public static class TypeExtensions
	{
		public static object GetDefault(this Type type)
		{
			if (typeof(void) != type && type.GetTypeInfo().IsValueType)
			{
				return Activator.CreateInstance(type);
			}
			return null;
		}

		public static ConstructorInfo GetConstructorInfo(this EvalType type, Context context, IMethodSymbol symbol)
		{
			if (!type.IsRuntime())
			{
				return typeof(EvalInstance).GetTypeInfo().DeclaredConstructors.First();
			}

			var constructors= type.Runtime.GetTypeInfo().DeclaredConstructors;
			return constructors.First(c =>
			{
				var sparams = symbol.Parameters;
				var cparams = c.GetParameters();

				if (sparams.Length != cparams.Length)
					return false;

				var sameArgs = true;
				for (int i = 0; i < cparams.Length; i++)
				{
					var cparam = cparams[i];
					var sparam = sparams.ElementAt(i);
					var evaltype = context.ResultAssembly.GetEvalType(sparam.Type);

					if (cparam.ParameterType != evaltype.Runtime)
					{
						break;
					}
				}

				return sameArgs;
			});
		}

		public static string GetQualifierName(this ITypeSymbol type)
		{
			var arraySuffix= string.Empty;

			if (type.Kind == SymbolKind.ArrayType)
			{
				var arrayType = type as IArrayTypeSymbol;
				arraySuffix += "[" + (new string(',', arrayType.Rank - 1)) + "]";
				type = arrayType.ElementType;
			}

			var name = type.Name;

			if (type.Kind == SymbolKind.NamedType)
			{
				var namedType = type as INamedTypeSymbol;
				if (namedType != null && namedType.Arity > 0)
				{
					name += $"`{namedType.Arity}";
				}
			}

			name += arraySuffix;

			var fullname = type.ContainingNamespace + "." + name + ", " + type.ContainingAssembly.Identity.ToString();

			return fullname;
			
		}

		public static MethodInfo GetMethodInfo(this Type type, Context context, IMethodSymbol symbol)
		{
			return type.GetRuntimeMethod(symbol.Name, symbol.Parameters.Select(p => context.ResultAssembly.GetEvalType(p.Type).Runtime).ToArray());
		}

		public static PropertyInfo GetPropertyInfo(this Type type, IPropertySymbol symbol)
		{
			return type.GetRuntimeProperty(symbol.Name);
		}

		public static EvalParameter ToParameter(this ParameterSyntax node, Context context)
		{
			var declared = context.SemanticModel.GetDeclaredSymbol(node);

			ITypeSymbol typesymbol = null;

			if (declared is IParameterSymbol)
			{
				typesymbol = ((IParameterSymbol)declared).Type;
			}
			else if (declared is IParameterSymbol)
			{
				typesymbol = ((ILocalSymbol)declared).Type;
			}
			else
			{
				throw new ArgumentException("Bad parameter type");
			}

			var type = context.ResultAssembly.GetEvalType(typesymbol);
			var expression = Expression.Parameter(type.Runtime, node.Identifier.Text);

			return new EvalParameter(node.Identifier.Text, type);

		}
	}
}
