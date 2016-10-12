using System;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalProperty
	{
		public EvalProperty(Context context, IPropertySymbol symbol)
		{
			this.Symbol = symbol;
			this.BackingField = new EvalField(symbol.Name, context.ResultAssembly.GetEvalType(symbol.Type));

			this.InitializeGetter(context);
			this.InitializeSetter(context);
		}

		public EvalType Type => this.BackingField.Type;

		private void InitializeGetter(Context context)
		{
			if (!this.Symbol.IsWriteOnly)
			{
				var evalgetter = new EvalMethod(context, this.Symbol.GetMethod);

				// Auto-generated default accessor
				var methodinfo = typeof(EvalInstance).GetRuntimeMethod(nameof(EvalInstance.GetField), new[] { typeof(EvalField) });
				var instance = Expression.Parameter(typeof(EvalInstance));
				var call = Expression.Convert(Expression.Call(instance, methodinfo, Expression.Constant(this.BackingField)), this.Type.Runtime);
				var getterBody = Expression.Lambda(call, new[] { instance });
				evalgetter.Compile(getterBody);

				this.Getter = evalgetter;
			}
		}

		private void InitializeSetter(Context context)
		{
			if (!this.Symbol.IsReadOnly)
			{
				var evalsetter = new EvalMethod(context, this.Symbol.SetMethod);

				// Auto-generated default accessor
				var methodinfo = typeof(EvalInstance).GetRuntimeMethod(nameof(EvalInstance.SetField), new[] { typeof(EvalField), typeof(object) });
				var instance = Expression.Parameter(typeof(EvalInstance));
				var value = Expression.Parameter(typeof(object));
				var setterBody = Expression.Lambda(Expression.Call(instance, methodinfo, Expression.Constant(this.BackingField), value), new[] { instance, value });
				evalsetter.Compile(setterBody);

				this.Setter = evalsetter;
			}
		}

		public string Name { get; private set; }

		public IPropertySymbol Symbol { get; private set; }

		public EvalField BackingField
		{
			get;
			set;
		}

		public EvalMethod Getter
		{
			get;
			set;
		}

		public EvalMethod Setter
		{
			get;
			set;
		}
	}
}
