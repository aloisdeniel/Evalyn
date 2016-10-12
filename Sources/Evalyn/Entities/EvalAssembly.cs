using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalAssembly
	{
		public EvalAssembly(IAssemblySymbol symbol)
		{
			this.Name = symbol.Name;
			this.Symbol = symbol;
		}

		public string Name
		{
			get;
			private set;
		}

		public IAssemblySymbol Symbol { get; private set; }

		private List<EvalClass> types = new List<EvalClass>();

		public IEnumerable<EvalClass> Types => types.ToArray();

		internal void RegisterType(EvalClass type)
		{
			this.types.Add(type);
		}

		public EvalClass GetType(string fullname)
		{
			return this.types.FirstOrDefault(t => t.Fullname == fullname);
		}

		public void Initialize(Context context)
		{
			this.InitializeClasses(context);
			this.InitializeMembers(context);
		}

		private void InitializeClasses(Context context)
		{
			var semanticAssembly = context.SemanticModel.Compilation.Assembly;

			// Declaring all types
			foreach (var typename in semanticAssembly.TypeNames)
			{
				var type = semanticAssembly.GetTypeByMetadataName(typename);
				var evaltype = new EvalClass(context, type);
				this.RegisterType(evaltype);
			}

			// Inheritence
			foreach (var type in this.Types)
			{
				if (type.Symbol.BaseType != null)
				{
					type.BaseType = context.ResultAssembly.GetEvalType(type.Symbol.BaseType);
				}
			}
		}

		private void InitializeMembers(Context context)
		{
			var semanticAssembly = context.SemanticModel.Compilation.Assembly;

			foreach (var type in this.Types)
			{
				// Members
				var members = type.Symbol .GetMembers();
				foreach (var member in members)
				{
					if (member.Kind == SymbolKind.Method)
					{
						var method = member as IMethodSymbol;
						var evalmethod = new EvalMethod(context, method);
						type.RegisterMethod(evalmethod);
					}
					else if (member.Kind == SymbolKind.Property)
					{
						var property = member as IPropertySymbol;
						var evalprop = new EvalProperty(context, property);
						type.RegisterProperty(evalprop);
					}
					else if (member.Kind == SymbolKind.Field)
					{
						var field = member as IFieldSymbol;
						var evalfield = new EvalField(context, field);
						type.RegisterField(evalfield);
					}
				}
			}
		}

		public EvalClass GetEvalClass(ITypeSymbol symbol) => this.Types.FirstOrDefault(t => t.Symbol == symbol);

		public EvalMethod GetEvalMethod(IMethodSymbol symbol) => this.Types.SelectMany(t => t.Methods).FirstOrDefault(m => m.Symbol == symbol);

		public EvalProperty GetEvalProperty(IPropertySymbol symbol) => this.Types.SelectMany(t => t.Properties).FirstOrDefault(m => m.Symbol == symbol);

		public EvalField GetEvalField(IFieldSymbol symbol) => this.Types.SelectMany(t => t.Fields).FirstOrDefault(m => m.Symbol == symbol);

		public EvalType GetEvalType(ITypeSymbol type)
		{
			if (type == null)
			{
				return null;
			}

			if (type.ContainingAssembly == this.Symbol)
			{
				return new EvalType(this.GetEvalClass(type));
			}

			var fullname = type.GetQualifierName();

			var result = Type.GetType(fullname, true);

			if (result.GetTypeInfo().IsGenericType)
			{
				var namedType = ((INamedTypeSymbol)type);
				result = result.MakeGenericType(namedType.TypeArguments.Select(t => GetEvalType(t).Runtime).ToArray());
			}

			return new EvalType(result);
		}
	}
}
