namespace SharpMatrix.Data
{
    /**
     * @author Peter Abeles
     */
    public class MatrixType
    {
        /// <summary>
        /// To compare the "FixedCode" with the "CanonicalCode"
        /// you can do this: 
        ///     matrixType.CanonicalCode == (matrixType.FixedCode & MatrixType.FixedFlagMask);
        /// or this:
        ///     ((uint)MatrixType.Code.DDRM) == (matrixType.FixedCode & MatrixType.FixedFlagMask);
        /// </summary>
        public const uint FixedFlagMask = 0b0011_1111_1111_1111;

        /// <summary>
        /// These codes do not indicate whether or not the matrix is "fixed".
        /// They only indicate if the matrix is real or complex, if the
        /// matrix is dense or sparse, and the number of bits (64 or 32).
        /// The "UNSPECIFIED" value is zero.
        /// </summary>
        public enum Code : uint
        {
            // NOTE: These binary literals are only valid as of C#7
            DDRM = 0b0011_0000_0100_0000,
            FDRM = 0b0011_0000_0010_0000,
            ZDRM = 0b0001_0000_0100_0000,
            CDRM = 0b0001_0000_0010_0000,
            DSCC = 0b0010_0000_0100_0000,
            FSCC = 0b0010_0000_0010_0000,
            ZSCC = 0b0000_0000_0100_0000,
            CSCC = 0b0000_0000_0010_0000,
            UNSPECIFIED = 0b0000_0000_0000_0000,
        }

        public static readonly MatrixType DDRM = new MatrixType(true, true, 64);

        //FDRM(true,true,32),
        public static readonly MatrixType FDRM = new MatrixType(true, true, 32);

        //ZDRM(false,true,64),
        public static readonly MatrixType ZDRM = new MatrixType(false, true, 64);

        //CDRM(false,true,32),
        public static readonly MatrixType CDRM = new MatrixType(false, true, 32);

        //DSCC(true,false,64),
        public static readonly MatrixType DSCC = new MatrixType(true, false, 64);

        //FSCC(true,false,32),
        public static readonly MatrixType FSCC = new MatrixType(true, false, 32);

        //ZSCC(false,false,64),
        public static readonly MatrixType ZSCC = new MatrixType(false, false, 64);

        //CSCC(false,false,32),
        public static readonly MatrixType CSCC = new MatrixType(false, false, 32);

        //UNSPECIFIED(false,false,0);
        public static readonly MatrixType UNSPECIFIED = new MatrixType(false, false, 0);

        readonly bool _fixed;
        readonly bool _dense;
        readonly bool _real;
        readonly int _bits;

        public MatrixType(bool real, bool dense, int bits)
            : this(false, real, dense, bits)
        {
        }

        public MatrixType(bool isfixed, bool real, bool dense, int bits)
        {
            _real = real;
            _fixed = isfixed;
            _dense = dense;
            _bits = bits;

            FixedCode = CalculateCode(isfixed, real, dense, bits);
            CanonicalCode = CalculateCode(real, dense, bits);
        }

        public bool isReal()
        {
            return _real;
        }

        public bool isFixed()
        {
            return _fixed;
        }

        public bool isDense()
        {
            return _dense;
        }

        public int getBits()
        {
            return _bits;
        }

        public uint FixedCode { get; }
        public uint CanonicalCode { get; }

        /// <summary>
        /// This encoding returns the "canonical" code by setting flags 
        /// in the top nibble (shifts 13,12 for [real, dense]).
        /// and then "|" that result with the bits value.
        // Example: 64 bit Real-Dense = 0x3040 (0011 0000 0100 0000)
        // Example: 32 bit Real-Dense = 0x3020 (0011 0000 0010 0000)
        // NOTE: The "isFixed" flag is ignored.
        /// </summary>
        public static uint CalculateCode(MatrixType matrixType, bool includeFixedFlag = false)
        {
            if (includeFixedFlag)
                return CalculateCode(
                    matrixType._fixed, 
                    matrixType._real, 
                    matrixType._dense,
                    matrixType._bits);
            return CalculateCode(
                matrixType._real,
                matrixType._dense,
                matrixType._bits);
        }

        /// <summary>
        /// This encoding returns the "canonical" code by setting flags 
        /// in the top nibble (shifts 13,12 for [real, dense]).
        /// and then "|" that result with the bits value.
        // Example: 64 bit Real-Dense = 0x3040 (0011 0000 0100 0000)
        // Example: 32 bit Real-Dense = 0x3020 (0011 0000 0010 0000)
        // NOTE: The "isFixed" flag is ignored.
        /// </summary>
        public static uint CalculateCode(bool real, bool dense, int bits)
        {
            var code = (uint)bits;
            // Currently all known matrix types are assumed to be NOT fixed.
            //code = code | (uint)(isfixed ? 1 << 14 : 0);
            code = code | (uint)(real ? 1 << 13 : 0);
            code = code | (uint)(dense ? 1 << 12 : 0);
            return code;
        }

        /// <summary>
        /// This encoding sets flags in the top nibble 
        /// (shifts 14,13,12 for [fixed, real, dense]).
        /// and then "|" that result with the bits value.
        /// Example: 64 bit Fixed-Real-Dense = 0x7040 (0111 0000 0100 0000)
        /// Example: 32 bit Fixed-Real-Dense = 0x7020 (0111 0000 0010 0000)
        /// NOTE: Only use this version if you want the "isFixed" flag encoded.
        ///       To compare this with the "canonical" code the 15th bit must be masked!
        ///       Example: matrixType.CanonicalCode == MatrixType.FixedCode & MatrixType.FixedFlagMask;
        /// </summary>
        public static uint CalculateCode(bool isfixed, bool real, bool dense, int bits)
        {
            var code = (uint)bits;
            // Currently all known matrix types are assumed to be NOT fixed.
            code = code | (uint)(isfixed ? 1 << 14 : 0);
            code = code | (uint)(real ? 1 << 13 : 0);
            code = code | (uint)(dense ? 1 << 12 : 0);
            return code;
        }

    }
}