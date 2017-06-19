using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace KernelDemo
{
    public static class Kernel
    {
        public static Func<double[], double[], double> Linear => (v1, v2) => v1.Dot(v2);
        public static Func<double[], double[], double> Polynomial(int a, int b, int d) => (v1, v2) => Math.Pow(a * v1.Dot(v2) + b, d);
        public static Func<double[], double[], double> Gaussian(double sigma) => (v1, v2) => Math.Exp(-0.5 / sigma / sigma * v1.Subtract(v2).Euclidean());
        public static Func<double[], double[], double> Sigmoid(double sigma) => (v1, v2) => 1.0 / (1 + Math.Exp(-v1.Dot(v2) / sigma));
        public static Func<double[], double[], double> Sigmoid(double a, double b) => (v1, v2) => Math.Tanh(a * v1.Dot(v2) + b);

        public static double[,] KernelMatrix(IEnumerable<double[]> inputs, Func<double[], double[], double> kernel)
        {
            var array = inputs as double[][] ?? inputs.ToArray();
            var n = array.Length;
            var matrix = new double[n, n];
            for (var i = 0; i < n; i++)
            {
                for (var j = 0; j < n; j++)
                {
                    matrix[i, j] = kernel(array[i], array[j]);
                }
            }
            return matrix;
        }

        public static Func<double[], double[]> KernelVector(IEnumerable<double[]> inputs, Func<double[], double[], double> kernel)
        {
            return v1 => inputs.Select(v2 => kernel(v1, v2)).ToArray();
        }

        public static Func<double[], double> EvalFunc(IEnumerable<double[]> inputs, double[] outputs, Func<double[], double[], double> kernel, double lambda, double[] alpha)
        {
            var kerVec = KernelVector(inputs, kernel);
            var kerMat = KernelMatrix(inputs, kernel);
            var yMat = Matrix.Diagonal(outputs);
            var flipped = alpha.Subtract(lambda);
            var diag = Matrix.Diagonal(outputs.Subtract(kerMat.Dot(yMat).Dot(alpha)));
            var b = alpha.Dot(diag).Dot(flipped) / alpha.Dot(flipped);
            return x => alpha.Dot(yMat).Dot(kerVec(x)) + b;
        }

        public static double[] GradientDescent(IEnumerable<double[]> inputs, double[] outputs, Func<double[], double[], double> kernel, double lambda)
        {
            var eta = lambda * 1e-6;
            var n = outputs.GetLength(0);
            var yMat = Matrix.Diagonal(outputs);
            var kerMat = KernelMatrix(inputs, kernel);
            var alpha = Vector.Ones(n).Multiply(lambda / 2);
            var beta = 1.0;
            var dot = alpha.Dot(outputs);
            for (var itr = 0; itr < 10000; itr++)
            {
                var grad = alpha.Dot(yMat).Dot(kerMat).Dot(yMat);
                for (var i = 0; i < n; i++)
                {
                    grad[i] -= 1;
                    // penalty
                    grad[i] += beta * dot * outputs[i];
                    grad[i] += beta / ((lambda - alpha[i]) * (lambda - alpha[i]));
                    grad[i] -= beta / (alpha[i] * alpha[i]);
                }
                alpha = alpha.Subtract(grad.Multiply(eta));
                if (alpha.Any(double.IsNaN)) throw new Exception("NaN");
                if (alpha.Any(double.IsInfinity)) throw new Exception("Inf");
                if (grad.Abs().Sum() < 1.0) break;
                beta *= 0.99;
            }
            return alpha;
        }
    }
}