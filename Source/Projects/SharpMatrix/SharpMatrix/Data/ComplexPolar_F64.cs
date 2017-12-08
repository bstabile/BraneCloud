using System;
using SharpMatrix.Ops;

namespace SharpMatrix.Data
{
    /**
     * <p>
     * {@link Complex_F64} number in polar notation.<br>
     * z = r*(cos(&theta;) + i*sin(&theta;))<br>
     * where r and &theta; are polar coordinate parameters
     * </p>
     * @author Peter Abeles
     */
    public class ComplexPolar_F64
    {
        public double r;
        public double theta;

        public ComplexPolar_F64(double r, double theta)
        {
            this.r = r;
            this.theta = theta;
        }

        public ComplexPolar_F64(Complex_F64 n)
        {
            ComplexMath_F64.convert(n, this);
        }

        public ComplexPolar_F64()
        {
        }

        public Complex_F64 toStandard()
        {
            Complex_F64 ret = new Complex_F64();
            ComplexMath_F64.convert(this, ret);
            return ret;
        }

        public double getR()
        {
            return r;
        }

        public void setR(double r)
        {
            this.r = r;
        }

        public double getTheta()
        {
            return theta;
        }

        public void setTheta(double theta)
        {
            this.theta = theta;
        }

        public string toString()
        {
            return "( r = " + r + " theta = " + theta + " )";
        }
    }
}