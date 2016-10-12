using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class TryCatchWalker : WalkerBase<TryExpression>
	{
		public TryCatchWalker(BlockWalker block, LabelTarget returnTarget, Context context) : base(SyntaxKind.TryStatement, context)
		{
			this.blockWalker = block;
			this.returnTarget = returnTarget;
		}

		readonly LabelTarget returnTarget;

		readonly BlockWalker blockWalker;

		public override void VisitTryStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax node)
		{
			var catchBlocks = node.Catches.Select(c =>
			{
				CatchBlock result = null;

				this.Context.PushLocals();

				var typeinfo = this.SemanticModel.GetTypeInfo(c.Declaration.Type);
				var type = this.Context.ResultAssembly.GetEvalType(typeinfo.Type);

				if (type.IsRuntime())
				{
					var ex = Expression.Parameter(type.Runtime, c.Declaration.Identifier.Text);
					this.Context.Locals[c.Declaration.Identifier.Text] = ex;

					var body = new StatementWalker(blockWalker, returnTarget, this.Context).BuildWithResult(c.Block);

					result = Expression.Catch(ex, body);

				}
				else
				{
					// TODO  create an EvalException and manage catch filter 

					var ex = Expression.Parameter(type.Runtime, c.Declaration.Identifier.Text);
					this.Context.Locals[c.Declaration.Identifier.Text] = ex;
					var body = new StatementWalker(blockWalker, returnTarget, this.Context).BuildWithResult(c.Block);
					result = Expression.Catch(typeof(Exception),body);
				}

				this.Context.PopLocals();

				return result;
			});

			var block = new StatementWalker(blockWalker, returnTarget, this.Context).BuildWithResult(node.Block);

			this.Result = Expression.TryCatch(block, catchBlocks.ToArray());
			
		}
	}
}
