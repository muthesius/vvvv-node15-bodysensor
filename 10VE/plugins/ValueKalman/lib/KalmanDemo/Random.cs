///////////////////////////////////////////////////////////////////////////////
//
//  Random.cs
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

// Namespace
namespace Random
{
    /// <summary>
    /// Create a random sequence based on a Seed.
    /// </summary>
    public class Random
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Random()
        {
            Seed = 0;
            RandomNextSeed = false;
        }

        /// <summary>
        /// Construct and set seed.
        /// </summary>
        /// <param name="seed"></param>
        public Random(long seed)
        {
            Seed = seed;
            RandomNextSeed = false;
        }

        /// <summary>
        /// Construct and set seed.
        /// </summary>
        /// <param name="seed"></param>
        public Random(long seed, bool randomNextSeed)
        {
            Seed = seed;
            RandomNextSeed = randomNextSeed;
        }

        /// <summary>
        /// Controls what will the next value be.
        /// </summary>
        public long Seed { get; set; }

        /// <summary>
        /// After each number randomize the seed, makes each starting
        /// seed point the start of a unique sequence if true, else
        /// the values are just a very long sequence when random 
        /// bytes are used. Setting to false is faster.
        /// </summary>
        public bool RandomNextSeed { get; set; }

        #region Next Char, Byte, Short, UShort, Int, Long, Float, Double.
        /// <summary>
        /// Next character.
        /// </summary>
        public char NextChar
        {
            get
            {
                return (char)computeRandom();
            }
        }

        /// <summary>
        /// Next byte.
        /// </summary>
        public byte NextByte
        {
            get
            {
                return (byte)computeRandom();
            }
        }


        /// <summary>
        /// Next short.
        /// </summary>
        public short NextShort
        {
            get
            {
                int i = computeRandom();
                return (short)((i & 0x1) == 0 ? -i : i);
            }
        }
        
        /// <summary>
        /// Next short.
        /// </summary>
        public ushort NextUShort
        {
            get
            {
                return computeRandom();
            }
        }


        /// <summary>
        /// Next int.
        /// </summary>
        public int NextInt
        {
            get
            {
                int rv = 0;
                for (int i = 0; i < 4; i++)
                {
                    rv = (rv << 16) + (int)NextUShort;
                }
                return rv;
            }
        }


        /// <summary>
        /// Next long.
        /// </summary>
        public long NextLong
        {
            get
            {
                long rv = 0;
                for (int i = 0; i < 4; i++)
                {
                    rv = (rv << 16) + (long)NextUShort;
                }
                return rv;
            }
        }

        /// <summary>
        /// Next float.
        /// </summary>
        public float NextFloat
        {
            get
            {
                return (float)NextDouble;
            }
        }

        /// <summary>
        /// 
        /// Next double, between 0 and 1.
        /// </summary>
        public double NextDouble
        {
            get
            {
                double d = (double)NextLong;
                d = d < 0 ? -d : d;
                d = d / long.MaxValue;
                return  d < 0 ? (d > 1 ? 1 : 0) : d;
            }
        }
        #endregion

        #region Next flat (evenly distributed) Byte, Int, Float, Double.

        /// <summary>
        /// Next flat evenly distributed value between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public byte NextFlat_Byte(byte min, byte max)
        {
            return (byte)NextFlat_Int(min, max);
        }

        /// <summary>
        /// Next flat evenly distributed value between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public int NextFlat_Int(int min, int max)
        {
            return (int)NextFlat_Float(min, max);
        }

        /// <summary>
        /// Next flat evenly distributed value between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public float NextFlat_Float(float min, float max)
        {
            float s = (max - min);
            float m = max < min ? max : min;
            return (float)(m + s * NextFloat);
        }

        /// <summary>
        /// Next flat evenly distributed value between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public double NextFlat_Double(double min, double max)
        {
            double s = (max - min);
            double m = max < min ? max : min;
            return (double)(m + s * NextDouble);
        }
        #endregion

        #region Next gaussian like number, Byte, Int, Float, Double.
        /// <summary>
        /// Next guassian like value +/- 3 sigma about average.
        /// </summary>
        /// <param name="ave"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public byte NextGuass_Byte(byte ave, byte sigma)
        {
            int sum = 0; 
            int sig3 = sigma + sigma + sigma;
            int low = ave - sig3;
            int hi = ave + sig3;
            for (int i = 0; i < 4; i++) sum += NextFlat_Int(low, hi);
            return (byte)(sum / 4);
        }

        /// <summary>
        /// Next guassian like value +/- 3 sigma about average.
        /// </summary>
        /// <param name="ave"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public int NextGuass_Int(int ave, int sigma)
        {
            int sum = 0;
            int sig3 = sigma + sigma + sigma;
            int low = ave - sig3;
            int hi = ave + sig3;
            for (int i = 0; i < 4; i++) sum += NextFlat_Int(low, hi);
            return sum / 4;
        }

        /// <summary>
        /// Next guassian like value +/- 3 sigma about average.
        /// </summary>
        /// <param name="ave"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public float NextGuass_Float(float ave, float sigma)
        {
            float sum = 0;
            float sig3 = sigma + sigma + sigma;
            float low = ave - sig3;
            float hi = ave + sig3;
            for (int i = 0; i < 4; i++) sum += NextFlat_Float(low, hi);
            return sum / 4;
        }

        /// <summary>
        /// Next guassian like value +/- 3 sigma about average.
        /// </summary>
        /// <param name="ave"></param>
        /// <param name="sigma"></param>
        /// <returns></returns>
        public double NextGuass_Double(double ave, double sigma)
        {
            double sum = 0;
            double sig3 = sigma + sigma + sigma;
            double low = ave - sig3;
            double hi = ave + sig3;
            for (int i = 0; i < 4; i++) sum += NextFlat_Double(low, hi);
            return sum / 4;
        }
        #endregion

        /// <summary>
        /// Compute a random ushort.
        /// </summary>
        /// <returns></returns>
        protected virtual ushort computeRandom()
        {
            ushort rv = compute_NoSeedChange();
            Seed++;
            if (RandomNextSeed)
            {
                Seed += compute_NoSeedChange();
            }
            return rv;
        }

        /// <summary>
        /// Just compute the random ushort, don't change seed.
        /// </summary>
        /// <returns></returns>
        protected virtual ushort compute_NoSeedChange()
        {
            // This is a simple hash, and it's fast.
            
            long[] v = {
                0x73ae2743a3eab13c, 
                0x53a75d3f2123bda1,
                0x42a3bcba71a72843};

            long rv = Seed;
            for (int i = 0; i < v.Length; i++)
            {
                rv += v[i];
                rv -= rv << 3;
                rv ^= (v[i] >> 7);
                rv ^= rv >> 11;
            }  
            return (ushort)(rv & 0xFFFF); 
        }


    }
}
