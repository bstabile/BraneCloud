/*
 * BraneCloud.Evolution.EC (Evolutionary Computation)
 * Copyright 2011 Bennett R. Stabile (BraneCloud.Evolution.net|com)
 * Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0.html)
 *
 * This is an independent conversion from Java to .NET of ...
 *
 * Sean Luke's ECJ project at GMU 
 * (Academic Free License v3.0): 
 * http://www.cs.gmu.edu/~eclab/projects/ecj
 *
 * Radical alteration was required throughout (including structural).
 * The author of ECJ cannot and MUST not be expected to support this fork.
 *
 * If you wish to create yet another fork, please use a different root namespace.
 * BraneCloud is a registered domain that will be used for name/schema resolution.
 */

using System;

namespace BraneCloud.Evolution.EC.Randomization
{
    /// <summary> 
    /// RandomChoice organizes arrays of floats into distributions which can
    /// be used to pick randomly from.  You can provide three kinds of arrays:
    /// 
    /// <ul>
    /// <li/> An array of floats
    /// <li/> An array of doubles
    /// <li/> An array of arbitrary objects, plus a RandomChoiceChooser which knows
    /// how to get and set the appropriate "float" value of objects in this array.
    /// </ul>
    /// 
    /// <p/>Before the RandomChoice can pick randomly from your array, it must first
    /// organize it.  It does this by doing the following.  First, it normalizes the
    /// values in the array.  Then it modifies them to their sums.  That is, each item i
    /// in the array is set to the sum of the original values for items 0...i.  If you
    /// cannot allow your objects to be modified, then this is not the class for you.
    /// 
    /// <p/>An array is valid if (1) it has no negative values and (2) not all of its
    /// values are zero.  This RandomChoice code <i>should</i> (I hope) guarantee that
    /// an element of zero probability is never returned.  RandomChoice uses a binary
    /// search to find your index, followed by linear probing (marching up or down
    /// the list) to find the first non-zero probability item in the vacinity of that
    /// index.  As long as there are not a whole lot of zero-valued items in a row,
    /// RandomChoice is efficient.
    /// 
    /// You organize your array with organizeDistribution().  Then you can have the
    /// RandomChoice pick random items from the array and return their indexes to you.
    /// You do this by calling pickFromDistribution(), passing it a random floating
    /// point value between 0.0 and 1.0.  You call organizeDistribution() only once;
    /// after which you may call pickFromDistribution() as many times as you like.
    /// You should not modify the array thereafter.
    /// 
    /// </summary>    
    public static class RandomChoice
    {
        #region Constants

        public const int CHECKBOUNDARY = 8;

        #endregion // Constants
        #region ExemptZeroes (Private)

        /// <summary>
        /// allows us to have zero-probability values
        /// </summary>
        private static int ExemptZeroes(float[] probabilities, int index)
        {
            //System.out.PrintLn(index);
            if (probabilities[index] == 0.0f)
            // I need to scan forward because I'm in a left-trail
            // scan forward
            {
                while (index < probabilities.Length - 1 && probabilities[index] == 0.0f)
                    index++;
            }
            // scan backwards
            else
            {
                while (index > 0 && probabilities[index] == probabilities[index - 1])
                    index--;
            }
            return index;
        }

        /// <summary>
        /// allows us to have zero-probability values
        /// </summary>
        private static int ExemptZeroes(double[] probabilities, int index)
        {
            //System.out.PrintLn(index);
            if (probabilities[index] == 0.0)
            // I need to scan forward because I'm in a left-trail
            // scan forward
            {
                while (index < probabilities.Length - 1 && probabilities[index] == 0.0)
                    index++;
            }
            // scan backwards
            else
            {
                while (index > 0 && probabilities[index] == probabilities[index - 1])
                    index--;
            }
            return index;
        }

        /// <summary>
        /// allows us to have zero-probability values
        /// </summary>
        private static int ExemptZeroes(Object[] objs, IRandomChoiceChooser chooser, int index)
        {
            //System.out.PrintLn(index);
            if (chooser.GetProbability(objs[index]) == 0.0f)
            // I need to scan forward because I'm in a left-trail
            // scan forward
            {
                while (index < objs.Length - 1 && chooser.GetProbability(objs[index]) == 0.0f)
                    index++;
            }
            // scan backwards
            else
            {
                while (index > 0 && chooser.GetProbability(objs[index]) == chooser.GetProbability(objs[index - 1]))
                    index--;
            }
            return index;
        }

        /// <summary>
        /// allows us to have zero-probability values
        /// </summary>
        private static int ExemptZeroes(Object[] objs, IRandomChoiceChooserD chooser, int index)
        {
            //System.out.PrintLn(index);
            if (chooser.GetProbability(objs[index]) == 0.0)
            // I need to scan forward because I'm in a left-trail
            // scan forward
            {
                while (index < objs.Length - 1 && chooser.GetProbability(objs[index]) == 0.0)
                    index++;
            }
            // scan backwards
            else
            {
                while (index > 0 && chooser.GetProbability(objs[index]) == chooser.GetProbability(objs[index - 1]))
                    index--;
            }
            return index;
        }

        #endregion // ExemptZeroes (Private)
        #region OrganizeDistribution

        /// <summary>
        /// Same as organizeDistribution(probabilities,  <b>false</b>); 
        /// </summary>
        public static void OrganizeDistribution(float[] probabilities)
        {
            OrganizeDistribution(probabilities, false);
        }
        
        /// <summary>
        /// Normalizes probabilities, then converts them into continuing sums.  
        /// This prepares them for being usable in pickFromDistribution.
        /// If the probabilities are all 0, then selection is uniform, unless allowAllZeros
        /// is false, in which case an ArithmeticException is thrown.  If any of them are negative,
        /// or if the distribution is empty, then an ArithmeticException is thrown.
        /// For example, 
        /// {0.6, 0.4, 0.2, 0.8} -> {0.3, 0.2, 0.1, 0.4} -> {0.3, 0.5, 0.6, 1.0} 
        /// </summary>       
        public static void OrganizeDistribution(float[] probabilities, bool allowAllZeros)
        {
            // first normalize
            var sum = 0.0;
            if (probabilities.Length == 0)
                throw new ArithmeticException("Distribution has no elements");
            
            foreach (var t in probabilities)
            {
                if (t < 0.0)
                    throw new ArithmeticException("Distribution has negative probabilities");
                sum += t;
            }
            
            if (sum == 0.0)
                if (!allowAllZeros)
                    throw new ArithmeticException("Distribution has all zero probabilities");
                else
                {
                    for (var x = 0; x < probabilities.Length; x++)
                        probabilities[x] = 1.0f;
                    sum = probabilities.Length;
                }
            
            for (var x = 0; x < probabilities.Length; x++)
                probabilities[x] = (float) (probabilities[x] / sum);
            
            // now sum
            sum = 0.0;
            for (var x = 0; x < probabilities.Length; x++)
            {
                sum += probabilities[x];
                //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
                probabilities[x] = (float) sum;
            }
            
            // now we need to work backwards setting 0 values
            int x2;
            for (x2 = probabilities.Length - 1; x2 > 0; x2--)
                if (probabilities[x2] == probabilities[x2 - 1])
                // we're 0.0
                    probabilities[x2] = 1.0f;
                else
                    break;
            probabilities[x2] = 1.0f;
        }
        
        /// <summary>
        /// Same as organizeDistribution(probabilities,  <b>false</b>); 
        /// </summary>
        public static void OrganizeDistribution(double[] probabilities)
        {
            OrganizeDistribution(probabilities, false);
        }
        
        /// <summary>
        /// Normalizes probabilities, then converts them into continuing sums.  
        /// This prepares them for being usable in pickFromDistribution.
        /// If the probabilities are all 0, then selection is uniform, unless allowAllZeros
        /// is false, in which case an ArithmeticException is thrown.  If any of them are negative,
        /// or if the distribution is empty, then an ArithmeticException is thrown.
        /// For example, 
        /// {0.6, 0.4, 0.2, 0.8} -> {0.3, 0.2, 0.1, 0.4} -> {0.3, 0.5, 0.6, 1.0} 
        /// </summary>        
        public static void  OrganizeDistribution(double[] probabilities, bool allowAllZeros)
        {
            // first normalize
            var sum = 0.0;
            
            if (probabilities.Length == 0)
                throw new ArithmeticException("Distribution has no elements");
            
            foreach (var t in probabilities)
            {
                if (t < 0.0)
                    throw new ArithmeticException("Distribution has negative probabilities");
                sum += t;
            }
            
            if (sum == 0.0)
                if (!allowAllZeros)
                    throw new ArithmeticException("Distribution has all zero probabilities");
                else
                {
                    for (var x = 0; x < probabilities.Length; x++)
                        probabilities[x] = 1.0;
                    sum = probabilities.Length;
                }
            
            for (var x = 0; x < probabilities.Length; x++)
                probabilities[x] /= sum;
            
            // now sum
            sum = 0.0;
            for (var x = 0; x < probabilities.Length; x++)
            {
                sum += probabilities[x];
                probabilities[x] = sum;
            }
            
            // now we need to work backwards setting 0 values
            int x2;
            for (x2 = probabilities.Length - 1; x2 > 0; x2--)
                if (probabilities[x2] == probabilities[x2 - 1])
                // we're 0.0
                    probabilities[x2] = 1.0;
                else
                    break;
            probabilities[x2] = 1.0;
        }
        
        /// <summary>
        /// Same as organizeDistribution(objs, chooser, <b>false</b>); 
        /// </summary>
        public static void  OrganizeDistribution(Object[] objs, IRandomChoiceChooser chooser)
        {
            OrganizeDistribution(objs, chooser, false);
        }
        
        /// <summary>
        /// Normalizes the probabilities associated with an array of objects, 
        /// then converts them into continuing sums.  
        /// This prepares them for being usable in pickFromDistribution.
        /// If the probabilities are all 0, then selection is uniform, 
        /// unless allowAllZeros is false, in which case an ArithmeticException is thrown.  
        /// If any of them are negative, or if the distribution is empty, 
        /// then an ArithmeticException is thrown.
        /// For example, 
        /// {0.6, 0.4, 0.2, 0.8} -> {0.3, 0.2, 0.1, 0.4} -> {0.3, 0.5, 0.6, 1.0} 
        /// The probabilities are retrieved and set using chooser.
        /// </summary>        
        public static void  OrganizeDistribution(Object[] objs, IRandomChoiceChooser chooser, bool allowAllZeros)
        {
            // first normalize
            var sum = 0.0;
            
            if (objs.Length == 0)
                throw new ArithmeticException("Distribution has no elements");
            
            foreach (var t in objs)
            {
                if (chooser.GetProbability(t) < 0.0)
                    throw new ArithmeticException("Distribution has negative probabilities");
                sum += chooser.GetProbability(t);
            }
            
            if (sum == 0.0)
                if (!allowAllZeros)
                    throw new ArithmeticException("Distribution has all zero probabilities");
                else
                {
                    foreach (var t in objs)
                        chooser.SetProbability(t, 1.0f);
                    sum = objs.Length;
                }

            foreach (var t in objs)
            {
                //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
                chooser.SetProbability(t, (float) (chooser.GetProbability(t) / sum));
            }

            // now sum
            sum = 0.0;
            foreach (var t in objs)
            {
                sum += chooser.GetProbability(t);
                //UPGRADE_WARNING: Data types in Visual C# might be different.  Verify the accuracy of narrowing conversions. "ms-help://MS.VSCC.v80/dv_commoner/local/redirect.htm?index='!DefaultContextWindowIndex'&keyword='jlca1042'"
                chooser.SetProbability(t, (float) sum);
            }
            
            // now we need to work backwards setting 0 values
            int x2;
            for (x2 = objs.Length - 1; x2 > 0; x2--)
                if (chooser.GetProbability(objs[x2]) == chooser.GetProbability(objs[x2 - 1]))
                // we're 0.0
                    chooser.SetProbability(objs[x2], 1.0f);
                else
                    break;
            chooser.SetProbability(objs[x2], 1.0f);
        }
        
        /// <summary>
        /// Same as organizeDistribution(objs, chooser, <b>false</b>); 
        /// </summary>
        public static void  OrganizeDistribution(Object[] objs, IRandomChoiceChooserD chooser)
        {
            OrganizeDistribution(objs, chooser, false);
        }
        
        /// <summary>
        /// Normalizes the probabilities associated with an array of objects, 
        /// then converts them into continuing sums.  
        /// This prepares them for being usable in pickFromDistribution.
        /// If the probabilities are all 0, then selection is uniform, unless allowAllZeros
        /// is false, in which case an ArithmeticException is thrown.  If any of them are negative,
        /// or if the distribution is empty, then an ArithmeticException is thrown.
        /// For example, 
        /// {0.6, 0.4, 0.2, 0.8} -> {0.3, 0.2, 0.1, 0.4} -> {0.3, 0.5, 0.6, 1.0} 
        /// The probabilities are retrieved and set using chooser.
        /// </summary>        
        public static void  OrganizeDistribution(Object[] objs, IRandomChoiceChooserD chooser, bool allowAllZeros)
        {
            // first normalize
            var sum = 0.0;
            
            if (objs.Length == 0)
                throw new ArithmeticException("Distribution has no elements");
            
            foreach (var t in objs)
            {
                if (chooser.GetProbability(t) < 0.0)
                    throw new ArithmeticException("Distribution has negative probabilities");
                sum += chooser.GetProbability(t);
            }
            
            if (sum == 0.0)
                if (!allowAllZeros)
                    throw new ArithmeticException("Distribution has all zero probabilities");
                else
                {
                    foreach (var t in objs)
                        chooser.SetProbability(t, 1.0);
                    sum = objs.Length;
                }

            foreach (var t in objs)
                chooser.SetProbability(t, chooser.GetProbability(t) / sum);

            // now sum
            sum = 0.0;
            foreach (var t in objs)
            {
                sum += chooser.GetProbability(t);
                chooser.SetProbability(t, sum);
            }
            
            // now we need to work backwards setting 0 values
            int x2;
            for (x2 = objs.Length - 1; x2 > 0; x2--)
                if (chooser.GetProbability(objs[x2]) == chooser.GetProbability(objs[x2 - 1]))
                // we're 0.0
                    chooser.SetProbability(objs[x2], 1.0);
                else
                    break;
            chooser.SetProbability(objs[x2], 1.0);
        }

        #endregion //  OrganizeDistribution
        #region PickFromDistribution

        /// <summary>
        /// Picks a random item from an array of probabilities,
        /// normalized and summed as follows:  For example,
        /// if four probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.length &lt; CHECKBOUNDARY,
        /// then a linear search is used, else a binary search is used. */
        /// </summary>
        public static int PickFromDistribution(float[] probabilities, float prob)
        {
            return PickFromDistribution(probabilities, prob, CHECKBOUNDARY);
        }

        /// <summary>
        /// Picks a random item from an array of probabilities,
        /// normalized and summed as follows:  For example,
        /// if four probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.length &lt; checkboundary,
        /// then a linear search is used, else a binary search is used. 
        /// </summary>
        public static int PickFromDistribution(float[] probabilities, float prob, int checkboundary)
        {
            if (prob < 0.0f || prob > 1.0f)
                throw new ArithmeticException("Invalid probability for pickFromDistribution (must be 0.0<=x<=1.0)");
            if (probabilities.Length == 1)
                return 0;
            if (probabilities.Length < checkboundary)
            {
                // simple linear scan
                for (var x = 0; x < probabilities.Length - 1; x++)
                    if (probabilities[x] > prob)
                        return ExemptZeroes(probabilities, x);
                return ExemptZeroes(probabilities, probabilities.Length - 1);
            }

            // binary search
            var top = probabilities.Length - 1;
            var bottom = 0;
            
            while (top != bottom)
            {
                var cur = (top + bottom) / 2; // integer division
                
                if (probabilities[cur] > prob)
                    if (cur == 0 || probabilities[cur - 1] <= prob)
                        return ExemptZeroes(probabilities, cur);
                    // step down
                    else
                        top = cur;
                else if (cur == probabilities.Length - 1)
                // oops
                    return ExemptZeroes(probabilities, cur);
                else if (bottom == cur)
                // step up
                    bottom++;
                // (8 + 9)/2 = 8
                else
                    bottom = cur; // (8 + 10) / 2 = 9
            }
            return ExemptZeroes(probabilities, bottom); // oops
        }
         
        /// <summary>
        /// Picks a random item from an array of probabilities,
        /// normalized and summed as follows:  For example,
        /// if four probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.length &lt; CHECKBOUNDARY,
        /// then a linear search is used, else a binary search is used.
        /// </summary>
        public static int PickFromDistribution(double[] probabilities, double prob)
        {
            return PickFromDistribution(probabilities, prob, CHECKBOUNDARY);
        }

        /// <summary>
        /// Picks a random item from an array of probabilities,
        /// normalized and summed as follows:  For example,
        /// if four probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.length &lt; checkboundary,
        /// then a linear search is used, else a binary search is used. 
        /// </summary>        
        public static int PickFromDistribution(double[] probabilities, double prob, int checkboundary)
        {
            if (prob < 0.0 || prob > 1.0)
                throw new ArithmeticException("Invalid probability for pickFromDistribution (must be 0.0<=x<=1.0)");
            if (probabilities.Length == 1)
                return 0;
            if (probabilities.Length < checkboundary)
            {
                // simple linear scan
                for (var x = 0; x < probabilities.Length - 1; x++)
                    if (probabilities[x] > prob)
                        return ExemptZeroes(probabilities, x);
                return ExemptZeroes(probabilities, probabilities.Length - 1);
            }

            // binary search
            var top = probabilities.Length - 1;
            var bottom = 0;
            
            while (top != bottom)
            {
                var cur = (top + bottom) / 2; // integer division
                
                if (probabilities[cur] > prob)
                    if (cur == 0 || probabilities[cur - 1] <= prob)
                        return ExemptZeroes(probabilities, cur);
                    // step down
                    else
                        top = cur;
                else if (cur == probabilities.Length - 1)
                // oops
                    return ExemptZeroes(probabilities, cur);
                else if (bottom == cur)
                // step up
                    bottom++;
                // (8 + 9)/2 = 8
                else
                    bottom = cur; // (8 + 10) / 2 = 9
            }
            return ExemptZeroes(probabilities, bottom); // oops
        }
      
        /// <summary>
        /// Picks a random item from an array of objects, each with an
        /// associated probability that is accessed by taking an object
        /// and passing it to chooser.getProbability(obj).  The objects'
        /// probabilities are 
        /// normalized and summed as follows:  For example,
        /// if four probabilities are {0.3, 0.2, 0.1, 0.4}, then
        /// they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.Length &lt; CHECKBOUNDARY,
        /// then a linear search is used, else a binary search is used.
        /// </summary>
        public static int PickFromDistribution(Object[] objs, IRandomChoiceChooser chooser, float prob)
        {
            return PickFromDistribution(objs, chooser, prob, CHECKBOUNDARY);
        }

        /// <summary>
        /// Picks a random item from an array of objects, each with an
        /// associated probability that is accessed by taking an object
        /// and passing it to chooser.GetProbability(obj).  The objects'
        /// probabilities are normalized and summed as follows:  
        /// For example, if four probabilities are {0.3, 0.2, 0.1, 0.4}, 
        /// then they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.Length &lt; checkboundary,
        /// then a linear search is used, else a binary search is used. 
        /// </summary>       
        public static int PickFromDistribution(Object[] objs, IRandomChoiceChooser chooser, float prob, int checkboundary)
        {
            if (prob < 0.0f || prob > 1.0f)
                throw new ArithmeticException("Invalid probability for PickFromDistribution (must be 0.0<=x<=1.0)");
            if (objs.Length == 1)
                return 0;
            if (objs.Length < checkboundary)
            {
                // simple linear scan
                for (var x = 0; x < objs.Length - 1; x++)
                    if (chooser.GetProbability(objs[x]) > prob)
                        return ExemptZeroes(objs, chooser, x);
                return ExemptZeroes(objs, chooser, objs.Length - 1);
            }

            // binary search
            var top = objs.Length - 1;
            var bottom = 0;
            
            while (top != bottom)
            {
                var cur = (top + bottom) / 2; // integer division
                
                if (chooser.GetProbability(objs[cur]) > prob)
                    if (cur == 0 || chooser.GetProbability(objs[cur - 1]) <= prob)
                        return ExemptZeroes(objs, chooser, cur);
                    // step down
                    else
                        top = cur;
                else if (cur == objs.Length - 1)
                // oops
                    return ExemptZeroes(objs, chooser, cur);
                else if (bottom == cur)
                // step up
                    bottom++;
                // (8 + 9)/2 = 8
                else
                    bottom = cur; // (8 + 10) / 2 = 9
            }
            return ExemptZeroes(objs, chooser, bottom); // oops
        }
        
        /// <summary>
        /// Picks a random item from an array of objects, each with an
        /// associated probability that is accessed by taking an object
        /// and passing it to chooser.getProbability(obj).  The objects'
        /// probabilities are  normalized and summed as follows:  
        /// For example, if four probabilities are {0.3, 0.2, 0.1, 0.4}, 
        /// then they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.Length &lt; CHECKBOUNDARY,
        /// then a linear search is used, else a binary search is used.
        /// </summary>
        public static int PickFromDistribution(Object[] objs, IRandomChoiceChooserD chooser, double prob)
        {
            return PickFromDistribution(objs, chooser, prob, CHECKBOUNDARY);
        }

        /// <summary>
        /// Picks a random item from an array of objects, each with an
        /// associated probability that is accessed by taking an object
        /// and passing it to chooser.GetProbability(obj).  The objects'
        /// probabilities are  normalized and summed as follows:  
        /// For example, if four probabilities are {0.3, 0.2, 0.1, 0.4}, 
        /// then they should get normalized and summed by the outside owners
        /// as: {0.3, 0.5, 0.6, 1.0}.  If probabilities.Length &lt; checkboundary,
        /// then a linear search is used, else a binary search is used. 
        /// </summary>        
        public static int PickFromDistribution(Object[] objs, IRandomChoiceChooserD chooser, double prob, int checkboundary)
        {
            if (prob < 0.0 || prob > 1.0)
                throw new ArithmeticException("Invalid probability for pickFromDistribution (must be 0.0<=x<=1.0)");
            if (objs.Length == 1)
                return 0;
            if (objs.Length < checkboundary)
            {
                // simple linear scan
                for (var x = 0; x < objs.Length - 1; x++)
                    if (chooser.GetProbability(objs[x]) > prob)
                        return ExemptZeroes(objs, chooser, x);
                return ExemptZeroes(objs, chooser, objs.Length - 1);
            }

            // binary search
            var top = objs.Length - 1;
            var bottom = 0;
            
            while (top != bottom)
            {
                var cur = (top + bottom) / 2; // integer division
                
                if (chooser.GetProbability(objs[cur]) > prob)
                    if (cur == 0 || chooser.GetProbability(objs[cur - 1]) <= prob)
                        return ExemptZeroes(objs, chooser, cur);
                    // step down
                    else
                        top = cur;
                else if (cur == objs.Length - 1)
                // oops
                    return ExemptZeroes(objs, chooser, cur);
                else if (bottom == cur)
                // step up
                    bottom++;
                // (8 + 9)/2 = 8
                else
                    bottom = cur; // (8 + 10) / 2 = 9
            }
            return ExemptZeroes(objs, chooser, bottom); // oops
        }

        #endregion // PickFromDistribution
    }
}