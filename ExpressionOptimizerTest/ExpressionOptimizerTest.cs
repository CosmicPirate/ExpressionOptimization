using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ExpressionOptimize;
using System.Linq.Expressions;

namespace ExpressionOptimizerTest
{
    [TestClass]
    public class ExpressionOptimizerTest
    {
        private int callCount = 0;

        [TestMethod]
        public void TestMethod1()
        {
            ExpressionOptimizer optimizer = new ExpressionOptimizer();

            Expression<Func<int, int, int>> ex = (int x, int y) => F(x) > F(y) ? F(x) : (F(x) < F(2 * y) ? F(2 * y) : F(y));
            var a = ex.Compile().Invoke(1, 2);

            Object[] lambdaParams = { 1, 2 };
            callCount = 0;
            int result = (int)Program.OptimizedCalculation(ex, lambdaParams, "F");

            Assert.AreEqual(a, result);
            Assert.AreEqual(callCount, 3);


            Expression<Func<int, int>> ex1 = (int x) => F(F(x)) + F(x) * F(F(x));
            a = ex1.Compile().Invoke(2);
            lambdaParams = new Object[1] { 2 };
            callCount = 0;
            result = (int)Program.OptimizedCalculation(ex1, lambdaParams, "F");

            Assert.AreEqual(a, result);
            Assert.AreEqual(callCount, 3);  //  плюс один внутренний вызов
        }

        public int F(int x)
        {
            ++callCount;
            return x << 2;
        }
    }



}
