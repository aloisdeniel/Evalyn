using System;
using System.Linq;

namespace Evalyn.Test
{
	public static class Helpers
	{

		public static string[] References =
		{
			typeof(object).Assembly.Location,
			typeof(Enumerable).Assembly.Location,
			typeof(System.Diagnostics.Debug).Assembly.Location,
		};

	}
}
