using System;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Evalyn
{
	public class SourceWalker : WalkerBase<Expression>
	{
		public static SyntaxKind[] SupportedStatements { get; private set; } = new[]
		{
			SyntaxKind.ThisExpression,
			SyntaxKind.BaseExpression,
			SyntaxKind.IdentifierName,
			SyntaxKind.SimpleMemberAccessExpression,
		};

		public SourceWalker(Context context) : base(SupportedStatements,context)
		{
		}


		public override void VisitMemberAccessExpression(Microsoft.CodeAnalysis.CSharp.Syntax.MemberAccessExpressionSyntax node)
		{
			var info = this.SemanticModel.GetSymbolInfo(node).Symbol;
			var type = this.Context.ResultAssembly.GetEvalType(info.ContainingType);

			var instanceExpression = this.Context.GetInvokingInstance(node);

			if (info.Kind == SymbolKind.Property)
			{
				var propsymbol = info as IPropertySymbol;
				if (type.IsRuntime())
				{
					var propinfo = type.Runtime.GetPropertyInfo(propsymbol);
					this.Result = Expression.Property(instanceExpression, propinfo);
				}
				else
				{
					this.Result = this.Context.InvokeGetProperty(propsymbol, instanceExpression);
				}
			}
			else if (info.Kind == SymbolKind.Field)
			{
				var propfield = info as IFieldSymbol;
				if (type.IsRuntime())
				{
					this.Result = Expression.Field(instanceExpression, propfield.Name);
				}
				else
				{
					this.Result = this.Context.InvokeGetField(propfield, instanceExpression);
				}
			}
		}

		public override void VisitThisExpression(ThisExpressionSyntax node)
		{
			this.Result = this.Context.This;
		}

		public override void VisitBaseExpression(BaseExpressionSyntax node)
		{
			this.Result = this.Context.This;
		}

		public override void VisitIdentifierName(Microsoft.CodeAnalysis.CSharp.Syntax.IdentifierNameSyntax node)
		{
			var info = this.SemanticModel.GetSymbolInfo(node).Symbol;

			if (info.Kind == SymbolKind.Parameter || info.Kind == SymbolKind.Local)
			{
				this.Result = this.Context.Locals[info.Name];
			}
			else if (info.Kind == SymbolKind.Field)
			{
				this.Result = this.Context.InvokeGetField(info as IFieldSymbol, this.Context.This);
			}
			else if (info.Kind == SymbolKind.Property)
			{
				this.Result = this.Context.InvokeGetProperty(info as IPropertySymbol, this.Context.This);
			}
			else
			{
				// Todo static
				//return this.VisitMemberAccessExpression(;
				throw new ArgumentException("Identifier not found : " + node.Identifier.Text);
			}
		}
	}
}
