using System;
using System.Collections.Generic;
using System.Linq;

namespace Evalyn
{
	public class EvalInstance
	{
		public EvalInstance(EvalClass type, EvalMethod constructor, params object[] args)
		{
			this.Type = type;
			this.fields = new object[type.Fields.Count()];

			for (int i = 0; i < this.fields.Count(); i++)
			{
				var field = type.Fields[i];
				this.fields[i] = field.Type.Runtime.GetDefault();
			}

			constructor.Invoke(this, args);
		}

		public EvalClass Type { get; private set; }

		#region Fields

		private object[] fields;

		public object GetField(EvalField field)
		{
			var value = fields[field.Index];

			return value;
		}

		public void SetField(EvalField field, object value)
		{
			this.fields[field.Index] = value;
		}

		#endregion
	}
}
