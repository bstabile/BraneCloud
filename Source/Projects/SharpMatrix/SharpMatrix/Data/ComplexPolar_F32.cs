using System;

namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * <p>
     * {@link Complex_F32} number in polar notation.<br>
     * z = r*(cos(&theta;) + i*sin(&theta;))<br>
     * where r and &theta; are polar coordinate parameters
     * </p>
     * @author Peter Abeles
     */
    public class ComplexPolar_F32
    {
        public float r;
        public float theta;

        public ComplexPolar_F32(float r, float theta)
        {
            this.r = r;
            this.theta = theta;
        }

        public ComplexPolar_F32(Complex_F32 n)
        {
            ComplexMath_F32.convert(n, this);
        }

        public ComplexPolar_F32()
        {
        }

        public Complex_F32 toStandard()
        {
            Complex_F32 ret = new Complex_F32();
            ComplexMath_F32.convert(this, ret);
            return ret;
        }

        public float getR()
        {
            return r;
        }

        public void setR(float r)
        {
            this.r = r;
        }

        public float getTheta()
        {
            return theta;
        }

        public void setTheta(float theta)
        {
            this.theta = theta;
        }

        public string toString()
        {
            return "( r = " + r + " theta = " + theta + " )";
        }
    }
}