using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace RegressionDemo
{
    public static class Regression
    {
        public static IEnumerable<Func<double, double>> PolynomialBases(int m)
        {
            Func<double, double> Generator(int i) => x => Math.Pow(x, i);
            return Enumerable.Range(0, m).Select(Generator);
        }

        public static IEnumerable<Func<double, double>> GaussianBases(int m, double sigma, double min, double max)
        {
            var d = max - min;
            Func<double, double> Generator(int i) => x => Math.Exp(-(x - min - i * d / m) * (x - min - i * d / m) / 2 / sigma);
            return Enumerable.Range(0, m).Select(Generator);
        }

        public static IEnumerable<Func<double, double>> LogisticBases(int m, double sigma, double min, double max)
        {
            var d = max - min;
            Func<double, double> Generator(int i) => x => 1 / (1 + Math.Exp(-(x - min - i * d / m) / sigma));
            return Enumerable.Range(0, m).Select(Generator);
        }

        public static Func<double, double> Regress(IEnumerable<Func<double, double>> bases, double[] input, double[] output)
        {
            var designMatrix = DesignMatrix(input, bases);
            var weights = designMatrix.Solve(output, true);
            return LinearCombination(weights, bases);
        }

        public static double[,] DesignMatrix(IEnumerable<double> data, IEnumerable<Func<double, double>> bases)
        {
            return data.Select(v => bases.Select(b => b(v)).ToArray()).ToArray().ToMatrix();
        }

        public static Func<double, double> LinearCombination(double[] weights, IEnumerable<Func<double, double>> bases)
        {
            return x => weights.Zip(bases, (w, b) => w * b(x)).Sum();
        }
    }
}