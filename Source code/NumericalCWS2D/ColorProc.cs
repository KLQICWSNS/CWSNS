using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NumericalCWS2D
{
    static class ColorProc
    {
        internal static Color Complex2Color(FloatComplex c)
        {
            if (c.Real is float.NaN) return Color.Black;
            return Complex2Color(c.Magnitude, c.Phase);
        }
        internal static Color Complex2Color(Complex c)
        {
            if (c.Real is double.NaN) return Color.Black;
            return Complex2Color(c.Magnitude, c.Phase);
        }
        internal static Color Complex2Color(Complex c, bool CheckMod)
        {
            if (c.Real is double.NaN) return Color.Black;
            if(CheckMod && c.mo2() > 1.0000000000001) throw new Exception("Please check the amplitude of the wavefunction.");
            return Complex2Color(c.Magnitude, c.Phase);
        }
        internal static Color Complex2Color(double ampl, double hue)//hue is radian like phase
        {
            if (ampl is double.NaN) return Color.Black;
            if (ampl < 0) throw new ArgumentOutOfRangeException();
            if (ampl > 1.0) ampl = 1.0;
            int ac = (int)(ampl * 255.99999);
            if (hue is double.NaN)
            {
                return Color.FromArgb(ac, ac, ac);
            }
            if (hue >= 0)
            {
                hue = (hue * 180.0 / Math.PI) % 360.0;
            }
            else
            {
                hue = 360.0 - ((-hue * 180.0 / Math.PI) % 360.0);
            }
            if (hue <= 60.0)
            {
                return Color.FromArgb(ac, (byte)(hue * ampl * 255.99999 / 60.0), 0);
            }
            else if (hue <= 120.0)
            {
                return Color.FromArgb((byte)((120.0 - hue) * ampl * 255.99999 / 60.0), ac, 0);
            }
            else if (hue <= 180.0)
            {
                return Color.FromArgb(0, ac, (byte)((hue - 120.0) * ampl * 255.99999 / 60.0));
            }
            else if (hue <= 240.0)
            {
                return Color.FromArgb(0, (byte)((240.0 - hue) * ampl * 255.99999 / 60.0), ac);
            }
            else if (hue <= 300.0)
            {
                return Color.FromArgb((byte)((hue - 240.0) * ampl * 255.99999 / 60.0), 0, ac);
            }
            else
            {
                return Color.FromArgb(ac, 0, (byte)((360.0 - hue) * ampl * 255.99999 / 60.0));
            }
        }
    }
}
