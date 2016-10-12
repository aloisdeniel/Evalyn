using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class Context
	{
		public Context(IAssemblySymbol assembly, SemanticModel semanticModel)
		{
			this.SemanticModel = semanticModel;
			this.ResultAssembly = new EvalAssembly(assembly);
			this.ResultAssembly.Initialize(this);
		}

		private Stack<Dictionary<string, Expression>> localsStack = new Stack<Dictionary<string, Expression>>();

		public void PushLocals()
		{
			var scopeMembers = new Dictionary<string, Expression>(this.Locals);
			localsStack.Push(scopeMembers);
		}

		public void PopLocals()
		{
			this.Locals = localsStack.Pop();
		}

		private int lastIdentifier;

		public int CreateIdentifier() => ++lastIdentifier;

		public Expression This { get; set; }

		public SyntaxTree DebugTree { get; set; }

		public SemanticModel SemanticModel { get; private set; }

		public EvalAssembly ResultAssembly { get; private set; }

		public Dictionary<string, Expression> Locals { get; set; } = new Dictionary<string, Expression>();


	
	}
}
