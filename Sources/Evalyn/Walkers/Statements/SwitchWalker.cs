using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class SwitchWalker : WalkerBase<SwitchExpression>
	{
		public SwitchWalker(BlockWalker block, LabelTarget returnTarget, Context context) : base(SyntaxKind.IfStatement, context)
		{
			this.blockWalker = block;
			this.returnTarget = returnTarget;
		}

		readonly LabelTarget returnTarget;

		readonly BlockWalker blockWalker;

		public override void VisitSwitchStatement(Microsoft.CodeAnalysis.CSharp.Syntax.SwitchStatementSyntax node)
		{
			var expression = new ExpressionWalker(this.Context).BuildWithResult(node.Expression);
			var cases = new List<SwitchCase>();

			foreach (var section in node.Sections)
			{
				// TODO
			}

			this.Result = Expression.Switch(expression, cases.ToArray());
		}

	}
}
