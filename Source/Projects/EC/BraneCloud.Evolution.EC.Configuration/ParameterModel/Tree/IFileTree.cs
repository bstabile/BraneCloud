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

using System.Collections.Generic;

namespace BraneCloud.Evolution.EC.Configuration
{
    public interface IFileTree : IParameterSource, IParameterSourceBuilder
    {
        FileDirectory Root { get; }
        string FileSearchPattern { get; }

        /// <summary>
        /// At the end of the "Build" operation, this reflects 
        /// all of the files that were found beneath the root.
        /// It is, in essence, a "flattened" view of the tree.
        /// </summary>
        IDictionary<string, File> NodeRegistry { get; }

        /// <summary>
        /// This override for the Build(...) method uses the specified path
        /// to locate a file that will be parsed. It uses the DefaultSearchPattern
        /// to filter the files that are to be discovered and processed.
        /// Note that the recommmended approach is to use the overload that
        /// accepts a <see cref="ParameterSourceLocator"/> for determining
        /// the root path at which to begin the search.
        /// </summary>
        /// <param name="rootDir">The root directory where the tree will be "anchored".</param>
        void Build(string rootDir);

        /// <summary>
        /// This override for the Build(...) method is for the case in which
        /// the parameter file extension has been changed. 
        /// Instead of searching for "*.params", the client can search 
        /// for whatever extension it might require.
        /// </summary>
        /// <param name="rootDir">The top-level directory where the tree will be "rooted".</param>
        /// <param name="fileSearchPattern"></param>
        void Build(string rootDir, string fileSearchPattern);
    }
}