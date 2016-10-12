using System;
using Microsoft.CodeAnalysis;

namespace Evalyn
{
	public class EvalField
	{
		public EvalField(Context context, IFieldSymbol symbol) : this(symbol.Name,context.ResultAssembly.GetEvalType(symbol.Type)) 
		{
			this.Symbol = symbol;
		}

		public EvalField(string name, EvalType type)
		{
			this.Name = name;
			this.Type = type;
		}

		public IFieldSymbol Symbol { get; private set; }

		public string Name { get; private set; }

		public EvalType Type { get; private set; }

		public int Index { get; set; }
	}
}
