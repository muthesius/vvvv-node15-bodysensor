///////////////////////////////////////////////////////////////////////////////
//
//  Kalman1D.cs
//
//  By Philip R. Braica (HoshiKata@aol.com, VeryMadSci@gmail.com)
//
//  Distributed under the The Code Project Open License (CPOL)
//  http://www.codeproject.com/info/cpol10.aspx
///////////////////////////////////////////////////////////////////////////////

// Using.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// Namespace.
namespace KalmanDemo
{
    /// <summary>
    /// Kalman 1D w/ velocity estimation.
    /// </summary>
    public class Kalman1D
    {
        #region Protected data.
        /// <summary>
        /// State.
        /// </summary>
        double [] m_x = new double[2];
        /// <summary>
        /// Covariance.
        /// </summary>
        double [] m_p = new double[4];

        /// <summary>
        /// Minimal covariance.
        /// </summary>
        double [] m_q = new double[4];

        /// <summary>
        /// Minimal innovative covariance, keeps filter from locking in to a solution.
        /// </summary>
        double m_r;
        #endregion

        /// <summary>
        /// The last updated value, can also be set if filter gets
        /// sudden absolute measurement data for the latest update.
        /// </summary>
        public double Value 
        {
            get { return m_x[0]; }
            set { m_x[0] = value; }
        }

        /// <summary>
        /// How fast the value is changing.
        /// </summary>
        public double Velocity
        {
            get { return m_x[1]; }
        }

        /// <summary>
        /// The last kalman gain used, useful for debug.
        /// </summary>
        public double LastGain { get; protected set; }

        /// <summary>
        /// Last updated positional variance.
        /// </summary>
        /// <returns></returns>
        public double Variance () { return m_p[0]; }   

        /// <summary>
        /// Predict the value forward from last measurement time by dt.
        /// X = F*X + H*U        
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Predicition(double dt)
        {
            return m_x[0] + (dt * m_x[1]);
        }

        /// <summary>
        /// Get the estimated covariance of position predicted 
        /// forward from last measurement time by dt.
        /// P = F*X*F^T + Q.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Variance(double dt)
        {
            return m_p[0] + dt * (m_p[2] + m_p[1]) + dt * dt * m_p[3] + m_q[0];
            // Not needed.
            // m_p[1] = m_p[1] + dt * m_p[3] + m_q[1];
            // m_p[2] = m_p[2] + dt * m_p[3] + m_q[2];
            // m_p[3] = m_p[3] + m_q[3];
        }

        /// <summary>
        /// Reset the filter.
        /// </summary>
        /// <param name="qx">Measurement to position state minimal variance.</param>
        /// <param name="qv">Measurement to velocity state minimal variance.</param>
        /// <param name="r">Measurement covariance (sets minimal gain).</param>
        /// <param name="pd">Initial variance.</param>
        /// <param name="ix">Initial position.</param>
	    public void Reset(double qx, double qv, double r, double pd, double ix)    
	    {
	        m_q[0] = qx; m_q[1] = qv;
	        m_r = r;
 	        m_p[0] = m_p[3] = pd; 
	        m_p[1] = m_p[2] = 0;
	        m_x[0] = ix; 
	        m_x[1] = 0;
 	    }

        /// <summary>
        /// Update the state by measurement m at dt time from last measurement.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Update(double m, double dt)
        {
            // Predict to now, then update.
            // Predict:
            //   X = F*X + H*U
            //   P = F*X*F^T + Q.
            // Update:
            //   Y = M – H*X          Called the innovation = measurement – state transformed by H.	
            //   S = H*P*H^T + R      S= Residual covariance = covariane transformed by H + R
            //   K = P * H^T *S^-1    K = Kalman gain = variance / residual covariance.
            //   X = X + K*Y          Update with gain the new measurement
            //   P = (I – K * H) * P  Update covariance to this time.

            // X = F*X + H*U
            double oldX = m_x[0];
            m_x[0] = m_x[0] + (dt * m_x[1]);

            // P = F*X*F^T + Q
            m_p[0] = m_p[0] + dt * (m_p[2] + m_p[1]) + dt * dt * m_p[3] + m_q[0];
            m_p[1] = m_p[1] + dt * m_p[3] + m_q[1];
            m_p[2] = m_p[2] + dt * m_p[3] + m_q[2];
            m_p[3] = m_p[3] + m_q[3];

            // Y = M – H*X  
            double y0 = m - m_x[0];
            double y1 = ((m - oldX) / dt) - m_x[1];

            // S = H*P*H^T + R 
            // Because H = [1, 0] this is easy, and s is a single value not a matrix to invert.
            double s = m_p[0] + m_r;

            // K = P * H^T *S^-1 
            double k = m_p[0] / s;
            LastGain = k;

            // X = X + K*Y
            m_x[0] += y0 * k;
	        m_x[1] += y1 * k;

            // P = (I – K * H) * P
            for (int i = 0; i < 4; i++) m_p[i] = m_p[i] - k * m_p[i];

            // Return latest estimate.
            return m_x[0];
        }
    }
}
