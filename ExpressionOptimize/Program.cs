using System;
using System.Linq.Expressions;
using System.Collections;
using System.Collections.Generic;

namespace ExpressionOptimize
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Func<int, int, int> expression = (int x, int y) => F(x) > F(y) ? F(x) : (F(x) < F(2 * y) ? F(2 * y) : F(y));
            Console.WriteLine("до оптимизации");
            var a = expression(1, 2);

            //Func<int, int, Func<int, int, int, int>, int> fn = (int x, int y, Func<int, int, int, int> Foo) => Foo(F(x), F(y), F(2 * y));
            //Func<int, int, int> optimizedLambda = (int x, int y) => fn(1, 2, (int p, int q, int l) => p > q ? p : (p < l ? l : q));
            //var b = optimizedLambda(1, 2);

            Expression<Func<int, int, int>> ex = (int x, int y) => F(x) > F(y) ? F(x) : (F(x) < F(2 * y) ? F(2 * y) : F(y));

            Object[] lambdaParams = { 1, 2 };
            Console.WriteLine("после оптимизации");
            int result = (int)OptimizedCalculation(ex, lambdaParams, "F");

            return;
        }

        public static int F(int x)
        {
            Console.WriteLine("F(" + x + ")");
            return x << 2;
        }



        public static Object OptimizedCalculation(LambdaExpression lambda, Object[] lambdaParams, string func)
        {
            var optimized = OptimizeLambda(lambda, func);
            var result = CalculateLabda(optimized, lambdaParams);
            return result;
        }

        public static LambdaExpression OptimizeLambda(LambdaExpression lambda, string func)
        {
            ExpressionOptimizer optimizer = new ExpressionOptimizer();
            return (LambdaExpression)optimizer.Optimize(lambda, func);
        }

        public static Object CalculateLabda(LambdaExpression lambda, Object[] lambdaParams)
        {
            var compiledLambda = lambda.Compile();
            var invokeMethod = compiledLambda.GetType().GetMethod("Invoke");

            if (invokeMethod != null)
                return invokeMethod.Invoke(compiledLambda, lambdaParams);
            else
                throw new Exception();
        }

    }

}