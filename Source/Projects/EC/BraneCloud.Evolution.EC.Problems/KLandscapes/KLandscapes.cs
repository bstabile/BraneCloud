
/**
 * KLandscapes implements the K-Landscapes problem of Vanneschi,
 * Castelli and Manzoni. See the README.txt.  
 *
 <p><b>Parameters</b><br>
 <table>
 <tr><td valign=top><i>base</i>.<tt>data</tt><br>
 <font size=-1>classname, inherits or == ec.app.klandscapes.KLandscapesData</font></td>
 <td valign=top>(the class for the prototypical GPData object for the KLandscapes problem)</td></tr>
 </table>

 <tr><td valign=top><i>base</i>.<tt>k-value</tt><br>
 <font size=-1>Integer specifying the amount of epistasis in fitness contributions</font></td>
 <td valign=top>Values from 0 upwards.</td></tr>
 </table>

 <p><b>Parameter bases</b><br>
 <table>
 <tr><td valign=top><i>base</i>.<tt>data</tt></td>
 <td>species (the GPData object)</td></tr>
 </table>
 *
 * @author Luca Manzoni
 * @version 1.0
 */


using System;
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.GP;
using BraneCloud.Evolution.EC.Problems.KLandscapes.Func;
using BraneCloud.Evolution.EC.Randomization;
using BraneCloud.Evolution.EC.Simple;
using BraneCloud.Evolution.EC.Util;


namespace BraneCloud.Evolution.EC.Problems.KLandscapes
{

    [ECConfiguration("ec.problems.klandscapes.KLandscapes")]
    public class KLandscapes : GPProblem, ISimpleProblem
    {
        string P_PROBLEMNAME = "k-landscapes";
        string P_KVALUE = "k-value";


        // Score of the nodes. Functionals (positions 0 and 1) and terminals (positions from 2 to 5)
        double[] _nodeScore;

        // Score fo the edges. Row: functionals. Columns: funcionals + terminals
        double[][] _edgeScore;

        // Best possible fitness. Must be not negative.
        double _bestFitness;

        // The K of the K-Landscapes. It is an index of epistasis. It has
        // integer values from 2 upwards, in the paper's experimental
        // section, but can take on values 0 and 1.
        int k;

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);
            state.Output.ExitIfErrors();
            IParameter kval = new Parameter(EvolutionState.P_EVALUATOR).Push(P_PROBLEM).Push(P_PROBLEMNAME)
                .Push(P_KVALUE);
            k = state.Parameters.GetInt(kval, null, 0);
            // System.out.println("K = " + k);

            for (int i = 0; i < _indices.Length; i++)
                _indices[i] = -1;
            _indices['A' - 'A'] = 0;
            _indices['B' - 'A'] = 1;
            _indices['X' - 'A'] = 2;
            _indices['Y' - 'A'] = 3;
            _indices['Z' - 'A'] = 4;
            _indices['W' - 'A'] = 5;


            // now do some initialization
            IMersenneTwister r = state.Random[0];
            _nodeScore = new double[6];
            _edgeScore = TensorFactory.Create<double>(2, 6);
            for (int i = 0; i < 6; i++)
            {
                _nodeScore[i] = 2 * r.NextDouble() - 1;
            }
            // We need to assure that the best fitness is positive (to normalize it to 1)
            // A method to do this is to have at least one terminal symbol with a positive score.
            bool ok = false;
            for (int i = 2; i < 6; i++)
            {
                if (_nodeScore[i] > 0)
                    ok = true;
            }
            if (!ok)
                _nodeScore[2] = r.NextDouble();
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    _edgeScore[i][j] = r.NextDouble();
                }
            }
            _bestFitness = ComputeBestFitness();


        }

        // doesn't need to be cloned
        readonly int[] _indices = new int[256]; // we assume we only have letters, and 0 means "no sucessor"

        int GetIndex(char c)
        {
            return _indices[c - 'A'];
        }

        public void Evaluate(
            IEvolutionState state,
            Individual ind,
            int subpopulation,
            int threadnum)
        {
            if (!ind.Evaluated)
            {
                double score = Fitness(((GPIndividual) ind).Trees[0].Child);
                SimpleFitness f = (SimpleFitness) ind.Fitness;
                f.SetFitness(state, (float) score, score == 1.0);
                ind.Evaluated = true;
            }
        }

        double Fitness(GPNode root)
        {
            // Compute the penality (it increases with the difference in depth between the tree and k.
            double penalty = 1 / (1 + Math.Abs(k + 1 - root.Depth));
            return penalty * FitnessHelper(root) / _bestFitness;
        }

        // We recursively search for the subtree with the maximal "score" 
        double FitnessHelper(GPNode node)
        {
            double max = SubtreeFitness(node, k);
            for (int i = 0; i < node.Children.Length; i++)
            {
                GPNode child = node.Children[i];
                double tmp = FitnessHelper(child);
                if (tmp > max)
                    max = tmp;
            }
            return max;
        }

        double SubtreeFitness(GPNode node, int depth)
        {
            int index = GetIndex(((KLandscapeTree) node).Value);
            double score = _nodeScore[index];
            if (depth == 0 || index > 1) //If we have reached the maximum depth (or we have found a terminal)
                return score;
            for (int i = 0; i < node.Children.Length; i++)
            {
                GPNode child = node.Children[i];
                int childindex = GetIndex(((KLandscapeTree) child).Value);
                //We recursively compute the "score" of the subtree
                score += (1 + _edgeScore[index][childindex]) * SubtreeFitness(child, depth - 1);
            }
            return score;
        }

        double ComputeBestFitness()
        {
            // This is a dynamic programming kludge.
            double[][] ttable = TensorFactory.Create<double>(k, 2);
            double[][] ftable = TensorFactory.Create<double>(k + 1, 2);
            for (int i = 0; i < 2; i++)
            {
                ftable[0][i] = _nodeScore[i];
            }
            // Case 1: the optimum hase depth at most k
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (i == 0)
                    {
                        double max = (1 + _edgeScore[j][2]) * _nodeScore[2];
                        for (int h = 3; h < 6; h++)
                        {
                            double tmp = (1 + _edgeScore[j][h]) * _nodeScore[h];
                            if (tmp > max)
                                max = tmp;
                        }
                        ttable[i][j] = _nodeScore[j] + 2 * max;
                    }
                    else
                    {
                        double max = (1 + _edgeScore[j][0]) * ttable[i - 1][0];
                        for (int h = 1; h < 2; h++)
                        {
                            double tmp = (1 + _edgeScore[j][h]) * ttable[i - 1][h];
                            if (tmp > max)
                                max = tmp;
                        }
                        ttable[i][j] = _nodeScore[j] + 2 * max;
                    }
                }
            }
            // Case 2: the optimum has depth k+1
            for (int i = 1; i < k + 1; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    double max = (1 + _edgeScore[j][0]) * ftable[i - 1][0];
                    for (int h = 1; h < 2; h++)
                    {
                        double tmp = (1 + _edgeScore[j][h]) * ftable[i - 1][h];
                        if (tmp > max)
                            max = tmp;
                    }
                    ftable[i][j] = _nodeScore[j] + 2 * max;
                }
            }
            double best = _nodeScore[2];
            for (int i = 3; i < 6; i++)
            {
                if (_nodeScore[i] > best)
                    best = _nodeScore[i];
            }
            for (int i = 0; i < k; i++)
            {
                for (int j = 0; j < 2; j++)
                {
                    if (ttable[i][j] > best)
                        best = ttable[i][j];
                }
            }
            for (int i = 0; i < 2; i++)
            {
                if (0.5 * ftable[k][i] > best)
                    best = 0.5 * ftable[k][i];
            }
            return best;
        }

        public override Object Clone()
        {
            KLandscapes tmp = (KLandscapes) base.Clone();
            tmp._nodeScore = new double[6];
            tmp._edgeScore = TensorFactory.Create<double>(2, 6);
            tmp._bestFitness = _bestFitness;
            tmp.k = k;
            for (int i = 0; i < 6; i++)
            {
                tmp._nodeScore[i] = _nodeScore[i];
                for (int j = 0; j < 2; j++)
                {
                    tmp._edgeScore[j][i] = _edgeScore[j][i];
                }
            }
            return tmp;
        }

    }
}