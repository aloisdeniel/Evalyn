using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalParameter
	{
		public EvalParameter(Context context, IParameterSymbol symbol) : this(symbol.Name, context.ResultAssembly.GetEvalType(symbol.Type))
		{
			this.Symbol = symbol;
		}

		public EvalParameter(string name, EvalType type)
		{
			this.Name = name;
			this.Type = type;
			this.Symbol = null;
			this.Expression = System.Linq.Expressions.Expression.Parameter(this.Type.Runtime, this.Name);
		}

		public IParameterSymbol Symbol { get; private set; }

		public string Name { get; private set; }

		public EvalType Type { get; private set; }

		public ParameterExpression Expression { get; private set; }
	}
}
