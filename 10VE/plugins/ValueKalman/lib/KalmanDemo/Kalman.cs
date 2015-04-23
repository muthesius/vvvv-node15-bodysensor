///////////////////////////////////////////////////////////////////////////////
//
//  Kalman.cs
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
    /// Kalman 
    ///  
    /// Predict:
    ///   X = F*X + H*U
    ///   P = F*P*F^T + Q.
    /// 
    /// Update:
    ///   Y = M – H*X          Called the innovation = measurement – state transformed by H.	
    ///   S = H*P*H^T + R      S= Residual covariance = covariane transformed by H + R
    ///   K = P * H^T *S^-1    K = Kalman gain = variance / residual covariance.
    ///   X = X + K*Y          Update with gain the new measurement
    ///   P = (I – K * H) * P  Update covariance to this time.
    ///
    /// Note: 
    ///   Derived classes need to perhaps hide certain matrixes to simplify the 
    ///   interface. Also perhaps override: SetupF, Predict, Reset or Update.
    /// </summary>
    public class Kalman
    {
        #region Protected data.
        /// <summary>
        /// State.
        /// </summary>
        protected Matrix m_x = new Matrix();

        /// <summary>
        /// Covariance.
        /// </summary>
        protected Matrix m_p = new Matrix();

        /// <summary>
        /// Minimal covariance.
        /// </summary>
        protected Matrix m_q = new Matrix();

        /// <summary>
        /// Minimal innovative covariance, keeps filter from locking in to a solution.
        /// </summary>
        protected Matrix m_r = new Matrix();

        /// <summary>
        /// Converts m_x forward to a new time interval.
        /// Depending on what the states mean in m_x this could be anything.
        /// Often for 2D it is: 1, dt
        ///                     0,  1  
        /// Because that would convert a 2 dimentional motion, velocity vector
        /// forward by time dt.
        /// 
        /// For exampe, if acceleration was used instead of velocity 
        /// we could instead have dt*dt instead of dt and thus have a 2nd order
        /// Kalman that estimated position with smooth accelerations.
        /// </summary>
        protected Matrix m_f = new Matrix();

        /// <summary>
        /// Converts measurement vector into state vector space.
        /// </summary>
        protected Matrix m_h = new Matrix();

        /// <summary>
        /// Apriori per update information. For example if we knew something 
        /// moved at a specific velocity always we could just use U to add
        /// that in and decouple that from the other statistics addressed 
        /// by the Kalman filter.
        /// </summary>
        protected Matrix m_u = new Matrix();
        #endregion

        /// <summary>
        /// State.
        /// </summary>
        public Matrix X { get { return m_x; } set { m_x.Set(value); } }

        /// <summary>
        /// Covariance.
        /// </summary>
        public Matrix P { get { return m_p; } set { m_p.Set(value); } }

        /// <summary>
        /// Minimal covariance.
        /// </summary>
        public Matrix Q { get { return m_q; } set { m_q.Set(value); } }

        /// <summary>
        /// Minimal innovative covariance, keeps filter from locking in to a solution.
        /// </summary>
        public  Matrix R { get { return m_r; } set { m_r.Set(value); } }

        /// <summary>
        /// Converts m_x forward to a new time interval.
        /// Depending on what the states mean in m_x this could be anything.
        /// Often for 2D it is: 1, dt
        ///                     0,  1  
        /// Because that would convert a 2 dimentional motion, velocity vector
        /// forward by time dt.
        /// 
        /// For exampe, if acceleration was used instead of velocity 
        /// we could instead have dt*dt instead of dt and thus have a 2nd order
        /// Kalman that estimated position with smooth accelerations.
        /// </summary>
        public Matrix F { get { return m_f; } set { m_f.Set(value); } }

        /// <summary>
        /// Converts measurement vector into state vector space.
        /// </summary>
        public Matrix H { get { return m_h; } set { m_h.Set(value); } }

        /// <summary>
        /// Apriori per update information. For example if we knew something 
        /// moved at a specific velocity always we could just use U to add
        /// that in and decouple that from the other statistics addressed 
        /// by the Kalman filter.
        /// </summary>
        public Matrix U { get { return m_u; } set { m_u.Set(value); } }


        /// <summary>
        /// How fast the value is changing.
        /// </summary>
        public double Value(int index)
        {
            return m_x.Data[index];
        }

        /// <summary>
        /// The last kalman gain determinant used, useful for debug.
        /// </summary>
        public double LastGain { get; protected set; }

        /// <summary>
        /// Last updated value[0] variance.
        /// </summary>
        /// <returns></returns>
        public double Variance() { return m_p.Data[0]; }

        /// <summary>
        /// Get the covariance of item i to itself.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public double Variance(int index) { return m_p.Trace(index); }

        /// <summary>
        /// Setup matrix F based on dt, the time between last update and this update. 
        /// Default is for a rectangular time based:
        ///  1, dt, dt^2, dt^3, ...
        ///  0,  1, dt,   dt^2, ...
        ///  0,  0, 1,    dt, ...
        ///  0,  0, 0,     1, ...
        ///  ...
        /// </summary>
        /// <param name="dt"></param>
        public virtual void SetupF(double dt)
        {
            m_f.Zero();
            
            for (int i = 0; i < m_f.Rows; i++)
            {
                double m = 1;
                m_f.Set(i, i, m);
                for (int j = i + 1; j < m_f.Columns; j++)
                {
                    m *= dt;
                    m_f.Set(i, i, m);
                }
            }
        }

        /// <summary>
        /// Predict the most significant value forward from 
        /// last measurement time by dt.
        /// X = F*X + H*U        
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual double Predict(double dt)
        {
            SetupF(dt);
            Matrix tmp = Matrix.Multiply(m_f, m_x);
            Matrix tmp2 = Matrix.Multiply(m_h, m_u);
            tmp.Add(tmp2);
            return tmp.Data[0];
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
            SetupF(dt);
            Matrix tmp = Matrix.MultiplyABAT(m_f, m_p);
            tmp.Add(m_q);
            return tmp.Data[0];
        }

        /// <summary>
        /// Reset the filter.        
        /// </summary>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <param name="p"></param>
        /// <param name="x"></param>
        public virtual void Reset(Matrix q, Matrix r, Matrix p, Matrix x)
        {
            Q = q;
            R = r;
            P = p;
            X = x;
        }

        /// <summary>
        /// Update the state by measurement m at dt time from last measurement.
        /// </summary>
        /// <param name="measurement"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual double Update(Matrix measurement, double dt)
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
            SetupF(dt);
            Matrix t1 = Matrix.Multiply(m_f, m_x);
            Matrix t2 = Matrix.Multiply(m_h, m_u);
            t1.Add(t2);
            m_x.Set(t1);

            // P = F*P*F^T + Q
            m_p = Matrix.MultiplyABAT(m_f, m_p);
            m_p.Add(m_q);

            // Y = M – H*X  
            t1 = Matrix.Multiply(m_h, m_x);
            Matrix y = Matrix.Subtract(measurement, t1);

            // S = H*P*H^T + R 
            Matrix s = Matrix.MultiplyABAT(m_h, m_p);
            s.Add(m_r);

            // K = P * H^T *S^-1 
            Matrix ht = Matrix.Transpose(m_h);
            Matrix tmp = Matrix.Multiply(m_p, ht);
            Matrix sinv = Matrix.Invert(s);
            Matrix k = new Matrix(y.Rows, m_x.Rows);
            if (sinv != null)
            {
                k = Matrix.Multiply(tmp, sinv);
            }

            LastGain = k.Determinant;

            // X = X + K*Y
            m_x.Add(Matrix.Multiply(k, y));

            // P = (I – K * H) * P
            Matrix kh = Matrix.Multiply(k, m_h);
            Matrix id = new Matrix(kh.Columns, kh.Rows);
            id.SetIdentity();
            id.Subtract(kh);
            id.Multiply(m_p);
            m_p.Set(id);

            // Return latest estimate.
            return m_x.Data[0];
        }
    }
}
