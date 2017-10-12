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
using System.Diagnostics;
using System.IO;
using BraneCloud.Evolution.EC.Configuration;

namespace ParamFileParsingConsoleTest
{
    public class Program
    {
        private const string RelativePath = @"..\..\..\..\..\..\Solutions\EC\ParamFiles\ec";
        private const string NameValueFormat = "{0} = {1}";

        static void Main(string[] args)
        {
            var rootDir = Path.GetFullPath(RelativePath);
            Console.WriteLine(NameValueFormat, "RelativePath", RelativePath);
            Console.WriteLine(NameValueFormat, "RootDir", rootDir);

            if (!Directory.Exists(rootDir))
            {
                Console.WriteLine("Invalid path specified: {0}", rootDir);
                Console.WriteLine("Press <enter> to exit...");
                Console.ReadLine();
                return;
            }
            var tree = new FileTree(rootDir);
            Console.WriteLine();
            var writer = new StreamWriter("tree.txt", false);
            writer.Write(tree.ToString());
            writer.Write(Environment.NewLine);
            writer.Write(tree.ToXml());
            writer.Flush();
            writer.Close();
            Process.Start("tree.txt");
            Console.WriteLine();

            Console.WriteLine("Press <enter> to exit...");
            Console.ReadLine();
        }

    }
}