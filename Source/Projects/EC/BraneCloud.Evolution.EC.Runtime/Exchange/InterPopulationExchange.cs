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

using BraneCloud.Evolution.EC.Select;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Runtime.Exchange
{
    /// <summary> 
    /// InterPopulationExchange is an Exchanger which implements a simple exchanger
    /// between subpops. IterPopulationExchange uses an arbitrary graph topology
    /// for migrating individuals from subpops. The assumption is that all
    /// subpops have the same representation and same task to solve, otherwise
    /// the exchange between subpops does not make much sense.
    /// 
    /// <p/>InterPopulationExchange has a topology which is similar to the one used by
    /// IslandExchange.  Every few generations, a subpop will send some number
    /// of individuals to other subpops.  Since all subpops evolve at
    /// the same generational speed, this is a synchronous procedure (IslandExchange
    /// instead is asynchronous by default, though you can change it to synchronous).
    /// 
    /// <p/>Individuals are sent from a subpop prior to breeding.  They are stored
    /// in a waiting area until after all subpops have bred; thereafter they are
    /// added into the new subpop.  This means that the subpop order doesn't
    /// matter.  Also note that it means that some individuals will be created during breeding,
    /// and immediately killed to make way for the migrants.  A little wasteful, we know,
    /// but it's simpler that way.
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><tt><i>base</i>.chatty</tt><br/>
    /// <font size="-1">boolean, default = true</font></td>
    /// <td valign="top"> Should we be verbose or silent about our exchanges?
    /// </td></tr>
    /// </table>
    /// <p/><i>Note:</i> For each subpop in your population, there <b>must</b> be 
    /// one exch.subpop... declaration set.
    /// <table>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.select</tt><br/>
    /// <font size="-1">classname, inherits and != ec.SelectionMethod</font></td>
    /// <td valign="top"> The selection method used by subpop #n for picking 
    /// migrants to emigrate to other subpops
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.select-to-die</tt><br/>
    /// <font size="-1">classname, inherits and != ec.SelectionMethod (Default is random selection)</font></td>
    /// <td valign="top"> The selection method used by subpop #n for picking 
    /// individuals to be replaced by migrants
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.mod</tt><br/>
    /// <font size="-1">int >= 1</font></td>
    /// <td valign="top"> The number of generations that subpop #n waits between 
    /// sending emigrants
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.start</tt><br/>
    /// <font size="-1">int >= 0</font></td>
    /// <td valign="top"> The generation when subpop #n begins sending emigrants
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.Size</tt><br/>
    /// <font size="-1">int >= 0</font></td>
    /// <td valign="top"> The number of emigrants sent at one time by generation #n
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.num-dest</tt><br/>
    /// <font size="-1">int >= 0</font></td>
    /// <td valign="top"> The number of destination subpops for this subpop.
    /// </td></tr>
    /// <tr><td valign="top"><tt><i>base</i>.subpop.<i>n</i>.dest.<i>m</i></tt><br/>
    /// <font size="-1">int >= 0</font></td>
    /// <td valign="top"> Subpopulation #n's destination #m is this subpop.
    /// </td></tr>
    /// </table>
    /// 
    /// <p/><b>Parameter bases</b><br/>
    /// <table>
    /// <tr><td valign="top"/><tt><i>base</i>.subpop.<i>n</i>.select</tt><br/>
    /// <td>selection method for subpop #n's migrants</td></tr>
    /// </table>
    /// </summary>	
    [Serializable]
    [ECConfiguration("ec.exchange.InterPopulationExchange")]
    public class InterPopulationExchange : Exchanger
    {
        #region Constants

        /// <summary>
        /// The subpop delimiter. 
        /// </summary>
        public const string P_SUBPOP = "subpop";

        /// <summary>
        /// The parameter for the modulo (how many generations should pass between consecutive sendings of individuals. 
        /// </summary>
        public const string P_MODULO = "mod";

        /// <summary>
        /// The number of emigrants to be sent. 
        /// </summary>
        public const string P_SIZE = "size";

        /// <summary>
        /// How many generations to pass at the beginning of the evolution 
        /// before the first emigration from the current subpop. 
        /// </summary>
        public const string P_OFFSET = "start";

        /// <summary>
        /// The number of destinations from current island. 
        /// </summary>
        public const string P_DEST_FOR_SUBPOP = "num-dest";

        /// <summary>
        /// The prefix for destinations. 
        /// </summary>
        public const string P_DEST = "dest";

        /// <summary>
        /// The selection method for sending individuals to other islands. 
        /// </summary>
        public const string P_SELECT_METHOD = "select";

        /// <summary>
        /// The selection method for deciding individuals to be replaced by immigrants. 
        /// </summary>
        public const string P_SELECT_TO_DIE_METHOD = "select-to-die";

        /// <summary>
        /// Whether or not we're chatty. 
        /// </summary>
        public const string P_CHATTY = "chatty";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// My parameter base -- I need to keep this in order to help the server reinitialize contacts 
        /// </summary>
        public IParameter ParamBase;

        /// <summary>
        /// The number of immigrants in the storage for each of the subpops.
        /// </summary>
        public int[] NumImmigrants { get; set; }

        /// <summary>
        /// Storage for the incoming immigrants: 2 sizes:
        /// the subpop and the index of the emigrant.
        /// This is virtually the array of mailboxes.
        /// </summary>
        public Individual[][] Immigrants { get; set; }

        public bool Chatty { get; set; }

        /// <summary>
        /// BRS: Not sure what this is?
        /// </summary>
        public int NRSources { get; set; }

        public IPEInformation[] ExchangeInformation { get; set; }

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Sets up the Island Exchanger.
        /// </summary>
        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            ParamBase = paramBase;

            var p_numsubpops = new Parameter(Initializer.P_POP).Push(Population.P_SIZE);
            var numsubpops = state.Parameters.GetInt(p_numsubpops, null, 1);
            if (numsubpops == 0)
            {
                // later on, Population will complain with this fatally, so don't
                // exit here, just deal with it and assume that you'll soon be shut
                // down
            }

            // how many individuals (maximally) would each of the mailboxes have to hold
            var incoming = new int[numsubpops];

            // allocate some of the arrays
            ExchangeInformation = new IPEInformation[numsubpops];
            for (var i = 0; i < numsubpops; i++)
                ExchangeInformation[i] = new IPEInformation();
            NumImmigrants = new int[numsubpops];

            IParameter p;

            var localBase = ParamBase.Push(P_SUBPOP);

            Chatty = state.Parameters.GetBoolean(ParamBase.Push(P_CHATTY), null, true);

            for (var i = 0; i < numsubpops; i++)
            {

                // update the parameter for the new context
                p = localBase.Push("" + i);

                // read the selection method
                ExchangeInformation[i].ImmigrantsSelectionMethod = (SelectionMethod)
                    state.Parameters.GetInstanceForParameter(p.Push(P_SELECT_METHOD), paramBase.Push(P_SELECT_METHOD), typeof(SelectionMethod));
                if (ExchangeInformation[i].ImmigrantsSelectionMethod == null)
                    state.Output.Fatal("Invalid parameter.", p.Push(P_SELECT_METHOD), paramBase.Push(P_SELECT_METHOD));
                ExchangeInformation[i].ImmigrantsSelectionMethod.Setup(state, p.Push(P_SELECT_METHOD));

                // read the selection method
                if (state.Parameters.ParameterExists(p.Push(P_SELECT_TO_DIE_METHOD), paramBase.Push(P_SELECT_TO_DIE_METHOD)))
                    ExchangeInformation[i].IndsToDieSelectionMethod = (SelectionMethod)
                        state.Parameters.GetInstanceForParameter(p.Push(P_SELECT_TO_DIE_METHOD), paramBase.Push(P_SELECT_TO_DIE_METHOD), typeof(SelectionMethod));
                // use RandomSelection
                else
                    ExchangeInformation[i].IndsToDieSelectionMethod = new RandomSelection();
                ExchangeInformation[i].IndsToDieSelectionMethod.Setup(state, p.Push(P_SELECT_TO_DIE_METHOD));

                // get the modulo
                ExchangeInformation[i].Modulo = state.Parameters.GetInt(p.Push(P_MODULO), paramBase.Push(P_MODULO), 1);
                if (ExchangeInformation[i].Modulo == 0)
                    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p.Push(P_MODULO), paramBase.Push(P_MODULO));

                // get the offset
                ExchangeInformation[i].Offset = state.Parameters.GetInt(p.Push(P_OFFSET), paramBase.Push(P_OFFSET), 0);
                if (ExchangeInformation[i].Offset == -1)
                    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p.Push(P_OFFSET), paramBase.Push(P_OFFSET));

                // get the size
                ExchangeInformation[i].Size = state.Parameters.GetInt(p.Push(P_SIZE), paramBase.Push(P_SIZE), 1);
                if (ExchangeInformation[i].Size == 0)
                    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p.Push(P_SIZE), paramBase.Push(P_SIZE));

                // get the number of destinations
                ExchangeInformation[i].NumDest = state.Parameters.GetInt(p.Push(P_DEST_FOR_SUBPOP), null, 0);
                if (ExchangeInformation[i].NumDest == -1)
                    state.Output.Fatal("Parameter not found, or it has an incorrect value.", p.Push(P_DEST_FOR_SUBPOP));

                ExchangeInformation[i].Destinations = new int[ExchangeInformation[i].NumDest];
                // read the destinations
                for (var j = 0; j < ExchangeInformation[i].NumDest; j++)
                {
                    ExchangeInformation[i].Destinations[j] = state.Parameters.GetInt(p.Push(P_DEST).Push("" + j), null, 0);
                    if (ExchangeInformation[i].Destinations[j] == -1 || ExchangeInformation[i].Destinations[j] >= numsubpops)
                        state.Output.Fatal("Parameter not found, or it has an incorrect value.", p.Push(P_DEST).Push("" + j));
                    // update the maximum number of incoming individuals for the destination island
                    incoming[ExchangeInformation[i].Destinations[j]] += ExchangeInformation[i].Size;
                }
            }

            // calculate the maximum number of incoming individuals to be stored in the mailbox
            var max = -1;

            foreach (var t in incoming)
                if (max == -1 || max < t)
                    max = t;

            // set up the mailboxes
            Immigrants = new Individual[numsubpops][];
            for (var i2 = 0; i2 < numsubpops; i2++)
            {
                Immigrants[i2] = new Individual[max];
            }
        }

        #endregion // Setup
        #region Operations

        public override Population PreBreedingExchangePopulation(IEvolutionState state)
        {
            // exchange individuals between subpops
            // BUT ONLY if the modulo and offset are appropriate for this
            // generation (state.Generation)
            // I am responsible for returning a population.  This could
            // be a new population that I created fresh, or I could modify
            // the existing population and return that.
            
            // for each of the islands that sends individuals
            for (var i = 0; i < ExchangeInformation.Length; i++)
            {
                
                // else, check whether the emigrants need to be sent
                if ((state.Generation >= ExchangeInformation[i].Offset) 
                            && ((ExchangeInformation[i].Modulo == 0) 
                            || (((state.Generation - ExchangeInformation[i].Offset) % ExchangeInformation[i].Modulo) == 0)))
                {
                    
                    // send the individuals!!!!
                    
                    // for each of the islands where we have to send individuals
                    for (var x = 0; x < ExchangeInformation[i].NumDest; x++)
                    {
                        
                        if (Chatty)
                            state.Output.Message("Sending the emigrants from subpop " + i + " to subpop " + ExchangeInformation[i].Destinations[x]);
                        
                        // select "size" individuals and send then to the destination as emigrants
                        ExchangeInformation[i].ImmigrantsSelectionMethod.PrepareToProduce(state, i, 0);

                        for (var y = 0; y < ExchangeInformation[i].Size; y++)
                        // send all necesary individuals
                        {
                            // get the index of the immigrant
                            var index = ExchangeInformation[i].ImmigrantsSelectionMethod.Produce(i, state, 0);
                            // copy the individual to the mailbox of the destination subpop
                            Immigrants[ExchangeInformation[i].Destinations[x]][NumImmigrants[ExchangeInformation[i].Destinations[x]]] = state.Population.Subpops[i].Individuals[index];
                            // increment the counter with the number of individuals in the mailbox
                            NumImmigrants[ExchangeInformation[i].Destinations[x]]++;
                        }
                        ExchangeInformation[i].ImmigrantsSelectionMethod.FinishProducing(state, i, 0); // end the selection step
                    }
                }
            }
            
            return state.Population;
        }
                
        public override Population PostBreedingExchangePopulation(IEvolutionState state)
        {
            // receiving individuals from other islands
            // same situation here of course.
            
            for (var x = 0; x < NumImmigrants.Length; x++)
            {
                
                if (NumImmigrants[x] > 0 && Chatty)
                {
                    state.Output.Message("Immigrating " + NumImmigrants[x] + " individuals from mailbox for subpop " + x);
                }
                
                var len = state.Population.Subpops[x].Individuals.Length;
                // double check that we won't go into an infinite loop!
                if (NumImmigrants[x] >= state.Population.Subpops[x].Individuals.Length)
                    state.Output.Fatal("Number of immigrants (" + NumImmigrants[x] + ") is larger than subpop #" + x + "'s size (" + len + ").  This would cause an infinite loop in the selection-to-die procedure.");
                
                var selected = new bool[len];
                var indeces = new int[NumImmigrants[x]];
                for (var i = 0; i < selected.Length; i++)
                    selected[i] = false;

                ExchangeInformation[x].IndsToDieSelectionMethod.PrepareToProduce(state, x, 0);
                for (var i = 0; i < NumImmigrants[x]; i++)
                {
                    do 
                    {
                        indeces[i] = ExchangeInformation[x].IndsToDieSelectionMethod.Produce(x, state, 0);
                    }
                    while (selected[indeces[i]]);
                    selected[indeces[i]] = true;
                }
                ExchangeInformation[x].IndsToDieSelectionMethod.FinishProducing(state, x, 0);
                
                for (var y = 0; y < NumImmigrants[x]; y++)
                {
                    
                    // read the individual
                    state.Population.Subpops[x].Individuals[indeces[y]] = Immigrants[x][y];
                    
                    // reset the evaluated flag (the individuals are not evaluated in the current island */
                    state.Population.Subpops[x].Individuals[indeces[y]].Evaluated = false;
                }
                
                // reset the number of immigrants in the mailbox for the current subpop
                // this doesn't need another synchronization, because the thread is already synchronized
                NumImmigrants[x] = 0;
            }
            
            return state.Population;
        }
        
        /// <summary>
        /// Called after PreBreedingExchangePopulation(...) to evaluate whether or not
        /// the exchanger wishes the run to shut down (with ec.EvolutionState.R_FAILURE).
        /// This would happen for two reasons.  First, another process might have found
        /// an ideal individual and the global run is now over.  Second, some network
        /// or operating system error may have occurred and the system needs to be shut
        /// down gracefully.
        /// This function does not return a String as soon as it wants to exit (another island found
        /// the perfect individual, or couldn't connect to the server). Instead, it sets a flag, called
        /// message, to remember next time to exit. This is due to a need for a graceful
        /// shutdown, where checkpoints are working properly and save all needed information. 
        /// </summary>
        public override string RunComplete(IEvolutionState state)
        {
            return null;
        }

        #region Contacts

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing.
        /// Called at the beginning of an evolutionary run, before a population is set up.
        /// It doesn't do anything, as this exchanger works on only 1 computer.
        /// </summary>
        public override void InitializeContacts(IEvolutionState state)
        {
        }

        /// <summary>
        /// Initializes contacts with other processes, if that's what you're doing.  
        /// Called after restarting from a checkpoint.
        /// It doesn't do anything, as this exchanger works on only 1 computer.
        /// </summary>
        public override void ReinitializeContacts(IEvolutionState state)
        {
        }

        /// <summary>
        /// Closes contacts with other processes, if that's what you're doing.  
        /// Called at the end of an evolutionary run. result is either 
        /// ec.EvolutionState.R_SUCCESS or ec.EvolutionState.R_FAILURE, 
        /// indicating whether or not an ideal individual was found. 
        /// </summary>
        public override void CloseContacts(IEvolutionState state, int result)
        {
        }

        #endregion // Contacts

        #endregion // Operations
    }
}