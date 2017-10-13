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

namespace BraneCloud.Evolution.EC.Vector
{
    /// <summary> 
    /// IntegerVectorSpecies is a subclass of VectorSpecies with special constraints
    /// for integral vectors, namely ByteVectorIndividual, ShortVectorIndividual,
    /// IntegerVectorIndividual, and LongVectorIndividual.
    /// </summary>
    /// <summary> <p/>IntegerVectorSpecies can specify min/max numeric constraints on gene values
    /// in three different ways.
    /// 
    /// <ol>
    /// <li/> You may provide a default min and max value.
    /// This is done by specifying:
    /// <p/><i>base.n</i>.<tt>min-gene</tt>
    /// <br/><i>base.n</i>.<tt>max-gene</tt>
    /// <p/><i>Note:</i> you <b>must</b> provide these values even if you don't use them,
    /// as they're used as defaults by #2 and #3 below.
    /// <p/>
    /// <li/> You may provide min and max values for genes in segments (regions) along
    /// the genome.  This is done by specifying:
    /// <p/><i>base</i>.<tt>num-segments</tt>
    /// The segments may be defined by either start or end indices of genes. 
    /// This is controlled by specifying the value of:
    /// <p/><i>base</i>.<tt>segment-type</tt>
    /// which can assume the value of start or end, with start being the default.
    /// The indices are defined using Java array style, i.e. the first gene has the index of 0, 
    /// and the last gene has the index of genome-size - 1.
    /// <p/>Using this method, each segment is specified by<i>j</i>...
    /// <p/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.start</tt>
    /// <br/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.min-gene</tt>
    /// <br/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.max-gene</tt>
    /// if segment-type value was chosen as start or by:
    /// <p/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.end</tt>
    /// <br/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.min-gene</tt>
    /// <br/><i>base</i>.<tt>segment.</tt><i>j</i><tt>.max-gene</tt>
    /// if segment-type value is equal to end.
    /// <p/>
    /// <li/> You may provide min and max values for each separate gene.
    /// This is done by specifying (for each gene location <i>i</i> you wish to specify).  
    /// <p/><i>base.n</i>.<tt>min-gene</tt>.<i>i</i>
    /// <br/><i>base.n</i>.<tt>max-gene</tt>.<i>i</i>
    /// </ol>
    /// 
    /// <p/>Any settings for #3 override #2, and both override #1. 
    /// 
    /// <p/><b>Parameters</b><br/>
    /// <table>
    /// <tr><td valign="top"><i>base.n</i>.<tt>min-gene</tt><br/>
    /// <font size="-1">long (default=0)</font></td>
    /// <td valign="top">(the minimum gene value)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.n</i>.<tt>max-gene</tt><br/>
    /// <font size="-1">long &gt;= <i>base.n</i>.min-gene</font></td>
    /// <td valign="top">(the maximum gene value)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.n</i>.<tt>min-gene</tt>.<i>i</i><br/>
    /// <font size="-1">long (default=<i>base.n</i>.<tt>min-gene</tt>)</font></td>
    /// <td valign="top">(the minimum gene value for gene <i>i</i>)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.n</i>.<tt>max-gene</tt>.<i>i</i><br/>
    /// <font size="-1">long &gt;= <i>base.n</i>.min-gene.<i>i</i> (default=<i>base.n</i>.<tt>max-gene</tt>)</font></td>
    /// <td valign="top">(the maximum gene value for gene <i>i</i>)</td></tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>num-segments</tt><br/>
    /// <font size="-1">int &gt;= 1 (default=no segments used)</font></td>
    /// <td valign="top">(the number of gene segments defined)</td>
    /// </tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>segment-type</tt><br/>
    /// <font size="-1">int &gt;= 1 (default=start)</font></td>
    /// <td valign="top">(defines the way in which segments are defined: either by providing start indices (segment-type=start) or by providing end indices (segment-type=end)</td>
    /// </tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>segment</tt>.<i>j</i>.<tt>start</tt><br/>
    /// <font size="-1">0 &lt;= int &lt; genome length</font></td>
    /// <td valign="top">(the start index of gene segment <i>j</i> -- the end of a segment is before the start of the next segment)</td>
    /// <td valign="top">(used when the value of segment-type parameter is equal to start)</td>
    /// </tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>segment</tt>.<i>j</i>.<tt>end</tt><br/>
    /// <font size="-1">0 &lt;= int &lt; genome length</font></td>
    /// <td valign="top">(the end of gene segment <i>j</i> -- the start of a segment is after the end of the previous segment)</td>
    /// <td valign="top">(used when the value of segment-type parameter is equal to end)</td>
    /// </tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>segment</tt>.<i>j</i>.<tt>min-gene</tt><br/>
    /// <font size="-1">double (default=0.0)</font></td>
    /// <td valign="top">(the minimum gene value for segment <i>j</i>)</td>
    /// </tr>
    /// 
    /// <tr><td valign="top"><i>base.</i>.<tt>segment</tt>.<i>j</i>.<tt>max-gene</tt><br/>
    /// <font size="-1"/>double &gt;= <i>base.</i>.<tt>segment</tt>.<i>j</i>.<tt>min-gene</tt></td>
    /// <td valign="top">(the maximum gene value for segment <i>j</i>)</td>
    /// </tr>
    /// </table>
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.vector.IntegerVectorSpecies")]
    public class IntegerVectorSpecies : VectorSpecies
    {
        #region Constants

        public const string P_MINGENE = "min-gene";
        public const string P_MAXGENE = "max-gene";
        
        public const string P_NUM_SEGMENTS = "num-segments";        
        public const string P_SEGMENT_TYPE = "segment-type";       
        public const string P_SEGMENT_START = "start";        
        public const string P_SEGMENT_END = "end";        
        public const string P_SEGMENT = "segment";

        #endregion // Constants
        #region Properties

        public long[] MinGenes;
        public long[] MaxGenes;

        #endregion // Properties
        #region Setup

        public override void Setup(IEvolutionState state, IParameter paramBase)
        {
            // keep in mind that the *species* variable has not been set yet.
            base.Setup(state, paramBase);

            var def = DefaultBase;


            // create the arrays
            MinGenes = new long[GenomeSize];
            MaxGenes = new long[GenomeSize];

            // LOADING GLOBAL MIN/MAX GENES
            var minGene = state.Parameters.GetLongWithDefault(paramBase.Push(P_MINGENE), def.Push(P_MINGENE), 0);
            var maxGene = state.Parameters.GetLong(paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE), minGene);
            if (maxGene < minGene)
                state.Output.Fatal("IntegerVectorSpecies must have a default min-gene which is <= the default max-gene",
                                                                        paramBase.Push(P_MAXGENE), def.Push(P_MAXGENE));

            for (var x = 0; x < GenomeSize; x++)
            {
                MinGenes[x] = minGene;
                MaxGenes[x] = maxGene;
            }

            // LOADING SEGMENTS

            // Now check to see if segments of genes (genes having the same min and max values) exist
            if (state.Parameters.ParameterExists(paramBase.Push(P_NUM_SEGMENTS), def.Push(P_NUM_SEGMENTS)))
            {
                if (DynamicInitialSize)
                    state.Output.WarnOnce("Using dynamic initial sizing, but per-segment min/max gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                        paramBase.Push(P_NUM_SEGMENTS), def.Push(P_NUM_SEGMENTS));

                var numSegments = state.Parameters.GetIntWithDefault(paramBase.Push(P_NUM_SEGMENTS), def.Push(P_NUM_SEGMENTS), 0);

                if (numSegments == 0)
                    state.Output.Warning("The number of genome segments has been defined to be equal to 0.\n"
                        + "Hence, no genome segments will be defined.", paramBase.Push(P_NUM_SEGMENTS),
                                                                              def.Push(P_NUM_SEGMENTS));
                else if (numSegments < 0)
                    state.Output.Fatal("Invalid number of genome segments: "
                        + numSegments + "\nIt must be a nonnegative value.",
                        paramBase.Push(P_NUM_SEGMENTS),
                              def.Push(P_NUM_SEGMENTS));

                //read the type of segment definition using the default start value
                var segmentType = state.Parameters.GetStringWithDefault(paramBase.Push(P_SEGMENT_TYPE),
                                                                def.Push(P_SEGMENT_TYPE), P_SEGMENT_START);

                if (segmentType.ToUpper().Equals(P_SEGMENT_START.ToUpper()))
                    InitializeGenomeSegmentsByStartIndices(state, paramBase, def, numSegments, minGene, maxGene);
                else if (segmentType.ToUpper().Equals(P_SEGMENT_END.ToUpper()))
                    InitializeGenomeSegmentsByEndIndices(state, paramBase, def, numSegments, minGene, maxGene);
                else
                    state.Output.Fatal("Invalid specification of genome segment type: " + segmentType
                        + "\nThe " + P_SEGMENT_TYPE + " parameter must have the value of "
                                   + P_SEGMENT_START + " or " + P_SEGMENT_END, paramBase.Push(P_SEGMENT_TYPE),
                                                                                     def.Push(P_SEGMENT_TYPE));
            }

            // LOADING PER-GENE VALUES

            var foundStuff = false;
            var warnedMin = false;
            var warnedMax = false;
            for (var x = 0; x < GenomeSize; x++)
            {
                if (!state.Parameters.ParameterExists(paramBase.Push(P_MINGENE).Push("" + x), paramBase.Push(P_MINGENE).Push("" + x)))
                {
                    if (foundStuff && !warnedMin)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing min-gene values for some genes.\n"
                            + "The first one is gene #" + x + ".", paramBase.Push(P_MINGENE).Push("" + x),
                                                                   paramBase.Push(P_MINGENE).Push("" + x));

                        warnedMin = true;
                    }
                }
                else
                {
                    if (DynamicInitialSize)
                        state.Output.WarnOnce("Using dynamic initial sizing, but per-gene min/max gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                            paramBase.Push(P_MINGENE).Push("" + x), paramBase.Push(P_MINGENE).Push("" + x));

                    MinGenes[x] = state.Parameters.GetLongWithDefault(paramBase.Push(P_MINGENE).Push("" + x),
                                                                      paramBase.Push(P_MINGENE).Push("" + x), minGene);
                    foundStuff = true;
                }

                if (!state.Parameters.ParameterExists(paramBase.Push(P_MAXGENE).Push("" + x),
                                             paramBase.Push(P_MAXGENE).Push("" + x)))
                {
                    if (foundStuff && !warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some genes.\n"
                            + "The first one is gene #" + x + ".", paramBase.Push(P_MAXGENE).Push("" + x),
                                                                   paramBase.Push(P_MAXGENE).Push("" + x));
                        warnedMax = true;
                    }
                }
                else
                {
                    if (DynamicInitialSize)
                        state.Output.WarnOnce("Using dynamic initial sizing, but per-gene min/max gene declarations.  This is probably wrong.  You probably want to use global min/max declarations.",
                            paramBase.Push(P_MINGENE).Push("" + x), paramBase.Push(P_MINGENE).Push("" + x));

                    MaxGenes[x] = state.Parameters.GetLongWithDefault(paramBase.Push(P_MAXGENE).Push("" + x),
                                                                      paramBase.Push(P_MAXGENE).Push("" + x), maxGene);
                    foundStuff = true;
                }
            }

            // VERIFY
            for (var x = 0; x < GenomeSize; x++)
            {
                if (MaxGenes[x] < MinGenes[x])
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene[" + x + "] which is <= the max-gene[" + x + "]");

                // check to see if these longs are within the data type of the particular individual
                if (!InNumericalTypeRange(MinGenes[x]))
                    state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                        + I_Prototype.GetType().FullName + ", but doesn't have a min-gene[" + x
                        + "] value within the range of this prototype's genome's data types");

                if (!InNumericalTypeRange(MaxGenes[x]))
                    state.Output.Fatal("This IntegerVectorSpecies has a prototype of the kind: "
                        + I_Prototype.GetType().FullName + ", but doesn't have a max-gene[" + x
                        + "] value within the range of this prototype's genome's data types");
            }
            /*
            //Debugging
            for(int i = 0; i < minGenes.length; i++)
            System.out.PrintLn("Min: " + minGenes[i] + ", Max: " + maxGenes[i]);
            */
        }

        #endregion // Setup
        #region Operations

        public virtual long MaxGene(int gene)
        {
            var m = MaxGenes;
            if (m.Length <= gene)
            {
                if (!DynamicInitialSize && !Warned) WarnAboutGene(gene);
                gene = m.Length - 1;
            }
            return m[gene];
        }
        
        public virtual long MinGene(int gene)
        {
            var m = MinGenes;
            if (m.Length <= gene)
            {
                if (!DynamicInitialSize && !Warned) WarnAboutGene(gene);
                gene = m.Length - 1;
            }
            return m[gene];
        }
        
        public virtual bool InNumericalTypeRange(long geneVal)
        {
            if (I_Prototype is ByteVectorIndividual)
                return (geneVal <= SByte.MaxValue && geneVal >= SByte.MinValue);
            if (I_Prototype is ShortVectorIndividual)
                return (geneVal <= Int16.MaxValue && geneVal >= Int16.MinValue);
            if (I_Prototype is IntegerVectorIndividual)
                return (geneVal <= int.MaxValue && geneVal >= int.MinValue);
            if (I_Prototype is LongVectorIndividual)
                return true;

            // geneVal is valid for all longs
            return false; // dunno what the individual is...
        }

        private void InitializeGenomeSegmentsByStartIndices(IEvolutionState state, IParameter paramBase, IParameter def,
                                                                            int numSegments, long minGene, long maxGene)
        {
            var warnedMin = false;
            var warnedMax = false;

            //loop in reverse order 
            var previousSegmentEnd = GenomeSize;
            var currentSegmentEnd = 0;

            for (var i = numSegments - 1; i >= 0; i--)
            {
                //check if the segment data exist
                if (state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                  def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START)))
                {
                    //Read the index of the end gene specifying current segment
                    currentSegmentEnd = state.Parameters.GetInt(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                                      def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START));
                }
                else
                {
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" + "\nYou must specify start indices for "
                        + numSegments + " segment(s)", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START),
                                                       paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_START));
                }

                //check if the start index is valid
                if (currentSegmentEnd >= previousSegmentEnd || currentSegmentEnd < 0)
                    state.Output.Fatal("Invalid start index value for segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be smaller than " + previousSegmentEnd + " and greater than or equal to  " + 0);

                //check if the index of the first segment is equal to 0
                if (i == 0 && currentSegmentEnd != 0)
                    state.Output.Fatal("Invalid start index value for the first segment "
                        + i + ": " + currentSegmentEnd + "\nThe value must be equal to " + 0);


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Int64.MaxValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE)))
                {
                    if (!warnedMin)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing min-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMin = true;
                    }

                    //the min-gene value has not been defined for this segment so assume the global min value
                    currentSegmentMinGeneValue = minGene;
                }
                //get the min value for this segment
                else
                {
                    currentSegmentMinGeneValue = state.Parameters.GetLongWithDefault(
                                paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);
                }

                var currentSegmentMaxGeneValue = Int64.MinValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE)))
                {
                    if (!warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMax = true;
                    }

                    //the max-gen value has not been defined for this segment so assume the global max value
                    currentSegmentMaxGeneValue = maxGene;
                }
                //get the max value for this segment
                else
                {
                    currentSegmentMaxGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment " + i
                        + " which is <= the max-gene value", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));


                //and assign min and max values for all genes in this segment
                for (var j = previousSegmentEnd - 1; j >= currentSegmentEnd; j--)
                {
                    MinGenes[j] = currentSegmentMinGeneValue;
                    MaxGenes[j] = currentSegmentMaxGeneValue;
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        private void InitializeGenomeSegmentsByEndIndices(IEvolutionState state, IParameter paramBase, IParameter def,
                                                                            int numSegments, long minGene, long maxGene)
        {
            var warnedMin = false;
            var warnedMax = false;

            var previousSegmentEnd = -1;
            var currentSegmentEnd = 0;
            // iterate over segments and set genes values for each segment
            for (var i = 0; i < numSegments; i++)
            {
                //check if the segment data exist
                if (state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                  def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END)))
                {
                    //Read the index of the end gene specifying current segment
                    currentSegmentEnd = state.Parameters.GetInt(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                                      def.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));
                }
                else
                {
                    state.Output.Fatal("Genome segment " + i + " has not been defined!" + "\nYou must specify end indices for "
                        + numSegments + " segment(s)", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END),
                                                       paramBase.Push(P_SEGMENT).Push("" + i).Push(P_SEGMENT_END));
                }

                //check if the end index is valid
                if (currentSegmentEnd <= previousSegmentEnd || currentSegmentEnd >= GenomeSize)
                    state.Output.Fatal("Invalid end index value for segment " + i + ": " + currentSegmentEnd
                            + "\nThe value must be greater than " + previousSegmentEnd + " and smaller than " + GenomeSize);

                //check if the index of the final segment is equal to the GenomeSize
                if (i == numSegments - 1 && currentSegmentEnd != (GenomeSize - 1))
                    state.Output.Fatal("Invalid end index value for the last segment " + i + ": " + currentSegmentEnd
                        + "\nThe value must be equal to the index of the last gene in the genome:  " + (GenomeSize - 1));


                //get min and max values of genes in this segment
                var currentSegmentMinGeneValue = Int64.MaxValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE)))
                {
                    if (!warnedMin)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing min-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMin = true;
                    }

                    //the min-gene value has not been defined for this segment so assume the global min value
                    currentSegmentMinGeneValue = minGene;
                }
                //get the min value for this segment
                else
                {
                    currentSegmentMinGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MINGENE), minGene);
                }

                var currentSegmentMaxGeneValue = Int64.MinValue;
                if (!state.Parameters.ParameterExists(paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE)))
                {
                    if (!warnedMax)
                    {
                        state.Output.Warning("IntegerVectorSpecies has missing max-gene values for some segments.\n"
                            + "The first segment is #" + i + ".", paramBase.Push(P_SEGMENT).Push("" + i),
                                                                  paramBase.Push(P_SEGMENT).Push("" + i));
                        warnedMax = true;
                    }

                    //the max-gen value has not been defined for this segment so assume the global max value
                    currentSegmentMaxGeneValue = maxGene;
                }
                //get the max value for this segment
                else
                {
                    currentSegmentMaxGeneValue = state.Parameters.GetLongWithDefault(
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                    paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE), maxGene);
                }

                //check is min is smaller than or equal to max
                if (currentSegmentMaxGeneValue < currentSegmentMinGeneValue)
                    state.Output.Fatal("IntegerVectorSpecies must have a min-gene value for segment " + i
                        + " which is <= the max-gene value", paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE),
                                                             paramBase.Push(P_SEGMENT).Push("" + i).Push(P_MAXGENE));

                //and assign min and max values for all genes in this segment
                for (var j = previousSegmentEnd + 1; j <= currentSegmentEnd; j++)
                {
                    MinGenes[j] = currentSegmentMinGeneValue;
                    MaxGenes[j] = currentSegmentMaxGeneValue;
                }

                previousSegmentEnd = currentSegmentEnd;
            }
        }

        #endregion // Operations
    }
}