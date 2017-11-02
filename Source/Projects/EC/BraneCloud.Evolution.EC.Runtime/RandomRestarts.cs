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
using BraneCloud.Evolution.EC.Configuration;
using BraneCloud.Evolution.EC.SteadyState;

namespace BraneCloud.Evolution.EC.Runtime
{
    /// <summary>
    /// A special Statistics class which performs random restarts on the population,
    /// effectively reininitializing the population and starting over again.
    /// RandomRestarts has two ways of determining when to perform a restart.  If
    /// the restart type is "fixed", then the restart will occur precisely when
    /// the generation is a multiple of restart-upper-bound, minus one.  (That's
    /// hardly random, of course).  If the restart type is "random", then at the
    /// beginning of the run, and after every restart, a new restart is chosen 
    /// randomly from one to restart-upper-bound.
    /// 
    /// <p/>This class is compatible with populations which load from files -- it
    /// temporarily disables the load-from-file feature when telling the population
    /// to populate itself again, forcing the population to do so by creating random
    /// individuals.
    /// 
    /// @author James O'Beirne
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top">< i > base </i>.<tt> start </tt><br/>
    /// <font size="-1">int &gt;= 0 (default = 1)</font></td>
    /// <td valign="top"> The first generation where the clock may be started.</td></tr>
    /// <tr><td valign="top"><i> base </i>.<tt> restart - type </tt><br/>
    /// <font size="-1">random (default) or fixed</font></td>
    /// <td valign="top">Either initiates clock at a random value or a fixed one.</td></tr>
    /// <tr><td valign="top"><i>base</i>.<tt>restart-upper-bound</tt><br/>
    /// <font size="-1">1 &lt; int &lt; \inf</font></td>
    /// <td valign="top">Maximum time clock can initiate with.</td></tr>
    /// </table>
    /// </summary>
    /// <remarks>
    /// We can't use the "evolve" namespace because of the existing type.
    /// However, we can still use it as part of the canonical name for configuration.
    /// </remarks>
    [Serializable]
    [ECConfiguration("ec.evolve.RandomRestarts")] 
    public class RandomRestarts : Statistics, ISteadyStateStatistics
    {
        #region Constants

        /// <summary>
        /// Two options available here: "fixed" and "random"; "fixed"
        /// will initate the restart timer at the value specified for
        /// <i>restart-upper-bound</i>, "random" will initiate the restart
        /// timer somewhere below the value specified for 
        /// <i>restart-upper-bound</i> 
        /// </summary>
        public const string P_RESTART_TYPE = "restart-type";

        /// <summary>
        /// This is the highest value at which the "ticking" restart clock can initiate at. 
        /// </summary>
        public const string P_RESTART_UPPERBOUND = "restart-upper-bound";

        public const string P_START = "start";

        #endregion // Constants
        #region Fields

        /// <summary>
        /// Are we doing random or fixed?
        /// </summary>
        string _restartType;

        #endregion // Fields
        #region Properties

        /// <summary>
        /// What we'll use for the "ticking" clock.
        /// </summary>
        public int Countdown { get; set; }

        /// <summary>
        /// Highest possible value on the clock
        /// </summary>
        public int Upperbound { get; set; }

        public int Start { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Gets the clock ticking.
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            base.Setup(state, paramBase);

            _restartType = state.Parameters.GetString(paramBase.Push(P_RESTART_TYPE), null);

            if (_restartType == null)
                _restartType = "random";

            Upperbound = state.Parameters.GetInt(paramBase.Push(P_RESTART_UPPERBOUND), null, 1);

            if (state.Parameters.ParameterExists(paramBase.Push(P_START), null))
            {
                Start = state.Parameters.GetInt(paramBase.Push(P_START), null, 0);
                if (Start < 0)
                    state.Output.Fatal("Start value must be >= 0", paramBase.Push(P_START));
            }
            else Start = 1;

            if (Upperbound < 1)
                state.Output.Fatal("Parameter either not found or invalid (<1).", paramBase.Push(P_RESTART_UPPERBOUND));

            if (!_restartType.Equals("random") && !_restartType.Equals("fixed"))
                state.Output.Fatal("Parameter must be either 'fixed' or 'random'.", paramBase.Push(P_RESTART_TYPE));

            // start counting down
            ResetClock(state);
        }

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Checks the clock; if it's time to restart, we repopulate the population. 
        /// Afterwards, we reset the clock. If it's not time yet, the clock goes tick.
        /// </summary>
        public override void PreEvaluationStatistics(IEvolutionState state)
        {
            base.PreEvaluationStatistics(state);
            if (state.Generation == Start) ResetClock(state); // first time only
            if (state.Generation >= Start) PossiblyRestart(state);
        }

        void PossiblyRestart(IEvolutionState state)
        {
            Countdown--;
            Subpopulation currentSubp = null;

            // time to restart!
            if (Countdown == 0)
            {
                state.Output.Message("Restarting the population prior to evaluating generation " + state.Generation);
                // for each subpopulation
                foreach (var t in state.Population.Subpops)
                {
                    currentSubp = t;
                    bool temp = currentSubp.LoadInds;
                    // disable LoadInds so we generate candidates randomly
                    currentSubp.LoadInds = false;
                    currentSubp.Populate(state, 0);
                    currentSubp.LoadInds = temp;
                }
                ResetClock(state);
            }
        }

        void ResetClock(IEvolutionState state)
        {
            if (_restartType.Equals("fixed"))
                Countdown = Upperbound;
            else
                // might need to fix random index to support multithreading
                Countdown = state.Random[0].NextInt(Upperbound) + 1;
        }

        #endregion // Operations
    }
}