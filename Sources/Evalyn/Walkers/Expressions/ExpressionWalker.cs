using System;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class ExpressionWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.EqualsValueClause,
			SyntaxKind.ParenthesizedExpression,
			SyntaxKind.InvocationExpression,
			SyntaxKind.SimpleLambdaExpression,
		}.Union(ConstantWalker.SupportedStatements)
		 .Union(SourceWalker.SupportedStatements)
		 .Union(ObjectCreationWalker.SupportedStatements)
		 .Union(ConditionalWalker.SupportedStatements)
		 .Union(BinaryWalker.SupportedStatements)
		 .Union(AssignmentWalker.SupportedStatements)
		 .Union(UnaryWalker.SupportedStatements).ToArray();

		public ExpressionWalker(Context context) : base(SupportedStatements, context)
		{
		}

		public override void VisitEqualsValueClause(Microsoft.CodeAnalysis.CSharp.Syntax.EqualsValueClauseSyntax node)
		{
			base.Visit(node.Value);
		}

		public override void VisitParenthesizedExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ParenthesizedExpressionSyntax node)
		{
			base.Visit(node.Expression);
		}

		public override void VisitLiteralExpression(Microsoft.CodeAnalysis.CSharp.Syntax.LiteralExpressionSyntax node)
		{
			var walker = new ConstantWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitIdentifierName(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax node)
		{
			var walker = new SourceWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitThisExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ThisExpressionSyntax node)
		{
			var walker = new SourceWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitMemberAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax node)
		{
			var walker = new SourceWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitPrefixUnaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.PrefixUnaryExpressionSyntax node)
		{
			var walker = new UnaryWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitPostfixUnaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.PostfixUnaryExpressionSyntax node)
		{
			var walker = new UnaryWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitBinaryExpression(Microsoft.CodeAnalysis.CSharp.Syntax.BinaryExpressionSyntax node)
		{
			var walker = new BinaryWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitAssignmentExpression(Microsoft.CodeAnalysis.CSharp.Syntax.AssignmentExpressionSyntax node)
		{
			var walker = new AssignmentWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitObjectCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ObjectCreationExpressionSyntax node)
		{
			var walker = new ObjectCreationWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ArrayCreationExpressionSyntax node)
		{
			var walker = new ObjectCreationWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitImplicitArrayCreationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ImplicitArrayCreationExpressionSyntax node)
		{
			var walker = new ObjectCreationWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitInvocationExpression(Microsoft.CodeAnalysis.CSharp.Syntax.InvocationExpressionSyntax node)
		{
			var walker = new InvocationWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitSimpleLambdaExpression(Microsoft.CodeAnalysis.CSharp.Syntax.SimpleLambdaExpressionSyntax node)
		{
			var walker = new LamdaWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}

		public override void VisitConditionalExpression(Microsoft.CodeAnalysis.CSharp.Syntax.ConditionalExpressionSyntax node)
		{
			var walker = new ConditionalWalker(this.Context);
			this.Result = walker.BuildWithResult(node);
		}
	}
}
