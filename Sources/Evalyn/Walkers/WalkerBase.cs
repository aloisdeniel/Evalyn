using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public abstract class WalkerBase : CSharpSyntaxWalker
	{

		public WalkerBase(SyntaxKind supportedKind, Context context) : this(new[] { supportedKind }, context)
		{

		}

		public WalkerBase(SyntaxKind[] supportedKinds, Context context)
		{
			this.Context = context;
			this.SupportedKinds = supportedKinds;
		}

		public bool IsFinished { get; protected set; }

		protected Context Context { get; private set; }

		protected SemanticModel SemanticModel => this.Context.SemanticModel;


		protected SyntaxKind[] SupportedKinds { get; private set; }

		public override void Visit(SyntaxNode node)
		{
			if (!this.IsFinished)
			{
				Debug.WriteLine($"[{this.GetType()}] visited a {node.Kind()} node ({node.ToString()})");
				base.Visit(node);
			}
		}

		public void Build(SyntaxNode node)
		{
			if (!this.SupportedKinds.Contains(node.Kind()))
			{
				//var info = this.Context.SemanticModel.GetSymbolInfo(node);

				throw new ArgumentException($"The given node is of kind '{node.Kind()}' instead of one of supporte ones : {string.Join(",", this.SupportedKinds)}");
			}

			this.Visit(node);
		}
	}

	public abstract class WalkerBase<TExpression> : WalkerBase
	{
		public WalkerBase(SyntaxKind supportedKind, Context context) : this(new[] { supportedKind }, context)
		{
		}

		public WalkerBase(SyntaxKind[] supportedKinds, Context context) : base(supportedKinds,context)
		{
		}

		private TExpression result;

		protected TExpression Result 
		{ 
			get 
			{
				return this.result; 
			} 
			set
			{ 
				this.result = value;
				this.IsFinished = true;
			} 
		}

		public TExpression BuildWithResult(SyntaxNode node)
		{
			this.Build(node);

			return this.Result;
		}

	}
}
