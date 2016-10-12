using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class StatementWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.LocalDeclarationStatement,
			SyntaxKind.ReturnStatement,
			SyntaxKind.ExpressionStatement,
			SyntaxKind.IfStatement,
			SyntaxKind.WhileStatement,
			SyntaxKind.Block,
			SyntaxKind.ExpressionStatement,
			SyntaxKind.TryStatement,
			SyntaxKind.ThrowStatement,
		};

		public StatementWalker(BlockWalker block, LabelTarget returnTarget, Context context) : base(SupportedStatements, context)
		{
			this.block = block;
			this.returnTarget = returnTarget;
		}

		readonly BlockWalker block;

		readonly LabelTarget returnTarget;

		public override void VisitThrowStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ThrowStatementSyntax node)
		{
			var operand = new ExpressionWalker(this.Context).BuildWithResult(node.Expression);
			this.Result = Expression.Throw(operand);
		}

		public override void VisitBlock(Microsoft.CodeAnalysis.CSharp.Syntax.BlockSyntax node)
		{
			var walker = new BlockWalker(this.returnTarget, this.Context, false);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitReturnStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ReturnStatementSyntax node)
		{
			var walker = new ExpressionWalker(this.Context);
			var exp = walker.BuildWithResult(node.Expression);
			this.Result = Expression.Return(this.returnTarget,exp);
		}

		public override void VisitWhileStatement(Microsoft.CodeAnalysis.CSharp.Syntax.WhileStatementSyntax node)
		{
			var walker = new WhileStatementWalker(block, returnTarget,this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitIfStatement(Microsoft.CodeAnalysis.CSharp.Syntax.IfStatementSyntax node)
		{
			var walker = new IfStatementWalker(block, returnTarget, this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitLocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax node)
		{
			var walker = new VariableDeclarationStatementWalker(this.Context);
			this.Result = Expression.Block(walker.BuildWithResult(node));
			foreach (var v in walker.Variables)
			{
				this.block.AddLocal(v);
			}
		}

		public override void VisitExpressionStatement(Microsoft.CodeAnalysis.CSharp.Syntax.ExpressionStatementSyntax node)
		{
			var walker = new ExpressionWalker(this.Context);
			this.Result = walker.BuildWithResult(node.Expression);
		}

		public override void VisitTryStatement(Microsoft.CodeAnalysis.CSharp.Syntax.TryStatementSyntax node)
		{
			var walker = new TryCatchWalker(block, returnTarget, this.Context);
			this.Result = walker.BuildWithResult(node);
		}
	}
}
