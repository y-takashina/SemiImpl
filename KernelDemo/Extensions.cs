using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace KernelDemo
{
    public static class Extensions
    {
        public static void PlotPoints(
            this Chart chart, 
            IEnumerable<(double x, double y)> points, 
            string name,
            SeriesChartType chartType = SeriesChartType.Point,
            ChartDashStyle dashStyle = ChartDashStyle.Solid,
            MarkerStyle markerStyle = MarkerStyle.Circle,
            int borderWidth = 2,
            int markerSize = 2,
            double xMin = double.NegativeInfinity,
            double xMax = double.PositiveInfinity,
            double yMin = double.NegativeInfinity,
            double yMax = double.PositiveInfinity,
            bool removeExisting = true
        )
        {
            if (removeExisting)
            {
                chart.Series.Remove(chart.Series.FindByName(name));
            }
            if (chart.Series.All(series => series.Name != name))
            {
                chart.Series.Add(name);
            }
            chart.Series[name].ChartType = chartType;
            chart.Series[name].BorderDashStyle = dashStyle;
            chart.Series[name].MarkerStyle = markerStyle;
            chart.Series[name].BorderWidth = borderWidth;
            chart.Series[name].MarkerSize = markerSize;
            if (!double.IsNegativeInfinity(xMin)) chart.ChartAreas[0].AxisX.Minimum = xMin;
            if (!double.IsPositiveInfinity(xMax)) chart.ChartAreas[0].AxisX.Maximum = xMax;
            if (!double.IsNegativeInfinity(yMin)) chart.ChartAreas[0].AxisY.Minimum = yMin;
            if (!double.IsPositiveInfinity(yMax)) chart.ChartAreas[0].AxisY.Maximum = yMax;
            foreach (var point in points)
            {
                chart.Series[name].Points.AddXY(point.x, point.y);
            }
        }
    }
}