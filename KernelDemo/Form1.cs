using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using Accord.Math;
using Accord.Math.Optimization;

namespace KernelDemo
{
    public partial class Form1 : Form
    {
        private readonly List<(double[] input, string output)> _data;
        private Func<double[], double[], double> _kernel;

        public Form1()
        {
            InitializeComponent();

            _data = new List<(double[], string)>();
            using (var sr = new StreamReader(Path.Combine("..", "..", "..", "data", "iris.csv")))
            {
                while (!sr.EndOfStream)
                {
                    var line = sr.ReadLine().Split(',');
                    var input = line.Take(4).Select(double.Parse).ToArray();
                    var output = line.Last();
                    _data.Add((input, output));
                }
            }

            _kernel = Kernel.Linear;

            chart1.Series.Clear();
            _updateChart(checkBox1.Text, true, 0, 1);
            _updateChart(checkBox2.Text, true, 0, 1);
        }

        private void _updateChart(string name, bool visible, int axis1, int axis2)
        {
            chart1.Series.Remove(chart1.Series.FindByName(name));
            var selected = _data.Where(io => io.output == name).Select(io => (io.input[axis1], io.input[axis2]));
            if (visible)
            {
                chart1.PlotPoints(selected, name, markerSize: 10);
            }
        }

        private List<(double x, double y, double z)> _classify(string positiveClass, string negativeClass, Func<double[], double[], double> kernel, double lambda)
        {
            var nPositive = _data.Count(item => item.output == positiveClass);
            var nNegative = _data.Count(item => item.output == negativeClass);
            var outputs = new double[nPositive + nNegative];
            for (var i = 0; i < nPositive + nNegative; i++)
            {
                outputs[i] = i < nPositive ? +1 : -1;
            }
            var positives = _data.Where(item => item.output == positiveClass).Select(item => item.input.Take(2).ToArray());
            var negatives = _data.Where(item => item.output == negativeClass).Select(item => item.input.Take(2).ToArray());
            var inputs = positives.Concat(negatives).ToArray();
            var alpha = Optimizer.GradientDescent(inputs, outputs, kernel, lambda);
            var eval = Kernel.EvalFunc(inputs, outputs, kernel, lambda, alpha);

            var interval = 0.05;
            var minX = chart1.ChartAreas[0].AxisX.Minimum;
            var maxX = chart1.ChartAreas[0].AxisX.Maximum;
            var minY = chart1.ChartAreas[0].AxisY.Minimum;
            var maxY = chart1.ChartAreas[0].AxisY.Maximum;
            var nCols = (int)((maxX - minX) / interval);
            var nRaws = (int)((maxY - minY) / interval);

            var points = new List<(double, double, double)>();
            for (var i = 0; i < nRaws; i++)
            {
                var y = minY + i * interval;
                for (var j = 0; j < nCols; j++)
                {
                    var x = minX + j * interval;
                    var z = eval(new[] { x, y });
                    points.Add((x, y, z));
                }
            }
            return points;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var count = (checkBox1.Checked ? 1 : 0) + (checkBox2.Checked ? 1 : 0) + (checkBox3.Checked ? 1 : 0);
            if (count != 2)
            {
                MessageBox.Show("Only two classes can be classified.");
                return;
            }

            var positiveClass = checkBox1.Checked ? checkBox1.Text : checkBox2.Text;
            var negativeClass = checkBox3.Checked ? checkBox3.Text : checkBox2.Text;
            if (!double.TryParse(textBox1.Text, out double _)) textBox1.Text = "1.0";
            var lambda = double.Parse(textBox1.Text);
            var points = _classify(positiveClass, negativeClass, _kernel, lambda);

            var border = points.Where(p => Math.Abs(p.z) < 0.05).Select(p => (p.x, p.y));
            var borderPositive = points.Where(p => Math.Abs(p.z + 1) < 0.05).Select(p => (p.x, p.y));
            var borderNegative = points.Where(p => Math.Abs(p.z - 1) < 0.05).Select(p => (p.x, p.y));
            chart1.PlotPoints(border, "border", markerSize: 5);
            chart1.PlotPoints(borderPositive, "+1", markerSize: 5);
            chart1.PlotPoints(borderNegative, "-1", markerSize: 5);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            _updateChart(checkBox1.Text, checkBox1.Checked, 0, 1);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            _updateChart(checkBox2.Text, checkBox2.Checked, 0, 1);
        }

        private void checkBox3_CheckedChanged(object sender, EventArgs e)
        {
            _updateChart(checkBox3.Text, checkBox3.Checked, 0, 1);
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton1.Checked) _kernel = Kernel.Linear;
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton2.Checked) _kernel = Kernel.Polynomial(1, 1, 2);
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton3.Checked) _kernel = Kernel.Sigmoid(0.01, 0.01);
        }

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            if (radioButton4.Checked) _kernel = Kernel.Gaussian(1);
        }
    }
}