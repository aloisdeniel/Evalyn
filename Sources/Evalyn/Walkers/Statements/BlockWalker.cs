using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class BlockWalker : WalkerBase<Expression>
	{
		public BlockWalker(LabelTarget returnTarget, Context context, bool insertExitLabel) : base(SyntaxKind.Block, context)
		{
			this.returnTarget = returnTarget;
			this.insertExitLabel = insertExitLabel;
		}

		readonly bool insertExitLabel;

		readonly LabelTarget returnTarget;

		private bool isStarted;

		private List<Expression> statements = new List<Expression>();

		private List<ParameterExpression> locals = new List<ParameterExpression>();

		public void AddLocal(ParameterExpression exp)
		{
			this.locals.Add(exp);
		}

		public override void VisitBlock(BlockSyntax node)
		{
			if (isStarted)
			{
				base.VisitBlock(node);
				return;
			}

			this.isStarted = true;

			var scopeMembers = new Dictionary<string,Expression>(Context.Locals);

			base.VisitBlock(node);

			Context.Locals = scopeMembers;

			if (insertExitLabel)
			{
				var defaultValue = this.returnTarget.Type.GetDefault();
				var lastStatement = Expression.Label(this.returnTarget, Expression.Constant(defaultValue));

				this.statements.Add(lastStatement);
				Result = Expression.Block( this.returnTarget.Type, this.locals, this.statements);
			}
			else if(this.statements.Count > 0)
			{
				Result = Expression.Block(typeof(void), this.locals, this.statements);
			}
			else
			{
				Result = Expression.Empty();
			}
		}

		public override void Visit(SyntaxNode node)
		{
			if (isStarted && StatementWalker.SupportedStatements.Contains(node.Kind()))
			{
				var walker = new StatementWalker(this,this.returnTarget, this.Context);
				this.statements.Add(walker.BuildWithResult(node));
			}
			else
			{
				base.Visit(node);
			}
		}
	}
}
