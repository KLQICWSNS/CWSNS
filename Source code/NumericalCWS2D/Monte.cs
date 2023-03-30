using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace NumericalCWS2D
{
    static class Monte
    {
        internal static double l;
        internal static double range; // width, -range/2 ~ range/2
        internal static double hran;
        internal static double campixlen;
        internal static int dispcor;
        internal static int campixs;
        internal static double dover2l;// campixlen / 2l
        internal static Func<double, double, double, double, Complex> wf;
        internal static Func<double, double, double, double, Complex> wfnof;
        internal static Form1 mw;
        //A thread-safe random number generator
        private static RNGCryptoServiceProvider globalRandom = new RNGCryptoServiceProvider();
        internal const string MsgTitle = "Coincidence Wavefront Sensor Numerical Simulation";
        [ThreadStatic]
        private static Random localRandom;
        internal static double NextRandom()
        {
            Random inst = localRandom;
            if (inst == null)
            {
                byte[] buffer = new byte[4];
                globalRandom.GetBytes(buffer);
                localRandom = inst = new Random(BitConverter.ToInt32(buffer, 0));
            }
            return inst.NextDouble();
        }
        internal static ushort[,,,] I_xLL;
        internal static ushort[,,,] I_xLR;
        internal static ushort[,,,] I_xRL;
        internal static ushort[,,,] I_xRR;
        internal static ushort[,,,] I_yLL;
        internal static ushort[,,,] I_yLR;
        internal static ushort[,,,] I_yRL;
        internal static ushort[,,,] I_yRR;
        internal static int I_x1L(int x1, int y1, int x2, int y2) => I_xLL[x1, y1, x2, y2] + I_xLR[x1, y1, x2, y2];//displaced
        internal static int I_x1R(int x1, int y1, int x2, int y2) => I_xRL[x1, y1, x2, y2] + I_xRR[x1, y1, x2, y2];//
        internal static int I_x2L(int x1, int y1, int x2, int y2) => I_xLL[x1, y1, x2, y2] + I_xRL[x1, y1, x2, y2];//
        internal static int I_x2R(int x1, int y1, int x2, int y2) => I_xLR[x1, y1, x2, y2] + I_xRR[x1, y1, x2, y2];//
        internal static int I_y1L(int x1, int y1, int x2, int y2) => I_yLL[x1, y1, x2, y2] + I_yLR[x1, y1, x2, y2];//
        internal static int I_y1R(int x1, int y1, int x2, int y2) => I_yRL[x1, y1, x2, y2] + I_yRR[x1, y1, x2, y2];//
        internal static int I_y2L(int x1, int y1, int x2, int y2) => I_yLL[x1, y1, x2, y2] + I_yRL[x1, y1, x2, y2];//
        internal static int I_y2R(int x1, int y1, int x2, int y2) => I_yLR[x1, y1, x2, y2] + I_yRR[x1, y1, x2, y2];//
        internal static int Ifromx(int x1, int y1, int x2, int y2) => I_xLL[x1, y1, x2, y2] + I_xLR[x1, y1, x2, y2] + I_xRL[x1, y1, x2, y2] + I_xRR[x1, y1, x2, y2];//
        internal static int Ifromy(int x1, int y1, int x2, int y2) => I_yLL[x1, y1, x2, y2] + I_yLR[x1, y1, x2, y2] + I_yRL[x1, y1, x2, y2] + I_yRR[x1, y1, x2, y2];//
        internal static int Ifromxcor(int x1, int y1, int x2, int y2)
        {
            y1 += dispcor;
            y2 += dispcor;
            if (y1 >= campixs || y2 >= campixs) return 0;
            return Ifromx(x1, y1, x2, y2);
        }
        internal static int Ifromycor(int x1, int y1, int x2, int y2)
        {
            x1 -= dispcor;
            x2 -= dispcor;
            if (x1 < 0 || x2 < 0) return 0;
            return Ifromy(x1, y1, x2, y2);
        }
        internal static double k1x(int x1, int y1, int x2, int y2)
        {
            y1 += dispcor;
            y2 += dispcor;
            if (y1 >= campixs || y2 >= campixs) return double.NaN;
            int i = Ifromx(x1, y1, x2, y2);
            if (i == 0) return double.NaN;
            return Math.Asin((I_x1R(x1, y1, x2, y2) - I_x1L(x1, y1, x2, y2)) / (double)i) * dover2l;
        }
        internal static double k2x(int x1, int y1, int x2, int y2)
        {
            y1 += dispcor;
            y2 += dispcor;
            if (y1 >= campixs || y2 >= campixs) return double.NaN;
            int i = Ifromx(x1, y1, x2, y2);
            if (i == 0) return double.NaN;
            return Math.Asin((I_x2R(x1, y1, x2, y2) - I_x2L(x1, y1, x2, y2)) / (double)i) * dover2l;
        }
        internal static double k1y(int x1, int y1, int x2, int y2)
        {
            x1 -= dispcor;
            x2 -= dispcor;
            if (x1 < 0 || x2 < 0) return double.NaN;
            int i = Ifromy(x1, y1, x2, y2);
            if (i == 0) return double.NaN;
            return Math.Asin((I_y1L(x1, y1, x2, y2) - I_y1R(x1, y1, x2, y2)) / (double)i) * dover2l;
        }
        internal static double k2y(int x1, int y1, int x2, int y2)
        {
            x1 -= dispcor;
            x2 -= dispcor;
            if (x1 < 0 || x2 < 0) return double.NaN;
            int i = Ifromy(x1, y1, x2, y2);
            if (i == 0) return double.NaN;
            return Math.Asin((I_y2L(x1, y1, x2, y2) - I_y2R(x1, y1, x2, y2)) / (double)i) * dover2l;
        }
        internal static bool NewAlloc()
        {
            try
            {
                I_xLL = new ushort[campixs, campixs, campixs, campixs];
                I_xLR = new ushort[campixs, campixs, campixs, campixs];
                I_xRL = new ushort[campixs, campixs, campixs, campixs];
                I_xRR = new ushort[campixs, campixs, campixs, campixs];
                I_yLL = new ushort[campixs, campixs, campixs, campixs];
                I_yLR = new ushort[campixs, campixs, campixs, campixs];
                I_yRL = new ushort[campixs, campixs, campixs, campixs];
                I_yRR = new ushort[campixs, campixs, campixs, campixs];
            }
            catch
            {
                return false;
            }
            GC.Collect();
            return true;
        }
        internal static void Dealloc()
        {
            I_xLL = null;
            I_xLR = null;
            I_xRL = null;
            I_xRR = null;
            I_yLL = null;
            I_yLR = null;
            I_yRL = null;
            I_yRR = null;
            GC.Collect();
        }
        internal static void SaveI(string prefix)
        {
            //to be added
        }
        internal static void ReadI(string prefix)
        {
            //to be added
        }
        internal static int progr;
        internal static bool goodnof;
        internal static double range2;
        internal static bool saturated = false;
        internal static void GenOnex()
        {
            double x1o, y1o, x2o, y2o, x1, y1, x2, y2, ll, lr, rl, rr, redo;
            Complex f1, f2, f3, f4;
        regen:
            x1o = NextRandom() * range;
            y1o = NextRandom() * range;
            x1 = x1o - hran;
            y1 = y1o - hran;
            if (goodnof)
            {
                do
                {
                    x2o = (NextRandom() - 0.5) * range2 + x1o;
                    y2o = (NextRandom() - 0.5) * range2 + y1o;
                } while (x2o < 0.0 || x2o >= range || y2o < 0.0 || y2o >= range);
            }
            else
            {
                x2o = NextRandom() * range;
                y2o = NextRandom() * range;
            }
            x2 = x2o - hran;
            y2 = y2o - hran;
            f1 = wf(x1 - l, y1 - l, x2 - l, y2 - l);
            f2 = wf(x1 - l, y1 - l, x2 + l, y2 - l);
            f3 = wf(x1 + l, y1 - l, x2 - l, y2 - l);
            f4 = wf(x1 + l, y1 - l, x2 + l, y2 - l);
            ll = (f1 - f4 + Complex.ImaginaryOne * (f2 + f3)).mo2();
            lr = (f2 - f3 + Complex.ImaginaryOne * (f1 + f4)).mo2();
            rl = (f3 - f2 + Complex.ImaginaryOne * (f1 + f4)).mo2();
            rr = (f4 - f1 + Complex.ImaginaryOne * (f2 + f3)).mo2();
            redo = NextRandom() * 16.0;
            double isum = ll + lr + rl + rr;
            if (isum > 16.000000001) throw new Exception("The amplitude of the wavefunction is too large to perform Monte Carlo method correctly.");
            if (redo >= isum) goto regen;
            int ax1 = (int)(x1o / campixlen), ay1 = (int)(y1o / campixlen), ax2 = (int)(x2o / campixlen), ay2 = (int)(y2o / campixlen);
            if (redo < ll)
            {
                lock (I_xLL) if (I_xLL[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_xLL[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else if (redo < ll + lr)
            {
                lock (I_xLR) if (I_xLR[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_xLR[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else if (redo < ll + lr + rl)
            {
                lock (I_xRL) if (I_xRL[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_xRL[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else
            {
                lock (I_xRR) if (I_xRR[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_xRR[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            progr++;
        }
        internal static void GenOney()
        {
            double x1o, y1o, x2o, y2o, x1, y1, x2, y2, ll, lr, rl, rr, redo;
            Complex f1, f2, f3, f4;
        regen:
            x1o = NextRandom() * range;
            y1o = NextRandom() * range;
            x1 = x1o - hran;
            y1 = y1o - hran;
            if (goodnof)
            {
                do
                {
                    x2o = (NextRandom() - 0.5) * range2 + x1o;
                    y2o = (NextRandom() - 0.5) * range2 + y1o;
                } while (x2o < 0.0 || x2o >= range || y2o < 0.0 || y2o >= range);
            }
            else
            {
                x2o = NextRandom() * range;
                y2o = NextRandom() * range;
            }
            x2 = x2o - hran;
            y2 = y2o - hran;
            f1 = wf(x1 + l, y1 + l, x2 + l, y2 + l);
            f2 = wf(x1 + l, y1 + l, x2 + l, y2 - l);
            f3 = wf(x1 + l, y1 - l, x2 + l, y2 + l);
            f4 = wf(x1 + l, y1 - l, x2 + l, y2 - l);
            ll = (f1 - f4 + Complex.ImaginaryOne * (f2 + f3)).mo2();
            lr = (f2 - f3 + Complex.ImaginaryOne * (f1 + f4)).mo2();
            rl = (f3 - f2 + Complex.ImaginaryOne * (f1 + f4)).mo2();
            rr = (f4 - f1 + Complex.ImaginaryOne * (f2 + f3)).mo2();
            redo = NextRandom() * 16.0;
            double isum = ll + lr + rl + rr;
            if (isum > 16.0000000001) throw new Exception("The amplitude of the wavefunction is too large to perform Monte Carlo method correctly.");
            if (redo >= isum) goto regen;
            int ax1 = (int)(x1o / campixlen), ay1 = (int)(y1o / campixlen), ax2 = (int)(x2o / campixlen), ay2 = (int)(y2o / campixlen);
            if (redo < ll)
            {
                lock (I_yLL) if (I_yLL[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_yLL[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else if (redo < ll + lr)
            {
                lock (I_yLR) if (I_yLR[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_yLR[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else if (redo < ll + lr + rl)
            {
                lock (I_yRL) if (I_yRL[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_yRL[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            else
            {
                lock (I_yRR) if (I_yRR[ax1, ay1, ax2, ay2] != ushort.MaxValue) I_yRR[ax1, ay1, ax2, ay2]++; else saturated = true;
            }
            progr++;
        }
    }
}
