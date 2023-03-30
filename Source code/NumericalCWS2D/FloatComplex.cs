using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace NumericalCWS2D
{
    [Serializable]
    public struct FloatComplex
    {
		private float m_real;
		private float m_imaginary;
		public static readonly FloatComplex Zero = new FloatComplex(0.0f, 0.0f);
		public static readonly FloatComplex One = new FloatComplex(1.0f, 0.0f);
		public static readonly FloatComplex ImaginaryOne = new FloatComplex(0.0f, 1.0f);
		public static readonly FloatComplex NegativeImaginaryOne = new FloatComplex(0.0f, -1.0f);
		public float Real
		{
			get
			{
				return m_real;
			}
		}
		public float Imaginary
		{
			get
			{
				return m_imaginary;
			}
		}
		public double Magnitude
		{
			get
			{
				return Abs(this);
			}
		}
		public double Phase
		{
			get
			{
				return Math.Atan2(m_imaginary, m_real);
			}
		}
		public FloatComplex(float real, float imaginary)
		{
			m_real = real;
			m_imaginary = imaginary;
		}
		public FloatComplex(double real, double imaginary)
		{
			m_real = (float)real;
			m_imaginary = (float)imaginary;
		}
		public static FloatComplex FromPolarCoordinates(double magnitude, double phase)
		{
			return new FloatComplex((float)(magnitude * Math.Cos(phase)), (float)(magnitude * Math.Sin(phase)));
		}
		public static FloatComplex Negate(FloatComplex value)
		{
			return -value;
		}
		public static FloatComplex Add(FloatComplex left, FloatComplex right)
		{
			return left + right;
		}
		public static FloatComplex Subtract(FloatComplex left, FloatComplex right)
		{
			return left - right;
		}
		public static FloatComplex Multiply(FloatComplex left, FloatComplex right)
		{
			return left * right;
		}
		public static FloatComplex operator -(FloatComplex value)
		{
			return new FloatComplex(0.0f - value.m_real, 0.0f - value.m_imaginary);
		}
		public static FloatComplex operator +(FloatComplex left, FloatComplex right)
		{
			return new FloatComplex(left.m_real + right.m_real, left.m_imaginary + right.m_imaginary);
		}
		public static FloatComplex operator -(FloatComplex left, FloatComplex right)
		{
			return new FloatComplex(left.m_real - right.m_real, left.m_imaginary - right.m_imaginary);
		}
		public static FloatComplex operator *(FloatComplex left, FloatComplex right)
		{
			float real = left.m_real * right.m_real - left.m_imaginary * right.m_imaginary;
			float imaginary = left.m_imaginary * right.m_real + left.m_real * right.m_imaginary;
			return new FloatComplex(real, imaginary);
		}
		public static FloatComplex operator *(FloatComplex left, double right)
		{
			float r = (float)right;
			return new FloatComplex(left.m_real * r, left.m_imaginary * r);
		}
		public static FloatComplex operator /(FloatComplex left, double right)
		{
			float r = (float)right;
			return new FloatComplex(left.m_real / r, left.m_imaginary / r);
		}
		public static double Abs(FloatComplex value)
		{
			if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
			{
				return double.PositiveInfinity;
			}
			double num = value.m_real;
			double num2 = value.m_imaginary;
			return Math.Sqrt(num * num + num2 * num2);
		}
		public static double m2(FloatComplex value)
		{
			if (float.IsInfinity(value.m_real) || float.IsInfinity(value.m_imaginary))
			{
				return double.PositiveInfinity;
			}
			double num = value.m_real;
			double num2 = value.m_imaginary;
			return num * num + num2 * num2;
		}
		public static FloatComplex Conjugate(FloatComplex value)
		{
			return new FloatComplex(value.m_real, -value.m_imaginary);
		}
		public static FloatComplex Reciprocal(FloatComplex value)
		{
			if (value.m_real == 0.0 && value.m_imaginary == 0.0)
			{
				throw new DivideByZeroException();
			}
			float m = (float)m2(value);
			return new FloatComplex(value.m_real / m, -value.m_imaginary / m);
		}
		public static bool operator ==(FloatComplex left, FloatComplex right)
		{
			if (left.m_real == right.m_real)
			{
				return left.m_imaginary == right.m_imaginary;
			}
			return false;
		}
		public static bool operator !=(FloatComplex left, FloatComplex right)
		{
			if (left.m_real == right.m_real)
			{
				return left.m_imaginary != right.m_imaginary;
			}
			return true;
		}
		public override bool Equals(object obj)
		{
			if (!(obj is FloatComplex))
			{
				return false;
			}
			if (obj is Complex) return this == (FloatComplex)obj;
			return this == (FloatComplex)obj;
		}
		public bool Equals(FloatComplex value)
		{
			if (m_real.Equals(value.m_real))
			{
				return m_imaginary.Equals(value.m_imaginary);
			}
			return false;
		}
		public bool Equals(Complex value)
		{
			return this == (FloatComplex)value;
		}
		public static explicit operator FloatComplex(Complex value)
        {
			return new FloatComplex(value.Real, value.Imaginary);
        }
		public static implicit operator Complex(FloatComplex value)
		{
			return new Complex(value.Real, value.Imaginary);
		}
		public static explicit operator FloatComplex(float value)
		{
			return new FloatComplex(value, 0.0f);
		}
		public static explicit operator FloatComplex(double value)
		{
			return new FloatComplex((float)value, 0.0f);
		}
		public static explicit operator FloatComplex(int value)
		{
			return new FloatComplex(value, 0.0f);
		}
		public static explicit operator FloatComplex(long value)
		{
			return new FloatComplex(value, 0.0f);
		}
		public override string ToString()
		{
			return string.Format(System.Globalization.CultureInfo.CurrentCulture, "({0}, {1})", new object[2]
			{
			m_real,
			m_imaginary
			});
		}
		public override int GetHashCode()
		{
			int num = 99999997;
			int num2 = m_real.GetHashCode() % num;
			int hashCode = m_imaginary.GetHashCode();
			return num2 ^ hashCode;
		}
	}
}
