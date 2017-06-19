using System;
using System.Collections.Generic;
using System.Linq;
using Accord.Math;

namespace KernelDemo
{
    public static class Optimizer
    {
        public static double[] GradientDescent(IEnumerable<double[]> inputs, double[] outputs, Func<double[], double[], double> kernel, double lambda)
        {
            var eta = lambda * 1e-6;
            var n = outputs.GetLength(0);
            var yMat = Matrix.Diagonal(outputs);
            var kerMat = Kernel.KernelMatrix(inputs, kernel);
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
                if (alpha.Any(Double.IsNaN)) throw new Exception("NaN");
                if (alpha.Any(Double.IsInfinity)) throw new Exception("Inf");
                if (grad.Abs().Sum() < 1.0) break;
                beta *= 0.99;
            }
            return alpha;
        }
    }
}