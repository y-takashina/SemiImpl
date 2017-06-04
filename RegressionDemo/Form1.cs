using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.Linq;
using static RegressionDemo.Regression;

namespace RegressionDemo
{
    public partial class Form1 : Form
    {
        private double[] _input;
        private double[] _output;

        public Form1()
        {
            InitializeComponent();
            chart1.Series.Clear();
        }

        private void _plotPoints(IEnumerable<(double x, double y)> points, string legend)
        {
            chart1.Series.Remove(chart1.Series.FindByName(legend));
            chart1.Series.Add(legend);
            chart1.Series[legend].ChartType = SeriesChartType.Line;
            chart1.ChartAreas[0].AxisY.Minimum = -1.5;
            chart1.ChartAreas[0].AxisY.Maximum = 1.5;
            foreach (var point in points)
            {
                chart1.Series[legend].Points.AddXY(point.x, point.y);
            }
        }

        private void _plotFormula(Func<double, double> f, double[] input, string legend)
        {
            var points = input.Select(x => (x, f(x)));
            _plotPoints(points, legend);
        }

        /// <summary>
        /// Create and plot data.
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            // load constants
            var n = int.TryParse(textBox1.Text, out int nBuffer) ? nBuffer : 1000;
            var deviation = double.TryParse(textBox2.Text, out double deviationBuffer) ? deviationBuffer : 0.2;

            // generate data
            var rand = new Random();
            double NoisySin(double x) => Math.Sin(x) + deviation * rand.NextGaussian();
            _input = Enumerable.Range(0, n).Select(i => 2 * Math.PI / n * i).ToArray();
            _output = _input.Select(NoisySin).ToArray();

            // plot data
            _plotPoints(_input.Zip(_output, (x, y) => (x, y)), "data");
            _plotFormula(Math.Sin, _input, "sin");
            chart1.Series["data"].Color = Color.SandyBrown;
            chart1.Series["sin"].Color = Color.SteelBlue;
        }

        /// <summary>
        /// Regress by polynomial functions.
        /// </summary>
        private void button2_Click(object sender, EventArgs e)
        {
            var m = int.TryParse(textBox3.Text, out int mBuffer) ? mBuffer : 10;
            var bases = PolynomialBases(m);
            var predict = Regress(bases, _input, _output);
            _plotFormula(predict, _input, "approximation");
            chart1.Series["approximation"].Color = Color.Red;
        }

        /// <summary>
        /// Regress by Gaussian functions.
        /// </summary>
        private void button3_Click(object sender, EventArgs e)
        {
            var m = int.TryParse(textBox3.Text, out int mBuffer) ? mBuffer : 10;
            var sigma = double.TryParse(textBox5.Text, out double sigmaBuffer) ? sigmaBuffer : 0.05;
            var bases = GaussianBases(m, sigma, 0, 2 * Math.PI);
            var predict = Regress(bases, _input, _output);
            _plotFormula(predict, _input, "approximation");
            chart1.Series["approximation"].Color = Color.Red;
        }

        /// <summary>
        /// Regress by Logistic functions.
        /// </summary>
        private void button4_Click(object sender, EventArgs e)
        {
            var m = int.TryParse(textBox3.Text, out int mBuffer) ? mBuffer : 10;
            var sigma = double.TryParse(textBox6.Text, out double sigmaBuffer) ? sigmaBuffer : 0.05;
            var bases = LogisticBases(m, sigma, 0, 2 * Math.PI);
            var predict = Regress(bases, _input, _output);
            _plotFormula(predict, _input, "approximation");
            chart1.Series["approximation"].Color = Color.Red;
        }
    }
}