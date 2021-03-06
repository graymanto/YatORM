﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using YatORM.Extensions;

namespace YatORM
{
    public class QueryTranslator : ExpressionVisitor
    {
        private readonly string _orderBy = string.Empty;

        private StringBuilder _sb;

        private int? _skip;

        private int? _take;

        private string _whereClause = string.Empty;

        public int? Skip
        {
            get
            {
                return this._skip;
            }
        }

        public int? Take
        {
            get
            {
                return this._take;
            }
        }

        public string OrderBy
        {
            get
            {
                return this._orderBy;
            }
        }

        public string WhereClause
        {
            get
            {
                return this._whereClause;
            }
        }

        public string Translate(Expression expression)
        {
            this._sb = new StringBuilder();
            Visit(expression);
            this._whereClause = this._sb.ToString();
            return this._whereClause;
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }

            return e;
        }

        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.DeclaringType == typeof(Queryable) && m.Method.Name == "Where")
            {
                Visit(m.Arguments[0]);
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                Visit(lambda.Body);
                return m;
            }

            if (m.Method.Name == "Take")
            {
                if (ParseTakeExpression(m))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "Skip")
            {
                if (ParseSkipExpression(m))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderBy")
            {
                if (ParseOrderByExpression(m, "ASC"))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }
            else if (m.Method.Name == "OrderByDescending")
            {
                if (ParseOrderByExpression(m, "DESC"))
                {
                    var nextExpression = m.Arguments[0];
                    return Visit(nextExpression);
                }
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    this._sb.Append(" NOT ");
                    Visit(u.Operand);
                    break;
                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }

        /// <summary>
        /// </summary>
        /// <param name="b">
        /// </param>
        /// <returns>
        /// </returns>
        protected override Expression VisitBinary(BinaryExpression b)
        {
            this._sb.Append("(");
            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                    this._sb.Append(" AND ");
                    break;

                case ExpressionType.AndAlso:
                    this._sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                    this._sb.Append(" OR ");
                    break;

                case ExpressionType.OrElse:
                    this._sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    this._sb.Append(IsNullConstant(b.Right) ? " IS " : " = ");

                    break;

                case ExpressionType.NotEqual:
                    this._sb.Append(IsNullConstant(b.Right) ? " IS NOT " : " <> ");

                    break;

                case ExpressionType.LessThan:
                    this._sb.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    this._sb.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    this._sb.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    this._sb.Append(" >= ");
                    break;

                default:
                    throw new NotSupportedException(
                        string.Format("The binary operator '{0}' is not supported", b.NodeType));
            }

            Visit(b.Right);
            this._sb.Append(")");
            return b;
        }

        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;

            if (q == null && c.Value == null)
            {
                this._sb.Append("NULL");
            }
            else if (q == null)
            {
                switch (Type.GetTypeCode(c.Value.GetType()))
                {
                    case TypeCode.Boolean:
                        this._sb.Append(((bool)c.Value) ? 1 : 0);
                        break;

                    case TypeCode.String:
                    case TypeCode.DateTime:
                        this._sb.Append("'");
                        this._sb.Append(c.Value);
                        this._sb.Append("'");
                        break;

                    case TypeCode.Object:
                        throw new NotSupportedException(
                            string.Format("The constant for '{0}' is not supported", c.Value));

                    default:
                        this._sb.Append(c.Value);
                        break;
                }
            }

            return c;
        }

        protected override Expression VisitMember(MemberExpression m)
        {
            if (m.Expression == null)
                throw new ArgumentNullException("m", "Member expression is null");

            if (m.Expression.NodeType == ExpressionType.Parameter)
            {
                this._sb.Append(m.Member.Name);
                return m;
            }

            try
            {
                var member = Expression.Convert(m, typeof(object));
                var getter = Expression.Lambda<Func<object>>(member);
                var valueResolver = getter.Compile();

                var type = m.Type;

                if (type == typeof(string) || type == typeof(DateTime) || type == typeof(Guid))
                {
                    this._sb.AppendInSingleQuotes(valueResolver());
                }
                else if (type == typeof(bool))
                {
                    this._sb.Append((bool)valueResolver() ? 1 : 0);
                }
                else
                {
                    this._sb.Append(valueResolver());
                }

                return m;
            }
            catch (Exception inner)
            {
                throw new NotSupportedException(
                    string.Format("The member '{0}' is not supported", m.Member.Name),
                    inner);
            }
        }

        protected bool IsNullConstant(Expression exp)
        {
            return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
        }

        private bool ParseOrderByExpression(MethodCallExpression expression, string order)
        {
            ////var unary = (UnaryExpression)expression.Arguments[1];
            ////var lambdaExpression = (LambdaExpression)unary.Operand;

            ////lambdaExpression = (LambdaExpression)Evaluator.PartialEval(lambdaExpression);

            ////var body = lambdaExpression.Body as MemberExpression;
            ////if (body != null)
            ////{
            ////    if (string.IsNullOrEmpty(_orderBy))
            ////    {
            ////        _orderBy = string.Format("{0} {1}", body.Member.Name, order);
            ////    }
            ////    else
            ////    {
            ////        _orderBy = string.Format("{0}, {1} {2}", _orderBy, body.Member.Name, order);
            ////    }

            ////    return true;
            ////}
            return false;
        }

        private bool ParseTakeExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                this._take = size;
                return true;
            }

            return false;
        }

        private bool ParseSkipExpression(MethodCallExpression expression)
        {
            var sizeExpression = (ConstantExpression)expression.Arguments[1];

            int size;
            if (int.TryParse(sizeExpression.Value.ToString(), out size))
            {
                this._skip = size;
                return true;
            }

            return false;
        }
    }
}