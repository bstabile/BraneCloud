
namespace BraneCloud.Evolution.EC.MatrixLib.Sparse
{
    //package org.ejml.sparse;

/**
 * Different types of fill in reducing techniques that can be selected
 *
 * @author Peter Abeles
 */
    public enum FillReducing
    {
        /**
         * No fill reduction permutation will be applied
         */
        NONE,

        /**
         * TESTING ONLY. Completely random permutation
         */
        RANDOM,

        /**
         * TESTING ONLY. Doesn't change the input.
         */
        IDENTITY
    }
}