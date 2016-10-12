using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Evalyn
{
	public class VariableDeclarationStatementWalker : WalkerBase<IEnumerable<Expression>>
	{
		public VariableDeclarationStatementWalker(Context context) : base(SyntaxKind.LocalDeclarationStatement, context)
		{

		}

		public IEnumerable<ParameterExpression> Variables { get; private set; }

		public override void VisitLocalDeclarationStatement(Microsoft.CodeAnalysis.CSharp.Syntax.LocalDeclarationStatementSyntax node)
		{
			var typeinfo = this.SemanticModel.GetTypeInfo(node.Declaration.Type);
			var type = this.Context.ResultAssembly.GetEvalType(typeinfo.Type);

			var variables = new List<ParameterExpression>();
			var initializers = new List<Expression>();

			foreach (var declarator in node.Declaration.Variables)
			{
				var variable = Expression.Variable(type.Runtime, declarator.Identifier.Text);

				variables.Add(variable);
				this.Context.Locals.Add(declarator.Identifier.Text, variable);

				if (declarator.Initializer != null)
				{
					var walker = new ExpressionWalker(this.Context);
					var decl = walker.BuildWithResult(declarator.Initializer);
					initializers.Add(Expression.Assign(variable, Expression.Convert(decl,variable.Type)));
				}
				else
				{
					initializers.Add(Expression.Empty());
				}
			}

			this.Variables = variables;
			this.Result = initializers;
		}
	}
}
