using System;
using Microsoft.CodeAnalysis;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class IfStatementWalker : WalkerBase<ConditionalExpression>
	{
		public IfStatementWalker(BlockWalker block, LabelTarget returnTarget, Context context) : base(SyntaxKind.IfStatement, context)
		{
			this.blockWalker = block;
			this.returnTarget = returnTarget;
		}

		readonly LabelTarget returnTarget;

		readonly BlockWalker blockWalker;

		public override void VisitIfStatement(Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax node)
		{
			var condition = new ExpressionWalker(this.Context).BuildWithResult(node.Condition);
			var statement = new StatementWalker(blockWalker,returnTarget,this.Context).BuildWithResult(node.Statement);

			if (node.Else != null)
			{
				var elseClause = new StatementWalker(blockWalker, returnTarget, this.Context).BuildWithResult(node.Else.Statement);
				this.Result = Expression.IfThenElse(condition, statement, elseClause);
			}
			else
			{
				this.Result = Expression.IfThen(condition, statement);
			}
		}
	}
}
