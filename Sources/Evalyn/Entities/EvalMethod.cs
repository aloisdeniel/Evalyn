using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalMethod
	{
		public EvalMethod(Context context, IMethodSymbol symbol)
		{
			this.Symbol = symbol;

			var parameters = new List<EvalParameter>();

			if (!symbol.IsStatic)
			{
				var tthis = new EvalParameter("this", new EvalType(typeof(EvalInstance)));
				parameters.Add(tthis);
			}

			parameters.AddRange(symbol.Parameters.Select(p => new EvalParameter(context, p)));

			this.Parameters = parameters;
		}

		public string Name => Symbol.Name;

		public IMethodSymbol Symbol { get; private set; } // Could eventually be converted to a simple identifier for improved performances

		public IEnumerable<EvalParameter> Parameters
		{
			get;
			private set;
		}

		public Expression Body
		{
			get; 
			private set;
		}

		public bool IsStatic { get; set; }

		private Delegate compiled;

		public void Compile(Expression body)
		{
			this.Body = body;
			var lambda = Expression.Lambda(this.Body, this.Name, this.Parameters.Select(p => p.Expression));
			this.compiled = lambda.Compile();
		}

		public object Invoke(params object[] args)
		{
			if (compiled == null)
				throw new InvalidOperationException("Method must be compiled first.");

			if (args.Length == 0 || args.First()?.GetType() != typeof(EvalInstance))
			{
				throw new InvalidOperationException("This method should only be called from an instance");
			}

			return this.compiled.DynamicInvoke(args);
		}

		public object Invoke(EvalInstance instance, params object[] args)
		{
			return this.Invoke(new object[] { instance }.Union(args).ToArray());
		}

		public bool HasParameters(params EvalType[] parametersTypes)
		{
			var parameters = this.IsStatic ? this.Parameters : this.Parameters.Skip(1);
			return parameters.Select(p => p.Type).SequenceEqual(parametersTypes);
		}
	}
}
