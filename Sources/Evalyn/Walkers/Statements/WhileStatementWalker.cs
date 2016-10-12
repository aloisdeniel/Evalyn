using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class WhileStatementWalker: WalkerBase<LoopExpression>
	{
		public WhileStatementWalker(BlockWalker block, LabelTarget returnTarget, Context context) : base(SyntaxKind.WhileStatement, context)
		{
			this.blockWalker = block;
			this.returnTarget = returnTarget;
		}

		readonly LabelTarget returnTarget;

		readonly BlockWalker blockWalker;

		public override void VisitWhileStatement(Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax node)
		{
			var target = Expression.Label();
	
			var condition = new ExpressionWalker(this.Context).BuildWithResult(node.Condition);
			var statement = new StatementWalker(blockWalker, returnTarget, this.Context).BuildWithResult(node.Statement);

			var body = Expression.Block(new[]
			{
				Expression.IfThen(Expression.Not(condition), Expression.Break(target)),
				statement,
			});

			this.Result = Expression.Loop( body,target);

		}
	}
}
