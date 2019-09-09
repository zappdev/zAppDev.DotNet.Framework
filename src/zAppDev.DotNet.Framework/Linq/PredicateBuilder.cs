// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Linq;
using System.Linq.Expressions;

namespace zAppDev.DotNet.Framework.Linq
{
    public static class PredicateBuilder
	{
		public static Expression<Func<T, bool>> True<T>() { return f => true; }
		public static Expression<Func<T, bool>> False<T>() { return f => false; }

		public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expr1,
															Expression<Func<T, bool>> expr2)
		{
			if (expr1.Body.NodeType == ExpressionType.Constant
				&& (((ConstantExpression)expr1.Body)).Value != null
				&& (bool)(((ConstantExpression)expr1.Body)).Value == false)
			{
				return expr2;
			}

			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.OrElse(expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expr1,
															 Expression<Func<T, bool>> expr2)
		{
			if(expr1.Body.NodeType == ExpressionType.Constant
				&& (((ConstantExpression)expr1.Body)).Value != null
				&& (bool)(((ConstantExpression)expr1.Body)).Value == true)
			{
				return expr2;
			}

			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.AndAlso(expr1.Body, invokedExpr), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> AndNot<T>(this Expression<Func<T, bool>> expr1,
															 Expression<Func<T, bool>> expr2)
		{
			if (expr1.Body.NodeType == ExpressionType.Constant
				&& (((ConstantExpression)expr1.Body)).Value != null
				&& (bool)(((ConstantExpression)expr1.Body)).Value == true)
			{
				var invokedExpr2 = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
				return Expression.Lambda<Func<T, bool>>
					  (Expression.Not(invokedExpr2), expr1.Parameters);
			}

			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.AndAlso(expr1.Body, Expression.Not(invokedExpr)), expr1.Parameters);
		}

		public static Expression<Func<T, bool>> OrNot<T>(this Expression<Func<T, bool>> expr1,
															 Expression<Func<T, bool>> expr2)
		{
			if (expr1.Body.NodeType == ExpressionType.Constant
				&& (((ConstantExpression)expr1.Body)).Value != null
				&& (bool)(((ConstantExpression)expr1.Body)).Value == false)
			{
				var invokedExpr2 = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
				return Expression.Lambda<Func<T, bool>>
					  (Expression.Not(invokedExpr2), expr1.Parameters);
			}

			var invokedExpr = Expression.Invoke(expr2, expr1.Parameters.Cast<Expression>());
			return Expression.Lambda<Func<T, bool>>
				  (Expression.OrElse(expr1.Body, Expression.Not(invokedExpr)), expr1.Parameters);
		}
	}
}