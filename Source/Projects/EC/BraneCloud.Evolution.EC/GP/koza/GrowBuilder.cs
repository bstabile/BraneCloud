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

namespace BraneCloud.Evolution.EC.GP.Koza
{
    /// <summary>
    /// GrowBuilder is a GPNodeBuilder which implements the GROW tree building method described in Koza I/II.  
    /// 
    /// <p/>GROW works by choosing a random integer <i>d</i> between minDepth and maxDepth, inclusive.  
    /// It then grows a tree of depth 1 to <i>d</i> inclusive.  Unlike lil-gp and Koza's texts, 
    /// ECJ defines the "depth" of a tree to be the number of <i>nodes</i> (not edges) 
    /// in the longest path from the root to any node in the tree.
    /// <p/>Actually, claiming to implement the Koza I/II approach is a bit of a fib -- Koza's original code is somewhat ad-hoc.  
    /// In the Koza approach, <i>d</i> is chosen in a kind of round-robin fashion rather than at random, 
    /// if RAMPED HALF/HALF is used.  Also, for all three algorithms (RAMPED HALF/HALF, GROW, FULL), 
    /// the algorithm will not generate a tree consisting of a single terminal, unless forced to.
    /// <p/>This implementation instead follows lil-gp's approach, which is to choose <i>d</i> at random 
    /// from between minDepth and maxDepth, inclusive, and to allow trees consisting of single terminals.   
    /// <p/>Determining what various algorithms do is a little confusing, mostly because the source code 
    /// for lil-gp and Koza don't actually quite do what they claim.  The table below lists the depth values 
    /// actually used (counting nodes, rather than edges, for depth).  It's probably not what you had expected!
    /// <br/>
    /// <br/>
    /// <div align="center">
    /// <table border="0" cellspacing="1" cellpadding="2">
    /// <tr>
    /// <td bgcolor="#ffffff"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" ></font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >Koza I Min</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >Koza I Max</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >Koza II Min</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >Koza II Max</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >lil-gp Min</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >lil-gp Max</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >ECJ Min</font><br/></td>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff" >ECJ Max</font><br/></td>
    /// </tr><tr>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff">GROW (mut)</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">&nbsp;</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">&nbsp;</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <tr></tr>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff">GROW (new)</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6? 7?</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6? 7?</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">3</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5</font><br/></td>
    /// <tr></tr>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff">FULL (new)</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6? 7?</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6? 7?</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">3</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">&nbsp;</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">&nbsp;</font><br/></td>
    /// <tr></tr>
    /// <td bgcolor="#3366cc"><font size="-1" face="simple,geneva,arial,helvetica" color="#ffffff">HALF (new)</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">2</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">2</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">5? 6?</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">3</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">7</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">2</font><br/></td>
    /// <td bgcolor="#cccccc"><font size="-1" face="simple,geneva,arial,helvetica">6</font><br/></td>
    /// </tr></table>
    /// </div>
    /// <br/>
    /// <br/>
    /// The table cell is empty when that parameter is not defined by the system by default.  
    /// Koza II has two values each because of a possible typo in the text -- while page 656 gives one maximum, page 671 gives another.  
    /// Note the odd fact that in Koza I/II GROW and FULL have <i>effectively</i> one-deeper tree values than HALF does, 
    /// even though they use the same code parameters!  This is because of a quirk in Koza's code.
    /// <p/> This algorithm ignores <tt>requestedSize</tt>, so no pipelines can ask it to grow a tree of a specific fixed size.  
    /// The algorithm also ignores any user-provided size distributions.
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base</i>.<tt>min-depth</tt><br/>
    /// <font size="-1">int &gt;= 1</font></td>
    /// <td valign="top">(smallest "maximum" depth the builder may use for building a tree. 2 is the default.)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base</i>.<tt>max-depth</tt><br/>
    /// <font size="-1">int &gt;= <i>base</i>.<tt>min-depth</tt></font></td>
    /// <td valign="top">(largest "maximum" depth thie builder may use for building a tree. 6 is the default.)</td></tr>
    /// </table>
    /// <p/><b>Default Base</b><br/>
    /// gp.koza.grow
    /// </summary>		
    [Serializable]
    [ECConfiguration("ec.gp.koza.GrowBuilder")]
    public class GrowBuilder : KozaBuilder
    {
        #region Constants

        public const string P_GROWBUILDER = "grow";

        #endregion // Constants
        #region Properties

        public override IParameter DefaultBase
        {
            get { return GPKozaDefaults.ParamBase.Push(P_GROWBUILDER); }
        }

        #endregion // Properties
        #region Operations

        public override GPNode NewRootedTree(IEvolutionState state, GPType type, int thread, IGPNodeParent parent,
                                                            GPFunctionSet funcs, int argPosition, int requestedSize)
        {
            var n = GrowNode(state, 0, state.Random[thread].NextInt(MaxDepth - MinDepth + 1) + MinDepth, type, thread, parent, argPosition, funcs);
            return n;
        }

        #endregion // Operations
    }
}