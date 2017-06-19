using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace KernelDemo
{
    public static class Optimizer
    {
        public static double[] GradientDescent(IEnumerable<double[]> inputs, double[] outputs, Func<double[], double[], double> kernel, double lambda, int maxItr = 10000)
        {
            var beta = 1.0;
            var eta = lambda * 1e-6;
            var n = outputs.GetLength(0);
            var diag = Matrix.Diagonal(outputs);
            var kerMat = Kernel.KernelMatrix(inputs, kernel);
            var alpha = Vector.Ones(n).Multiply(lambda / 2);
            for (var itr = 0; itr < maxItr; itr++)
            {
                var dot = alpha.Dot(outputs);
                // object
                var grad = alpha.Dot(diag).Dot(kerMat).Dot(diag).Subtract(1);
                for (var i = 0; i < n; i++)
                {
                    // penalty
                    grad[i] += beta * dot * outputs[i];
                    grad[i] += beta / ((lambda - alpha[i]) * (lambda - alpha[i]));
                    grad[i] -= beta / (alpha[i] * alpha[i]);
                }
                alpha = alpha.Subtract(grad.Multiply(eta));
                if (alpha.Any(Double.IsNaN)) throw new Exception("NaN");
                if (alpha.Any(Double.IsInfinity)) throw new Exception("Inf");
                if (grad.Abs().Sum() < 1.0) break;
                beta *= 0.99;
            }
            return alpha;
        }
    }
}