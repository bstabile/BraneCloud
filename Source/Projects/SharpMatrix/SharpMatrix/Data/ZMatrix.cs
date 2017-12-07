namespace BraneCloud.Evolution.EC.MatrixLib.Data
{
    /**
     * Interface for all complex 64 bit floating point rectangular matrices.
     *
     * @author Peter Abeles
     */
    public interface ZMatrix : Matrix
    {

        /**
         * Returns the complex value of the matrix's element
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @param output Storage for the complex number
         */
        void get(int row, int col, Complex_F64 output);

        /**
         * Set's the complex value of the matrix's element
         *
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @param real The real component
         * @param imaginary The imaginary component
         */
        void set(int row, int col, double real, double imaginary);

        /**
         * Returns the real component of the matrix's element.
         *
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @return The specified element's value.
         */
        double getReal(int row, int col);


        /**
         * Sets the real component of the matrix's element.
         *
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @param val  The element's new value.
         */
        void setReal(int row, int col, double val);

        /**
         * Returns the imaginary component of the matrix's element.
         *
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @return The specified element's value.
         */
        double getImag(int row, int col);


        /**
         * Sets the imaginary component of the matrix's element.
         *
         * @param row Matrix element's row index..
         * @param col Matrix element's column index.
         * @param val  The element's new value.
         */
        void setImag(int row, int col, double val);

        /**
         * Returns the number of elements in the internal data array
         *
         * @return Number of elements in the data array.
         */
        int getDataLength();

    }
}