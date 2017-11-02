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
using System.IO;

using BraneCloud.Evolution.EC.Util;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.GP
{
    /// <summary> 
    /// ERC is an abstract GPNode which implements Ephemeral Random Constants,
    /// as described in Koza I.  An ERC is a node which, when first instantiated,
    /// gets set to some random constant value which it always returns from
    /// then on, even after being crossed over into other individuals.
    /// In order to implement an ERC, you need to override several methods below.
    /// 
    /// <h2>Impementing an ERC</h2>
    /// 
    /// A basic no-frills ERC needs to have the following things:
    /// 
    /// <ul>
    /// <li/>The data holding the ERC (perhaps a float or a couple of floats)
    /// <li/>An implementation of the <b>eval</b> method which returns the appropriate
    /// data when the node is evaluated.
    /// <li/>Possibly an implementation of the <b>clone</b> method to copy that data
    /// properly.  If your ERC data is just a simple or immutable type
    /// (like an int or a string), you don't need write a clone() method;
    /// the default one works fine.  But if your data is an array or other
    /// mutable object, you'll need to override the clone() method to copy
    /// the array.  
    /// 
    /// <li/>An implementation of the <b>resetNode</b> method to randomize the
    /// data once cloned from the prototype.  This essentially "initializes"
    /// your ERC.
    /// 
    /// <li/>An implementation of the <b>encode</b> method which presents the
    /// ERC as a String.  If you don't plan on writing individuals out to
    /// files in a fashion that enables them to be read back in again later,
    /// but only care to print out individuals for statistics purposes, 
    /// you can implement this to just
    /// write <tt>"" + <i>value</i></tt>, where <i>value</i> is your data.
    /// 
    /// <li/>An implementation of the <b>nodeEquals</b> method to return true if
    /// the other node is also an ERC of the same type, and it has the
    /// same ERC data as yourself.
    /// </ul>
    /// A more advanced ERC will need some of the following gizmos:
    /// <ul>
    /// <li/>If you have ERCs of different class types (for example, a vector ERC
    /// and a floating-point scalar ERC), you will wish to distinguish them
    /// when they're printed to files.  To do this,  override the <b>name</b> 
    /// method to return different strings for each of them (perhaps "vec" versus "").
    /// 
    /// <li/>If you want to write your ERCs to files such that they can be read
    /// back in again, you'll need to override the <b>encode</b> method
    /// to write using the <tt>ec.util.Code</tt> class.  Further, you'll need to
    /// override the <b>decode</b> method to read in the individual using the
    /// <tt>ec.util.Code</tt> and <tt>ec.util.DecodeReturn</tt> classes.  The
    /// default version -- which is wrong -- returns <tt>false</tt>.
    /// When you do this, you'll probably also want to override the <b>ToStringForHumans()</b>
    /// method to return a simple string form of the ERC: perhaps just a number
    /// or a vector like "[7.24, 9.23]".  This is because by default <b>ToStringForHumans()</b>
    /// calls <b>ToString()</b>, which in turn calls <b>encode</b>, which you have
    /// just overidden to be more computer-ish.
    /// 
    /// <li/>ERCs can be mutated using a custom mutator pipeline, for example the
    /// <b>ec.gp.breed.MutateERCPipeline</b>.  If you expect to mutate your ERCs,
    /// you may wish to override the <b>mutateERC</b> method to do something
    /// more subtle than its default setting (which just randomizes the
    /// ERC again, by calling resetNode).
    /// 
    /// <li/>The default <b>nodeHashCode</b> implementation is poor and slow (it
    /// creates a string using Encode() and then hashes the sting).  You might
    /// create a better (and probably simpler) hash code function.
    /// 
    /// <li/>If you're going to use facilities such as the Island Model or the distributed
    /// evaluator, you'll need to implement the <b>writeNode</b> and <b>readNode</b>
    /// methods to read/write the node to DataInput/DataOutput.  The default implementations
    /// just throw errors.
    /// 
    /// <li/>If you need to set up your ERC class from the parameter file, do so in the <b>Setup</b> method.
    /// </ul>
    /// <p/> See the <b>ec.app.regression.func.RegERC</b> class for an example of a simple but "fuly-implemented"
    /// ERC.  A slightly more complicated example can be found in <b>ec.app.lawnmower.func.LawnERC</b>.
    /// </summary>
    [Serializable]
    [ECConfiguration("ec.gp.ERC")]
    public abstract class ERC : GPNode
    {
        #region Constants

        public const string ERC_PREFIX = "ERC";

        #endregion // Constants
        #region Properties

        /// <summary>
        /// Returns the lowercase "name" of this ERC function class, some
        /// simple, short name which distinguishes this class from other ERC
        /// function classes you're using.  If you have more than one ERC function,
        /// you need to distinguish them here.  By default the value is "ERC",
        /// which works fine for a single ERC function in the function set.
        /// Whatever the name is, it should generally only have letters, 
        /// numbers, or hyphens or underscores in it.
        /// No whitespace or other characters. 
        /// </summary>
        public override string Name
        {
            get { return "ERC"; }
        }

        /// <summary>
        /// Usually ERCs don't have children, and this default implementation makes certain of it. 
        /// But if you want to override this, you're welcome to.
        /// </summary>
        public override int ExpectedChildren => 0;

        #endregion // Properties
        #region Setup

        /// <summary>
        /// Remember to override this to randomize your ERC after it has been cloned.  
        /// The prototype will not ever receive this method call. 
        /// </summary>
        public abstract override void ResetNode(IEvolutionState state, int thread);

        #endregion // Setup
        #region Operations

        /// <summary>
        /// Mutates the node's "value".  This is called by mutating operators
        /// which specifically <i>mutate</i> the "value" of ERCs, as opposed to 
        /// replacing them with whole new ERCs. The default form of this function
        /// simply calls resetNode(state,thread), but you might want to modify
        /// this to do a specialized form of mutation, applying gaussian
        /// noise for example. 
        /// </summary>       
        public virtual void MutateERC(IEvolutionState state, int thread)
        {
            ResetNode(state, thread);
        }

        #endregion // Operations
        #region Comparison

        /// <summary>
        /// Implement this to do ERC-to-ERC comparisons. 
        /// </summary>
        public abstract override bool NodeEquals(GPNode node);

        /// <summary>
        /// Implement this to hash ERCs, along with other nodes, in such a way that two
        /// "equal" ERCs will usually hash to the same value. The default value, which 
        /// may not be very good, is a combination of the class hash code and the hash
        /// code of the string returned by Encode().  You might make a better hash value. 
        /// </summary>
        public override int NodeHashCode()
        {
            return base.NodeHashCode() ^ Encode().GetHashCode();
        }

        #endregion // Comparison
        #region Encoding

        /// <summary>
        /// Encodes data from the ERC, using BraneCloud.Evolution.EC.util.Code.  
        /// </summary>
        public abstract string Encode();

        /// <summary>
        /// Decodes data into the ERC from dret.  Return true if you sucessfully
        /// decoded, false if you didn't.  Don't increment dret.Pos's value beyond
        /// exactly what was needed to decode your ERC.  If you fail to decode,
        /// you should make sure that the position and data in the dret are exactly
        /// as they were originally. 
        /// </summary>
        public virtual bool Decode(DecodeReturn dret)
        {
            return false;
        }

        #endregion // Encoding
        #region ToString

        /// <summary>
        /// You might want to override this to return a special human-readable version of the erc value; 
        /// otherwise this defaults to ToString();  This should be something that resembles a LISP atom.  
        /// If a simple number or other object won't suffice, you might use something that begins with ERC_PREFIX + name() + [ + ... + ] 
        /// </summary>
        public override string ToStringForHumans()
        {
            return ToString();
        }

        /// <summary>
        /// This defaults to simply name() + "[" + Encode() + "]" 
        /// </summary>
        public override string ToString()
        {
            return Name + "[" + Encode() + "]";
        }

        #endregion // ToString
        #region IO

        /// <summary>
        /// To successfully write to a BinaryWriter, you must override this to write your specific ERC data out.  
        /// The default implementation issues a fatal error. 
        /// </summary>
        public override void WriteNode(IEvolutionState state, BinaryWriter writer)
        {
            state.Output.Fatal("WriteNode(EvolutionState,DataInput) not implemented in " + GetType().FullName);
        }
        
        /// <summary>
        /// To successfully read from a BinaryReader, you must override this to read your specific ERC data in.  
        /// The default implementation issues a fatal error. 
        /// </summary>
        public override void ReadNode(IEvolutionState state, BinaryReader reader)
        {
            state.Output.Fatal("ReadNode(EvolutionState,DataInput) not implemented in " + GetType().FullName);
        }
        
        public override GPNode ReadNode(DecodeReturn dret)
        {
            var len = dret.Data.Length;
            var originalPos = dret.Pos;
            
            // get my name
            var str2 = Name + "[";
            var len2 = str2.Length;
            
            if (dret.Pos + len2 >= len)
            // uh oh, not enough space
                return null;
            
            // check it out
            for (var x = 0; x < len2; x++)
                if (dret.Data[dret.Pos + x] != str2[x])
                    return null;
            
            // looks good!  try to load this sucker.
            dret.Pos += len2;
            var node = (ERC) LightClone();
            if (!node.Decode(dret))
            {
                dret.Pos = originalPos; 
                return null;
            } // couldn't decode it
            
            // the next item should be a "]"
            
            if (dret.Pos >= len)
            {
                dret.Pos = originalPos; 
                return null;
            }
            if (dret.Data[dret.Pos] != ']')
            {
                dret.Pos = originalPos; 
                return null;
            }
            
            // Check to make sure that the ERC's all there is
            if (dret.Data.Length > dret.Pos + 1)
            {
                char c = dret.Data[dret.Pos + 1];
                if (!Char.IsWhiteSpace(c) && c != ')' && c != '(')
                // uh oh
                {
                    dret.Pos = originalPos; 
                    return null;
                }
            }
            
            dret.Pos++;
            
            return node;
        }

        #endregion // IO
    }
}