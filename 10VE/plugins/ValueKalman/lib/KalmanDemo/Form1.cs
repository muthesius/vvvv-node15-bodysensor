///////////////////////////////////////////////////////////////////////////////
//
//  Form1.cs
//
//  By Philip R. Braica (HoshiKata@aol.com, VeryMadSci@gmail.com)
//
//  Fully open source, public domain license, have fun.
//  http://creativecommons.org/licenses/publicdomain/
///////////////////////////////////////////////////////////////////////////////

// Using.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

// Namespace
namespace KalmanDemo
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            NumericUpDown[] nds = { numericUpDown1, numericUpDown2, numericUpDown3, numericUpDown4, numericUpDown5, numericUpDown6 };

            for (int i = 0; i < nds.Length; i++)
            {
                nds[i].ValueChanged += new EventHandler(numericUpdown_ValueChanged);
            }
        }

        /// <summary>
        /// Last button.
        /// </summary>
        Button m_lastButton = null;

        /// <summary>
        /// Retrigger last button.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void numericUpdown_ValueChanged(object sender, EventArgs e)
        {
            if (m_lastButton == button1) button1_Click(sender, e);
            if (m_lastButton == button2) button2_Click(sender, e);
            if (m_lastButton == button3) button3_Click(sender, e);
            if (m_lastButton == button4) button4_Click(sender, e);
            if (m_lastButton == button6) button6_Click(sender, e);
            if (m_lastButton == button7) button7_Click(sender, e);
            if (m_lastButton == button8) button8_Click(sender, e);
        } 

        /// <summary>
        /// Test 1.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void button1_Click(object sender, EventArgs e)
        {
            m_lastButton = button1;
            double[] idealNoNoise = new double[1000];
            double half = idealNoNoise.Length / 2;

            // Generate a perfect idealized flight path.
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Rocket test, max height is half*half*10 = 500*500 or 250,000 feet.
                idealNoNoise[i] = (half * half) - ((i - half) * (i - half));
            }
            formTest(idealNoNoise);
        }

        /// <summary>
        /// Line test.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            m_lastButton = button2;

            double[] idealNoNoise = new double[100];
            double half = idealNoNoise.Length / 2;

            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                idealNoNoise[i] = 10 + 0.05 * (double)i;
            }
            formTest(idealNoNoise);
        }

        /// <summary>
        /// Two lines, a turning point.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            m_lastButton = button3;
            double[] idealNoNoise = new double[1000];
            double half = idealNoNoise.Length / 2;

            idealNoNoise[0] = 10;
            for (int i = 1; i < idealNoNoise.Length; i++)
            {
                if (i < idealNoNoise.Length / 2)
                {
                    idealNoNoise[i] = idealNoNoise[i - 1] + 0.2;
                }
                else
                {
                    idealNoNoise[i] = idealNoNoise[i - 1] - 0.2;
                }
            }
            formTest(idealNoNoise);
        }
        /// <summary>
        /// Parabola 2D.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button8_Click(object sender, EventArgs e)
        {
            m_lastButton = button8;
            double[] idealNoNoise = new double[1000];
            double[] idealNoNoiseVelocity = new double[idealNoNoise.Length];
            double half = idealNoNoise.Length / 2;

            // Generate a perfect idealized flight path.
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Rocket test, max height is half*half*10 = 500*500 or 250,000 feet.
                idealNoNoise[i] = (half * half) - ((i - half) * (i - half));
                idealNoNoiseVelocity[i] = -2 * (i - half);
            }
            formTest2D(idealNoNoise, idealNoNoiseVelocity);
        }

        /// <summary>
        /// Form a test.
        /// </summary>
        /// <param name="idealNoNoise"></param>
        protected void formTest(double [] idealNoNoise)
        {
            Random.Random r = new Random.Random();
            // Add in wind, auto-pilot on our rocket always successfully adapts so
            // that the effect by the wind is guassian process noise.
            double[] withProcessNoise = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Add in 1 foot = 1 std. deviation of process noise.
                withProcessNoise[i] = idealNoNoise[i] + r.NextGuass_Double(0, (double)numericUpDown5.Value);
            }

            // Add in measurement noise, the GPS on the rocket measures less often
            // than our data but we can just skip things.
            // Assume a very good GPS, accurate to +/- 6 feet even at these speeds.
            // 2 foot = 1 std dev. is about +/- 6 feet
            double[] withAllNoise = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Add in 1 foot = 1 std. deviation of process noise.
                withAllNoise[i] = withProcessNoise[i] + r.NextGuass_Double(0, (double)numericUpDown6.Value);
            }

            Kalman1D k = new Kalman1D();
            k.Reset(
                (double)numericUpDown1.Value,
                (double)numericUpDown2.Value,
                (double)numericUpDown3.Value,
                (double)numericUpDown4.Value, 0);

            // Assume we get to see every other measurement we calculated, and use
            // the others as the points to compare for estimates.
            // Run the filter, note our time unit is 1.
            double[] kalman = new double[idealNoNoise.Length];
            double[] vel = new double[idealNoNoise.Length];
            double[] kGain = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i += 2)
            {
                if (i == 0)
                {
                    kalman[0] = 0;
                    vel[0] = k.Velocity;
                    kGain[0] = k.LastGain;
                    kalman[1] = k.Predicition(1);
                    vel[1] = k.Velocity;
                    kGain[1] = k.LastGain;
                }
                else
                {
                    kalman[i] = k.Update(withAllNoise[i], 1);
                    kalman[i + 1] = kalman[i];
                    vel[i] = k.Velocity;
                    kGain[i] = k.LastGain;
                    vel[i + 1] = vel[i];
                    kGain[i + 1] = kGain[i];
               //     kalman[i + 1] = k.Predicition(1);
                }

            }
            
            // Compute errors.
            double[] kalmanDelIdeal = new double[idealNoNoise.Length];
            double[] measuredDelIdeal = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i += 2)
            {
                kalmanDelIdeal[i] = kalman[i] - idealNoNoise[i];
                measuredDelIdeal[i] = withAllNoise[i] - idealNoNoise[i];
            }

            // Chart stuff.
            chart1.Series.Clear();
            graph(chart1, "ideal", idealNoNoise, Color.Black);
            graph(chart1, "measured", withAllNoise, Color.Blue);
            graph(chart1, "kalman", kalman, Color.Red);
            chart1.ChartAreas[0].RecalculateAxesScale();

            chart2.Series.Clear();
            graph(chart2, "Measurement", measuredDelIdeal, Color.Blue);
            graph(chart2, "Kalman", kalmanDelIdeal, Color.Black);
            chart2.ChartAreas[0].RecalculateAxesScale();

            chart3.Series.Clear();
            graph(chart3, "Velocity", vel, Color.Black);
            chart3.ChartAreas[0].RecalculateAxesScale();

            chart4.Series.Clear();
            graph(chart4, "Gain", kGain, Color.Black);
            chart4.ChartAreas[0].RecalculateAxesScale();
        }

        /// <summary>
        /// Append a graph.
        /// </summary>
        /// <param name="c"></param>
        /// <param name="name"></param>
        /// <param name="vals"></param>
        /// <param name="clr"></param>
        protected void graph(Chart c, string name, double[] vals, Color clr)
        {
            Series s = new Series(name);
            s.ChartType = SeriesChartType.Line;

            for (int i = 0; i < vals.Length; i++)
            {
                s.Points.Add(new DataPoint(i, vals[i]));
            }
            s.Color = clr;
            c.Series.Add(s);
        }

        /// <summary>
        /// Line 2D
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            m_lastButton = button4;
            double[] idealNoNoise = new double[100];
            double[] idealNoNoiseVelocity = new double[idealNoNoise.Length];
            double half = idealNoNoise.Length / 2;

            idealNoNoise[0] = 10;
            for (int i = 1; i < idealNoNoise.Length; i++)
            {
                idealNoNoiseVelocity[i] = 0.2;
                idealNoNoise[i] = idealNoNoise[i - 1] + idealNoNoiseVelocity[i];
            }
            formTest2D(idealNoNoise, idealNoNoiseVelocity);
        }

        /// <summary>
        /// Line 2D B
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button6_Click(object sender, EventArgs e)
        {
            m_lastButton = button6;
            double[] idealNoNoise = new double[200];
            double[] idealNoNoiseVelocity = new double[idealNoNoise.Length];
            double half = idealNoNoise.Length / 2;

            idealNoNoise[0] = 10;
            for (int i = 1; i < idealNoNoise.Length; i++)
            {
                idealNoNoiseVelocity[i] = 0.2;
                
                
                idealNoNoise[i] = idealNoNoise[i - 1] + idealNoNoiseVelocity[i];
                if (i == idealNoNoise.Length / 2)
                {
                    idealNoNoise[i] = -10;
                }
            }
            formTest2D(idealNoNoise, idealNoNoiseVelocity);
        }

        /// <summary>
        /// Line 2dC
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {

            m_lastButton = button7;
            double[] idealNoNoise = new double[200];
            double[] idealNoNoiseVelocity = new double[idealNoNoise.Length];
            double half = idealNoNoise.Length / 2;

            idealNoNoise[0] = 10;
            for (int i = 1; i < idealNoNoise.Length; i++)
            {
                idealNoNoiseVelocity[i] = (i < idealNoNoise.Length / 2 ) ? 0.4 : - 0.4;

                idealNoNoise[i] = idealNoNoise[i - 1] + idealNoNoiseVelocity[i];
            }
            formTest2D(idealNoNoise, idealNoNoiseVelocity);
        }


        /// <summary>
        /// Form a test.
        /// </summary>
        /// <param name="idealNoNoise"></param>
        protected void formTest2D(double[] idealNoNoise, double[] idealNoNoiseVelocity)
        {
            Random.Random r = new Random.Random();
            // Add in wind, auto-pilot on our rocket always successfully adapts so
            // that the effect by the wind is guassian process noise.
            double[] withProcessNoise = new double[idealNoNoise.Length];
            double[] withProcessNoiseVel = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Add in 1 foot = 1 std. deviation of process noise.
                withProcessNoise[i] = idealNoNoise[i] + r.NextGuass_Double(0, (double)numericUpDown5.Value);
                withProcessNoiseVel[i] = idealNoNoiseVelocity[i] + r.NextGuass_Double(0, (double)numericUpDown6.Value);
            }

            // Add in measurement noise, the GPS on the rocket measures less often
            // than our data but we can just skip things.
            // Assume a very good GPS, accurate to +/- 6 feet even at these speeds.
            // 2 foot = 1 std dev. is about +/- 6 feet
            double[] withAllNoise = new double[idealNoNoise.Length];
            double[] withAllNoiseVel = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i++)
            {
                // Add in 1 foot = 1 std. deviation of process noise.
                withAllNoise[i] = withProcessNoise[i] + r.NextGuass_Double(0, (double)numericUpDown6.Value);
                withAllNoiseVel[i] = withProcessNoiseVel[i] + r.NextGuass_Double(0, 0.2*(double)numericUpDown6.Value);
            }

            Kalman2D k = new Kalman2D();
            k.Reset(
                (double)numericUpDown1.Value,
                (double)numericUpDown2.Value,
                (double)numericUpDown3.Value,
                (double)numericUpDown4.Value, 0);

            // Assume we get to see every other measurement we calculated, and use
            // the others as the points to compare for estimates.
            // Run the filter, note our time unit is 1.
            double[] kalman = new double[idealNoNoise.Length];
            double[] vel = new double[idealNoNoise.Length];
            double[] kGain = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i ++)
            {
                if (i == 0)
                {
                    kalman[0] = 0;
                    vel[0] = k.Velocity;
                    kGain[0] = k.LastGain;
                }
                else
                {
                    kalman[i] = k.Update(withAllNoise[i], withAllNoiseVel[i], 1);
                    vel[i] = k.Velocity;
                    kGain[i] = k.LastGain;
                }

            }

            for (int i = 0; i < kalman.Length; i++)
            {
                kalman[i] = (kalman[i] < 100000000000000) ? kalman[i] : 100000000000000;
                kalman[i] = (kalman[i] > -100000000000000) ? kalman[i] : -100000000000000;
                vel[i] = (vel[i] < 100000000000000) ? vel[i] : 100000000000000;
                vel[i] = (vel[i] > -100000000000000) ? vel[i] : -100000000000000;
            }

            // Compute errors.
            double[] kalmanDelIdeal = new double[idealNoNoise.Length];
            double[] measuredDelIdeal = new double[idealNoNoise.Length];
            for (int i = 0; i < idealNoNoise.Length; i += 2)
            {
                kalmanDelIdeal[i] = kalman[i] - idealNoNoise[i];
                measuredDelIdeal[i] = withAllNoise[i] - idealNoNoise[i];
            }
            
            // Chart stuff.
            chart1.Series.Clear();
            graph(chart1, "ideal", idealNoNoise, Color.Black);
            graph(chart1, "measured", withAllNoise, Color.Blue);
            graph(chart1, "kalman", kalman, Color.Red);
            chart1.ChartAreas[0].RecalculateAxesScale();

            chart2.Series.Clear();
            graph(chart2, "Measurement", measuredDelIdeal, Color.Blue);
            graph(chart2, "Kalman", kalmanDelIdeal, Color.Black);
            chart2.ChartAreas[0].RecalculateAxesScale();

            chart3.Series.Clear();
            graph(chart3, "Velocity", vel, Color.Black);
            chart3.ChartAreas[0].RecalculateAxesScale();

            chart4.Series.Clear();
            graph(chart4, "Gain", kGain, Color.Black);
            chart4.ChartAreas[0].RecalculateAxesScale();
        }

        /// <summary>
        /// Basic matrix unit test.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            Matrix c = new Matrix(1, 2) { Data = new double[] { 1, 2 } };
            Matrix r = new Matrix(2, 1) { Data = new double[] { 2, 3 } };

            r.Multiply(c);
            if (r.Data[0] != (2 + 6)) return;

            r = new Matrix(2, 1) { Data = new double[] { 2, 3 } };

            c.Multiply(r);
            if (c.Data[0] != 2) return;
            if (c.Data[1] != 3) return;
            if (c.Data[2] != 4) return;
            if (c.Data[3] != 6) return;

            c.Transpose();
            if (c.Data[0] != 2) return;
            if (c.Data[1] != 4) return;
            if (c.Data[2] != 3) return;
            if (c.Data[3] != 6) return;

            c.Data[3] = 2;
            Matrix ci = Matrix.Invert(c);
            if (ci.Data[0] != -0.250) return;
            if (ci.Data[1] != 0.375) return;
            if (ci.Data[2] != 0.5) return;
            if (ci.Data[3] != -0.250) return;
 
        }


        



    }
}
