///////////////////////////////////////////////////////////////////////////////
//
//  Kalman2D.cs
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
    /// Kalman 2D.
    /// </summary>
    public class Kalman2D
    {
        #region Protected data.
        /// <summary>
        /// State.
        /// </summary>
        Matrix m_x = new Matrix(1, 2);
        /// <summary>
        /// Covariance.
        /// </summary>
        Matrix m_p = new Matrix(2, 2);
        /// <summary>
        /// Minimal covariance.
        /// </summary>
        Matrix m_q = new Matrix(2, 2);

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
            get { return m_x.Data[0]; }
            set { m_x.Data[0] = value; }
        }

        /// <summary>
        /// How fast the value is changing.
        /// </summary>
        public double Velocity
        {
            get { return m_x.Data[1]; }
        }

        /// <summary>
        /// The last kalman gain used, useful for debug.
        /// </summary>
        public double LastGain { get; protected set; }

        /// <summary>
        /// Last updated positional variance.
        /// </summary>
        /// <returns></returns>
        public double Variance() { return m_p.Data[0]; }   

        /// <summary>
        /// Predict the value forward from last measurement time by dt.
        /// X = F*X + H*U        
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Predicition(double dt)
        {
            return m_x.Data[0] + (dt * m_x.Data[1]);
        }

        /// <summary>
        /// Get the estimated covariance of position predicted 
        /// forward from last measurement time by dt.
        /// P = F*P*F^T + Q.
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Variance(double dt)
        {
            return m_p.Data[0] + dt * (m_p.Data[2] + m_p.Data[1]) + dt * dt * m_p.Data[3] + m_q.Data[0];
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
            m_q.Data[0] = qx * qx;
            m_q.Data[1] = qv * qx;
            m_q.Data[2] = qv * qx;
            m_q.Data[3] = qv * qv;

	        m_r = r;
            m_p.Data[0] = m_p.Data[3] = pd;
            m_p.Data[1] = m_p.Data[2] = 0;
            m_x.Data[0] = ix;
            m_x.Data[1] = 0;
 	    }

        /// <summary>
        /// Update the state by measurement m at dt time from last measurement.
        /// </summary>
        /// <param name="m"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public double Update(double mx, double mv, double dt)
        {
            // Predict to now, then update.
            // Predict:
            //   X = F*X + H*U
            //   P = F*P*F^T + Q.
            // Update:
            //   Y = M – H*X          Called the innovation = measurement – state transformed by H.	
            //   S = H*P*H^T + R      S= Residual covariance = covariane transformed by H + R
            //   K = P * H^T *S^-1    K = Kalman gain = variance / residual covariance.
            //   X = X + K*Y          Update with gain the new measurement
            //   P = (I – K * H) * P  Update covariance to this time.
            //
            // Same as 1D but mv is used instead of delta m_x[0], and H = [1,1].

            // X = F*X + H*U
            Matrix f = new Matrix(2, 2) { Data = new double [] { 1, dt, 0, 1 } };
            Matrix h = new Matrix(2, 2) { Data = new double[] { 1, 0, 0, 1 } };
            Matrix ht = new Matrix(2, 2) { Data = new double[] { 1, 0, 0, 1 } };
            // U = {0,0};
            m_x = Matrix.Multiply(f, m_x);

            // P = F*P*F^T + Q
            m_p = Matrix.MultiplyABAT(f, m_p);
            m_p.Add(m_q);

            // Y = M – H*X  
            Matrix y = new Matrix(1, 2) { Data = new double[] { mx - m_x.Data[0], mv - m_x.Data[1] } };

            // S = H*P*H^T + R 
            Matrix s = Matrix.MultiplyABAT(h, m_p);
            s.Data[0] += m_r;
            s.Data[3] += m_r*0.1;
           
            // K = P * H^T *S^-1 
            Matrix tmp = Matrix.Multiply(m_p, ht);
            Matrix sinv = Matrix.Invert(s);
            Matrix k = new Matrix(2, 2); // inited to zero.

            if (sinv != null)
            {
                k = Matrix.Multiply(tmp, sinv);              
            }

            LastGain = k.Determinant;

            // X = X + K*Y
            m_x.Add(Matrix.Multiply(k, y));

            // P = (I – K * H) * P
            Matrix kh = Matrix.Multiply(k, h);
            Matrix id = new Matrix(2, 2) { Data = new double[] { 1, 0, 0, 1 } };
            kh.Multiply(-1);
            id.Add(kh);
            id.Multiply(m_p);
            m_p.Set(id);


            // Return latest estimate.
            return m_x.Data[0];
        }


    }
}
