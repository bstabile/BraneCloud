using System;
using System.Collections.Generic;
using SharpMatrix.Data;
using SharpMatrix.Dense.Row;

namespace SharpMatrix.Equation
{
    //package org.ejml.equation;

/**
 * matrix used to construct a matrix from a sequence of concatenations.
 *
 * @author Peter Abeles
 */
    public class MatrixConstructor
    {

        VariableMatrix output;
        List<Item> items = new List<Item>();


        public MatrixConstructor(ManagerTempVariables manager)
        {
            this.output = manager.createMatrix();
        }

        public void addToRow(Variable variable)
        {
            if (variable.getType() == VariableType.INTEGER_SEQUENCE)
            {
                if (((VariableIntegerSequence) variable).sequence.requiresMaxIndex())
                    throw new ParseError("Trying to create a matrix with an unbounded integer range." +
                                         " Forgot a value after a colon?");
            }
            items.Add(new Item(variable));
        }

        public void endRow()
        {
            items.Add(new Item());
        }

        public void construct()
        {
            // make sure the last item is and end row
            if (!items[items.Count - 1].endRow)
                endRow();

            // have to initialize some variable types first to get the actual size
            for (int i = 0; i < items.Count; i++)
            {
                items[i].initialize();
            }

            setToRequiredSize(output.matrix);

            int matrixRow = 0;
            List<Item> row = new List<Item>();
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                if (item.endRow)
                {
                    int expectedRows = 0;
                    int numCols = 0;
                    for (int j = 0; j < row.Count; j++)
                    {
                        Item v = row[j];

                        int numRows = v.getRows();

                        if (j == 0)
                        {
                            expectedRows = numRows;
                        }
                        else if (v.getRows() != expectedRows)
                        {
                            throw new InvalidOperationException("Row miss-matched. " + numRows + " " + v.getRows());
                        }

                        if (v.matrix)
                        {
                            CommonOps_DDRM.insert(v.getMatrix(), output.matrix, matrixRow, numCols);
                        }
                        else if (v.variable.getType() == VariableType.SCALAR)
                        {
                            output.matrix.set(matrixRow, numCols, v.getValue());
                        }
                        else if (v.variable.getType() == VariableType.INTEGER_SEQUENCE)
                        {
                            IntegerSequence sequence = ((VariableIntegerSequence) v.variable).sequence;
                            int col = numCols;
                            while (sequence.hasNext())
                            {
                                output.matrix.set(matrixRow, col++, sequence.next());
                            }
                        }
                        else
                        {
                            throw new ParseError("Can't insert a variable of type " + v.variable.getType() +
                                                 " inside a matrix!");
                        }
                        numCols += v.getColumns();
                    }

                    matrixRow += expectedRows;
                    row.Clear();
                }
                else
                {
                    row.Add(item);
                }
            }

        }

        public VariableMatrix getOutput()
        {
            return output;
        }

        protected void setToRequiredSize(DMatrixRMaj matrix)
        {


            int matrixRow = 0;
            int matrixCol = 0;
            List<Item> row = new List<Item>();
            for (int i = 0; i < items.Count; i++)
            {
                Item item = items[i];

                if (item.endRow)
                {
                    Item v = row[0];
                    int numRows = v.getRows();
                    int numCols = v.getColumns();
                    for (int j = 1; j < row.Count; j++)
                    {
                        v = row[j];
                        if (v.getRows() != numRows)
                            throw new InvalidOperationException("Row miss-matched. " + numRows + " " + v.getRows());
                        numCols += v.getColumns();
                    }
                    matrixRow += numRows;

                    if (matrixCol == 0)
                        matrixCol = numCols;
                    else if (matrixCol != numCols)
                        throw new ParseError("Row " + matrixRow + " has an unexpected number of columns; expected = " +
                                             matrixCol + " found = " + numCols);

                    row.Clear();
                }
                else
                {
                    row.Add(item);
                }
            }

            matrix.reshape(matrixRow, matrixCol);
        }


        private class Item
        {
            protected internal Variable variable;
            protected internal bool endRow;
            protected internal bool matrix;

            protected internal Item(Variable variable)
            {
                this.variable = variable;
                matrix = variable is VariableMatrix;
            }

            protected internal Item()
            {
                endRow = true;
            }

            public int getRows()
            {
                if (matrix)
                {
                    return ((VariableMatrix) variable).matrix.numRows;
                }
                else
                {
                    return 1;
                }
            }

            public int getColumns()
            {
                if (matrix)
                {
                    return ((VariableMatrix) variable).matrix.numCols;
                }
                else if (variable.getType() == VariableType.SCALAR)
                {
                    return 1;
                }
                else if (variable.getType() == VariableType.INTEGER_SEQUENCE)
                {
                    return ((VariableIntegerSequence) variable).sequence.length();
                }
                else
                {
                    throw new InvalidOperationException("BUG! Should have been caught earlier");
                }
            }

            public DMatrixRMaj getMatrix()
            {
                return ((VariableMatrix) variable).matrix;
            }

            public double getValue()
            {
                return ((VariableScalar) variable).getDouble();
            }

            public void initialize()
            {
                if (variable != null && !matrix && variable.getType() == VariableType.INTEGER_SEQUENCE)
                {
                    ((VariableIntegerSequence) variable).sequence.initialize(-1);
                }
            }
        }
    }
}