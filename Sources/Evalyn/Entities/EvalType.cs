using System;
using System.Linq.Expressions;

namespace Evalyn
{
	public class EvalType
	{

		public EvalType(Type type)
		{
			this.Runtime = type;
		}

		public EvalType(EvalClass type)
		{
			this.Runtime = typeof(EvalInstance);
			this.Eval = type;
		}

		public Type Runtime { get; private set; }

		public EvalClass Eval { get; private set; }

		public bool IsRuntime()
		{
			return Runtime != typeof(EvalInstance);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as EvalType); 
		}

		public bool Equals(EvalType t)
		{
			if ((object)t == null) return false;
			return (this.Runtime == t.Runtime) && (this.Eval == t.Eval);
		}
		public override int GetHashCode()
		{
			return this.Runtime.GetHashCode() ^ (this.Eval?.GetHashCode() ?? 0);
		}
	}
}
