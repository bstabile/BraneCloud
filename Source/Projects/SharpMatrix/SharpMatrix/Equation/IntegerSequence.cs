using System;
using System.Collections.Generic;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

    public enum IntegerSequenceType
    {
        EXPLICIT,
        FOR,
        COMBINED,
        RANGE
    }

    /**
     * Interface for an ordered sequence of integer values
     *
     * @author Peter Abeles
     */
    public interface IntegerSequence
    {

        int length();

        /**
         * Specifies the maximum index of the array.  If the maximum index is not known then a value &lt; 0 is passed
         * in and an exception should be thrown if this information is required
         *
         * NOTE: This is length - 1
         *
         * @param maxIndex Largest possible value in the sequence. or &lt; 0 if unknown
         */
        void initialize(int maxIndex);

        int next();

        bool hasNext();

        IntegerSequenceType getType();

        bool requiresMaxIndex();


    }
    class For : IntegerSequence
    {

        VariableInteger start;
        VariableInteger step;
        VariableInteger end;

        int valStart;
        int valStep;
        int valEnd;
        int where;
        int _length;

        public For(TokenList.Token start, TokenList.Token step, TokenList.Token end)
        {
            this.start = (VariableInteger)start.getVariable();
            this.step = step == null ? null : (VariableInteger)step.getVariable();
            this.end = (VariableInteger)end.getVariable();
        }

        //@Override
        public int length()
        {
            return _length;
        }

        //@Override
        public void initialize(int maxIndex)
        {
            valStart = start.value;
            valEnd = end.value;
            if (step == null)
            {
                valStep = 1;
            }
            else
            {
                valStep = step.value;
            }

            if (valStep <= 0)
            {
                throw new ArgumentException("step size must be a positive integer");
            }
            if (valEnd < valStart)
            {
                throw new ArgumentException("end value must be >= the start value");
            }

            where = 0;
            _length = (valEnd - valStart) / valStep + 1;

        }

        //@Override
        public int next()
        {
            return valStart + valStep * where++;
        }

        //@Override
        public bool hasNext()
        {
            return where < _length;
        }

        public int getStart()
        {
            return valStart;
        }

        public int getStep()
        {
            return valStep;
        }

        public int getEnd()
        {
            return valEnd;
        }

        //@Override
        public IntegerSequenceType getType()
        {
            return IntegerSequenceType.FOR;
        }

        //@Override
        public bool requiresMaxIndex()
        {
            return false;
        }
    }

    /**
 * An array of integers which was explicitly specified
 */
    public class Explicit : IntegerSequence
    {

        List<VariableInteger> sequence = new List<VariableInteger>();
        int where;

        public Explicit(TokenList.Token start, TokenList.Token end)
        {
            TokenList.Token t = start;
            while (true)
            {
                sequence.Add((VariableInteger)t.getVariable());
                if (t == end)
                {
                    break;
                }
                else
                {
                    t = t.next;
                }
            }
        }

        public Explicit(TokenList.Token single)
        {
            sequence.Add((VariableInteger)single.getVariable());
        }

        //@Override
        public int length()
        {
            return sequence.Count;
        }

        //@Override
        public void initialize(int maxIndex)
        {
            where = 0;
        }

        //@Override
        public int next()
        {
            return sequence[where++].value;
        }

        //@Override
        public bool hasNext()
        {
            return where < sequence.Count;
        }

        //@Override
        public IntegerSequenceType getType()
        {
            return IntegerSequenceType.EXPLICIT;
        }

        //@Override
        public bool requiresMaxIndex()
        {
            return false;
        }

        public List<VariableInteger> getSequence()
        {
            return sequence;
        }
    }

    /**
     * A sequence of integers which has been specified using a start number, end number, and step size.
     *
     * 2:3:21 = 2 5 8 11 14 17 20
     */

    /**
     * This is a sequence of sequences
     */
    public class Combined : IntegerSequence
    {

        List<IntegerSequence> sequences = new List<IntegerSequence>();

        int which;

        public Combined(TokenList.Token start, TokenList.Token end)
        {

            TokenList.Token t = start;
            do
            {
                if (t.getVariable().getType() == VariableType.SCALAR)
                {
                    sequences.Add(new Explicit(t));
                }
                else if (t.getVariable().getType() == VariableType.INTEGER_SEQUENCE)
                {
                    sequences.Add(((VariableIntegerSequence)t.getVariable()).sequence);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected token type");
                }
                t = t.next;
            } while (t != null && t.previous != end);
        }

        //@Override
        public int length()
        {
            int total = 0;
            for (int i = 0; i < sequences.Count; i++)
            {
                total += sequences[i].length();
            }
            return total;
        }

        //@Override
        public void initialize(int maxIndex)
        {
            which = 0;
            for (int i = 0; i < sequences.Count; i++)
            {
                sequences[i].initialize(maxIndex);
            }
        }

        //@Override
        public int next()
        {
            int output = sequences[which].next();

            if (!sequences[which].hasNext())
            {
                which++;
            }

            return output;
        }

        //@Override
        public bool hasNext()
        {
            return which < sequences.Count;
        }

        //@Override
        public IntegerSequenceType getType()
        {
            return IntegerSequenceType.COMBINED;
        }

        //@Override
        public bool requiresMaxIndex()
        {
            for (int i = 0; i < sequences.Count; i++)
            {
                if (sequences[i].requiresMaxIndex())
                    return true;
            }
            return false;
        }
    }

    /**
     * A sequence of integers which has been specified using a start number, end number, and step size and uses
     * the known upper limit of the array to bound it
     *
     * Examples:
     * :
     * 2:
     * 2:3:
     */
    public class Range : IntegerSequence
    {

        VariableInteger start;
        VariableInteger step;

        int valStart;
        int valStep;
        int valEnd;
        int where;
        int _length;

        public Range(TokenList.Token start, TokenList.Token step)
        {
            this.start = start == null ? null : (VariableInteger)start.getVariable();
            this.step = step == null ? null : (VariableInteger)step.getVariable();
        }

        //@Override
        public int length()
        {
            return _length;
        }

        //@Override
        public void initialize(int maxIndex)
        {
            if (maxIndex < 0)
                throw new ArgumentException(
                    "Range sequence being used inside an object without a known upper limit");
            valEnd = maxIndex;

            if (start != null)
                valStart = start.value;
            else
                valStart = 0;

            if (step == null)
            {
                valStep = 1;
            }
            else
            {
                valStep = step.value;
            }

            if (valStep <= 0)
            {
                throw new ArgumentException("step size must be a positive integer");
            }

            where = 0;
            _length = (valEnd - valStart) / valStep + 1;

        }

        //@Override
        public int next()
        {
            return valStart + valStep * where++;
        }

        //@Override
        public bool hasNext()
        {
            return where < _length;
        }

        public int getStart()
        {
            return valStart;
        }

        public int getStep()
        {
            return valStep;
        }

        public int getEnd()
        {
            return valEnd;
        }

        //@Override
        public IntegerSequenceType getType()
        {
            return IntegerSequenceType.RANGE;
        }

        //@Override
        public bool requiresMaxIndex()
        {
            return true;
        }
    }

}