
using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.Randomization;

namespace BraneCloud.Evolution.EC.App.Majority
{
    /**
           CA.cs
                
           Implements a 1-dimensional toroidal CA for purposes of doing the binary majority classification problem.
           You need to supply the automaton size and the rule neighborhood size at constructor time.
           The CA itself consists of an array of integers, each 0 or 1.   You can clear this array, set it to
           preset values, or.Randomize it.
           You provide the rule as an array of ints, all 0 or 1 as well.  The rules are specified in the following
           order.  Let's say that you have a neighborhood of 3, consiting of cells LCR, where L is the cell "left"
           of the target cell (left is less than, right is greater than).  Then the order of the rules are for the
           neighborhoods 0: 000, 1: 001, 2: 010, 3: 011, 4: 100, 5: 101, 6: 110, 7: 111.   In other words, the
           neighborhood is interpreted as a binary number and that's the index into the rule.
    */


    [Serializable]
    [ECConfiguration("ec.app.majority.CA")]
    public class CA
    {
        private const long SerialVersionUID = 1;

        public CA(int width, int neighborhood)
        {
            _ca = new int[width];
            _ca2 = new int[width];
            _neighborhood = neighborhood;
            _rule = new int[1 << neighborhood];
        }

        int[] _ca;
        int[] _ca2;
        int[] _rule;
        readonly int _neighborhood;

        public int[] GetVals()
        {
            return _ca;
        }

        public int[] GetRule()
        {
            return _rule;
        }

        public void SetRule(int[] r)
        {
            if (r.Length != _rule.Length)
                throw new InvalidOperationException("Rule.Length invalid given neighborhood size.");
            _rule = r;
        }

        public void SetVals(int[] vals)
        {
            if (vals.Length != _ca.Length)
                throw new InvalidOperationException("CA.Length invalid given prespecified size.");
            Array.Copy(vals, _ca, _ca.Length);
        }

        public void Clear(bool toOnes)
        {
            if (toOnes)
                for (int i = 0; i < _ca.Length; i++)
                    _ca[i] = 1;
            else
                for (int i = 0; i < _ca.Length; i++)
                    _ca[i] = 0;
        }

        public bool Converged()
        {
            int t = _ca[0];
            for (int i = 1; i < _ca.Length; i++)
                if (_ca[i] != t) return false;
            return true;
        }

        public void Randomize(EvolutionState state, int thread)
        {
            IMersenneTwister random = state.Random[thread];
            for (int i = 0; i < _ca.Length; i++)
                _ca[i] = random.NextBoolean() ? 0 : 1;
        }

        public void Step(int steps, bool stopWhenConverged)
        {
            int len = _ca.Length;
            int halfhood = _neighborhood / 2; // this is the size of one side of the neighborhood
            int mask = (1 << _neighborhood) - 1; // this masks out the state to the neighborhod.Length

            for (int q = 0; q < steps; q++)
            {
                int state = 0; // the current neighborhood state.  Rotates through.

                // initialize state to right toroidal values
                for (int i = len - halfhood; i < len; i++)
                    state = (state << 1) | _ca[i];
                // initialize state to left values
                for (int i = 0; i < halfhood + 1; i++)
                    state = (state << 1) | _ca[i];

                // scan with current state
                for (int i = 0; i < (len - halfhood) - 1; i++)
                {
                    _ca2[i] = _rule[state];
                    state = ((state << 1) | _ca[i + halfhood + 1]) & mask;
                }

                // continue to scan toroidally
                int j = 0;
                for (int i = len - halfhood - 1; i < len; i++)
                {
                    _ca2[i] = _rule[state];
                    state = ((state << 1) | _ca[j++]) & mask;
                }

                // swap
                int[] tmp = _ca;
                _ca = _ca2;
                _ca2 = tmp;

                // did we converge?
                if (stopWhenConverged && Converged())
                {
                    //Console.Error.WriteLine("converged at " + q);
                    return;
                }
            }
        }
    }
}
