using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NumericalCWS2D
{
    public struct Point4
    {
        public int x1;
        public int y1;
        public int x2;
        public int y2;
        public Point4(int x1v, int y1v, int x2v, int y2v)
        {
            x1 = x1v;
            y1 = y1v;
            x2 = x2v;
            y2 = y2v;
        }
    }
    static class Algor
    {
        internal static int pixs;
        internal static Form1 mw;
        internal static int startx1;
        internal static int starty1;
        internal static int startx2;
        internal static int starty2;
        internal static FloatComplex[,,,] FinalRes;
        internal static float[,,,] res;
        internal const int threads = 10;
        //A thread-safe random number generator
        private static RNGCryptoServiceProvider globalRandom = new RNGCryptoServiceProvider();
        [ThreadStatic]
        private static Random localRandom;
        private static ConcurrentQueue<Point4> Toexpand;
        private static byte[] threadcomplete;
        internal static int maxi;
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
        static bool AllowZone(int x1, int y1, int x2, int y2)
        {
            if (x1 < 0 || x1 >= pixs || y1 < 0 || y1 >= pixs || x2 < 0 || x2 >= pixs || y2 < 0 || y2 >= pixs) return false;
            if (Monte.Ifromxcor(x1, y1, x2, y2) == 0 || Monte.Ifromycor(x1, y1, x2, y2) == 0) return false;
            if (!(res[x1, y1, x2, y2] is float.NaN)) return false;
            return true;
        }
        internal static void ExpandWork(int threadno)
        {
            bool resend;
            double randfac;
            Point4 p;
            redo:
            if (Toexpand.TryDequeue(out p))
            {
                threadcomplete[threadno] = 0;
                resend = false;
                randfac = Monte.Ifromxcor(p.x1, p.y1, p.x2, p.y2) / (double)maxi / 3.0 + 0.5;
                if (AllowZone(p.x1 - 1, p.y1, p.x2, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1 - 1, p.y1, p.x2, p.y2] = res[p.x1, p.y1, p.x2, p.y2] - (float)Monte.k1x(p.x1 - 1, p.y1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1 - 1, p.y1, p.x2, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1 + 1, p.y1, p.x2, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1 + 1, p.y1, p.x2, p.y2] = res[p.x1, p.y1, p.x2, p.y2] + (float)Monte.k1x(p.x1, p.y1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1 + 1, p.y1, p.x2, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1 - 1, p.x2, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1 - 1, p.x2, p.y2] = res[p.x1, p.y1, p.x2, p.y2] - (float)Monte.k1y(p.x1, p.y1 - 1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1-1, p.x2, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1 + 1, p.x2, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1 + 1, p.x2, p.y2] = res[p.x1, p.y1, p.x2, p.y2] + (float)Monte.k1y(p.x1, p.y1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1 + 1, p.x2, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1, p.x2 - 1, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1, p.x2 - 1, p.y2] = res[p.x1, p.y1, p.x2, p.y2] - (float)Monte.k2x(p.x1, p.y1, p.x2 - 1, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1, p.x2-1, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1, p.x2 + 1, p.y2)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1, p.x2 + 1, p.y2] = res[p.x1, p.y1, p.x2, p.y2] + (float)Monte.k2x(p.x1, p.y1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1, p.x2 + 1, p.y2));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1, p.x2, p.y2 - 1)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1, p.x2, p.y2 - 1] = res[p.x1, p.y1, p.x2, p.y2] - (float)Monte.k2y(p.x1, p.y1, p.x2, p.y2 - 1);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1, p.x2, p.y2 - 1));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                        resend = true;
                    }
                if (AllowZone(p.x1, p.y1, p.x2, p.y2 + 1)) if (NextRandom() < randfac)
                    {
                        res[p.x1, p.y1, p.x2, p.y2 + 1] = res[p.x1, p.y1, p.x2, p.y2] + (float)Monte.k2y(p.x1, p.y1, p.x2, p.y2);
                        Toexpand.Enqueue(new Point4(p.x1, p.y1, p.x2, p.y2 + 1));
                    }
                    else if (!resend)
                    {
                        Toexpand.Enqueue(p);
                    }
            }
            else
            {
                threadcomplete[threadno] = 1;
                int s = 0;
                foreach (byte i in threadcomplete) s += i;
                if (s == threads) return;
            }
            goto redo;
        }
        internal static void DoWork()
        {
            res = new float[pixs, pixs, pixs, pixs];
            Parallel.For(0, pixs, x1 =>
            {
                for (int y1 = 0; y1 < pixs; y1++) for (int x2 = 0; x2 < pixs; x2++) for (int y2 = 0; y2 < pixs; y2++)
                        {
                            res[x1, y1, x2, y2] = float.NaN;
                        }
            });
            Toexpand = new ConcurrentQueue<Point4>();
            threadcomplete = new byte[threads];
            var tasklist = new Task[threads];
            Toexpand.Enqueue(new Point4(startx1, starty1, startx2, starty2));
            res[startx1, starty1, startx2, starty2] = 0.0f;
            for(int i = 0; i < threads; i++)
            {
                int ii = i;
                tasklist[ii] = Task.Run(() => ExpandWork(ii));
            }
            Task.WaitAll(tasklist);
            Parallel.For(0, pixs, x1 =>
            {
                for (int y1 = 0; y1 < pixs; y1++) for (int x2 = 0; x2 < pixs; x2++) for (int y2 = 0; y2 < pixs; y2++)
                        {
                            if(!(res[x1, y1, x2, y2] is float.NaN)) FinalRes[x1, y1, x2, y2] += new FloatComplex(res[x1, y1, x2, y2], 0.0f);
                        }
            });
            res = null;
        }
    }
}
