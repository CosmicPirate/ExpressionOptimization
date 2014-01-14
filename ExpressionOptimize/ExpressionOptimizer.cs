using System;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace ExpressionOptimize
{
    public class ExpressionOptimizer : ExpressionVisitor
    {
        private class MethodCall
        {
            public MethodCallExpression Expr = null;
            public int CallCount = 0;
            public ParameterExpression Parameter = null;
            public Expression ParameterExpression = null;
            public MethodCall(MethodCallExpression e, ParameterExpression parameter)
            {
                Expr = e; CallCount = 1; Parameter = parameter;
            }
            public void MoreInvokations()
            {
                ++CallCount;
            }
        }

        private string _functionName = null;
        private Dictionary<string, MethodCall> _methodCalls = null;
        private List<Expression> _innerLambdaParams = null;

        /**
         * приводит лямбду (x, y, ...) => ... к виду:  (x, y, ...) => fn(p, q, ...)
         *  где fn - оптимизированная лямбда, p, q - предварительно вычисленные выражения
         *  
         *  пример:
         *  Func<int, int, Func<int, int, int, int>, int> fn = (int x, int y, Func<int, int, int, int> Foo) => Foo(F(x), F(y), F(2 * y));
         *  Func<int, int, int> optimizedLambda = (int x, int y) => fn(1, 2, (int p, int q, int l) => p > q ? p : (p < l ? l : q));
         */
        public Expression Optimize(LambdaExpression lambda, string functionName)
        {
            if (_methodCalls != null)
                _methodCalls.Clear();
            else
                _methodCalls = new Dictionary<string, MethodCall>();

            _innerLambdaParams = new List<Expression>();

            _functionName = functionName;

            //  получаем оптимизированную лямбду
            Expression fn = BuildInnerLambda(lambda);

            //  копируем параметры для новой лямбды
            ParameterExpression[] parameters = new ParameterExpression[lambda.Parameters.Count];
            lambda.Parameters.CopyTo(parameters, 0);

            //  собираем новую лямбду
            Expression optimizedLambda = Expression.Lambda(
                Expression.Invoke(fn, _innerLambdaParams),
                parameters
            );

            return optimizedLambda;
        }

        private Expression BuildInnerLambda(LambdaExpression lambda)
        {
            LambdaExpression fn = (LambdaExpression)Visit(lambda);

            List<ParameterExpression> parameters = new List<ParameterExpression>();
            foreach(var val in _methodCalls.Values)
            {
                parameters.Add(val.Parameter);
                _innerLambdaParams.Add(val.Expr);
            }

            fn = Expression.Lambda(fn.Body, parameters);

            return fn;
        }

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            Expression retExpr = null;

            if (node.Method.Name == _functionName)
            {
                var str = node.ToString();
                MethodCall call;
                if (_methodCalls.TryGetValue(str, out call))
                {
                    ++call.CallCount;
                }
                else
                {
                    call = new MethodCall(node, Expression.Parameter(node.Type, "param" + (_methodCalls.Count)));
                    _methodCalls.Add(str, call);
                }

                retExpr = call.Parameter;
            }
            else
                retExpr = base.VisitMethodCall(node);

            return retExpr;
        }

    }
}
