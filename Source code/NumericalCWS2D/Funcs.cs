using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Drawing;

namespace NumericalCWS2D
{
    internal static class Funcs
    {
        //All the length units are mm
        static internal double a;//pump gaussian decay, small values have large spots
        static internal double b;//correlation strength, infinity when perfect
        static internal double bb;//correlation strength, infinity when perfect
        static internal double wl;//wavelength
        static internal double f;//path 1 Fourier lens
        static internal double kfact;
        internal static Func<double, double, double, double, Complex>[] FuncList = { DynComp.dyn, GoodCor, BadCor, NoCor, GoodCorNoFourier, BadCorNoFourier, NoCorNoFourier };
        //CertainFunc, GoodCor, BadCor, NoCor, GoodCorNoFourier, BadCorNoFourier, NoCorNoFourier
        //     0          1       2       3           4                5                6
        public static double mo2(this Complex z)
        {
            return z.Real * z.Real + z.Imaginary * z.Imaginary;
        }
        static Bitmap bm;
        internal static double bmwidth = 2.0;
        static int bmpixs = 0;
        static double[,] bmphas;
        internal static void LoadBitmap(string src, double width)
        {
            if (width <= 0) throw new Exception("Invalid input.");
            if(!System.IO.File.Exists(src)) throw new Exception("The image file does not exist.");
            using (bm = new Bitmap(src))
            {
                bmpixs = bm.Width;
                if (bm.Height != bmpixs) throw new Exception("The width and height of the bitmap should be the same.");
                bmwidth = width;
                bmphas = new double[bmpixs, bmpixs];
                for (int i = 0; i < bmpixs; i++)
                {
                    for (int j = 0; j < bmpixs; j++)
                    {
                        bmphas[i, j] = bm.GetPixel(i, bmpixs - 1 - j).GetHue() / 180.0 * Math.PI;
                    }
                }
            };
        }
        internal static void ClearBitmap()
        {
            bmphas = null;
        }
        static double BMPhase(double x, double y)
        {
            if (bmphas is null) return 0.0;
            x = (x / bmwidth + 0.5) * bmpixs;
            y = (y / bmwidth + 0.5) * bmpixs;
            if (x < 0 || x >= bmpixs || y < 0 || y >= bmpixs) return 0;
            return bmphas[(int)x, (int)y];
        }
        internal static Complex IdealCor(double x1, double y1, double x2, double y2)
        {
            double kx1 = kfact * x1;
            double ky1 = kfact * y1;
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = -kx1 * x2 - ky1 * y2 + BMPhase(x2, y2);
            double amp = Math.Exp(-2.0 * a * (x2 * x2 + y2 * y2));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
        internal static Complex GoodCor(double x1, double y1, double x2, double y2)
        {
            double kx1 = kfact * x1;
            double ky1 = kfact * y1;
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = -b * (kx1 * x2 + ky1 * y2) / (a + b) + BMPhase(x2, y2);// + 2*Math.PI*x2;
            double amp = Math.Exp(-(kx1 * kx1 + ky1 * ky1 + 4.0 * a * (a + b + b) * (x2 * x2 + y2 * y2)) / ((a + b) * 4.0));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
        internal static Complex BadCor(double x1, double y1, double x2, double y2)
        {
            double kx1 = kfact * x1;
            double ky1 = kfact * y1;
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = -bb * (kx1 * x2 + ky1 * y2) / (a + bb) + BMPhase(x2, y2);// + 2*Math.PI*x2;
            double amp = Math.Exp(-(kx1 * kx1 + ky1 * ky1 + 4.0 * a * (a + bb + bb) * (x2 * x2 + y2 * y2)) / ((a + bb) * 4.0));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }

        internal static Complex NoCor(double x1, double y1, double x2, double y2)
        {
            double kx1 = kfact * x1;
            double ky1 = kfact * y1;
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = BMPhase(x2, y2);
            double amp = Math.Exp(-(kx1 * kx1 + ky1 * ky1 + 4.0 * a * a * (x2 * x2 + y2 * y2)) / (a * 4.0));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
        internal static Complex GoodCorNoFourier(double x1, double y1, double x2, double y2)
        {
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = BMPhase(x2, y2);
            double amp = Math.Exp(-(a * (x1 * x1 + y1 * y1 + x2 * x2 + y2 * y2) + b * ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2))));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
        internal static Complex BadCorNoFourier(double x1, double y1, double x2, double y2)
        {
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = BMPhase(x2, y2);
            double amp = Math.Exp(-(a * (x1 * x1 + y1 * y1 + x2 * x2 + y2 * y2) + bb * ((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2))));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
        internal static Complex NoCorNoFourier(double x1, double y1, double x2, double y2)
        {
            //x2 /= magnify;
            //y2 /= magnify;
            double phas = BMPhase(x2, y2);
            double amp = Math.Exp(-(a * (x1 * x1 + y1 * y1 + x2 * x2 + y2 * y2)));
            return new Complex(amp * Math.Cos(phas), amp * Math.Sin(phas));
        }
    }
}
