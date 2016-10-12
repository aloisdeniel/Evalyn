using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalClass
	{
		public EvalClass(Context context, ITypeSymbol symbol)
		{
			this.Symbol = symbol;
		}

		public string Namespace => (Symbol.ContainingNamespace == Symbol.ContainingAssembly.GlobalNamespace) ? null : Symbol.ContainingNamespace.ToString();

		public string Name => Symbol.Name;

		public ITypeSymbol Symbol { get; set; }

		public string Fullname => string.IsNullOrEmpty(Namespace) ? Name : $"{Namespace}.{Name}";

		#region Inheritence

		public EvalType BaseType { get; set; }

		#endregion

		#region Fields

		private List<EvalField> fields = new List<EvalField>();

		public EvalField[] Fields => fields.ToArray();

		internal void RegisterField(EvalField field)
		{
			field.Index = fields.Count;
			this.fields.Add(field);
		}

		#endregion

		#region Properties

		private List<EvalProperty> properties = new List<EvalProperty>();

		public IEnumerable<EvalProperty> Properties => properties.ToArray();

		internal void RegisterProperty(EvalProperty property)
		{
			this.properties.Add(property);
			this.RegisterField(property.BackingField);
		}

		public EvalProperty GetProperty(string name) => this.properties.FirstOrDefault(f => f.Name == name);

		#endregion

		#region Methods

		private List<EvalMethod> methods = new List<EvalMethod>();

		public IEnumerable<EvalMethod> Methods => methods.ToArray();

		internal void RegisterMethod(EvalMethod method)
		{
			this.methods.Add(method);
		}

		public IEnumerable<EvalMethod> GetMethods()
		{
			return methods.ToArray();
		}

		public EvalMethod GetMethod(IMethodSymbol symbol) => this.Methods.FirstOrDefault(m => m.Symbol == symbol);

		public EvalMethod GetMethod(string name) => GetMethod(name, new Type[0]);

		public EvalMethod GetMethod<TArg>(string name) => GetMethod(name, new Type[] { typeof(TArg) });

		public EvalMethod GetMethod<TArg1, TArg2>(string name) => GetMethod(name, new Type[] { typeof(TArg1), typeof(TArg2),  });

		public EvalMethod GetMethod<TArg1, TArg2, TArg3>(string name) => GetMethod(name, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3) });

		public EvalMethod GetMethod<TArg1, TArg2, TArg3, TArg4>(string name) => GetMethod(name, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4) });

		public EvalMethod GetMethod<TArg1, TArg2, TArg3, TArg4, TArg5>(string name) => GetMethod(name, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5) });

		public EvalMethod GetMethod<TArg1, TArg2, TArg3, TArg4, TArg5, TArg6>(string name) => GetMethod(name, new Type[] { typeof(TArg1), typeof(TArg2), typeof(TArg3), typeof(TArg4), typeof(TArg5), typeof(TArg6) });

		public EvalMethod GetMethod(string name, params Type[] args) => this.GetMethod(name, args.Select(a => new EvalType(a)).ToArray());

		public EvalMethod GetMethod(string name, params EvalType[] args)
		{
			var method = this.methods.FirstOrDefault(m => m.Name == name && m.HasParameters(args));

			//TODO baste type call if null
			return method;
		}
			
		#endregion
	}
}
