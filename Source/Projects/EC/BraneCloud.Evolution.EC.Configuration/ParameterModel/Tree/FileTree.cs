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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BraneCloud.Evolution.EC.Configuration;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class FileTree : IFileTree
    {
        public const string DefaultSearchPattern = "*.params";

        #region Constructors

        public FileTree(string rootDir)
        {
            Build(rootDir, DefaultSearchPattern);
        }

        public FileTree(string rootDir, string fileSearchPattern)
        {
            FileSearchPattern = fileSearchPattern;
            Build(rootDir, fileSearchPattern);
        }

        #endregion // Constructors
        #region IParameterNamespace

        public string Name { get { return Root == null ? "" : Root.Name; } }
        public string FullName { get { return Root == null ? "" : Root.FullName; } }

        #endregion // IParameterNamespace
        #region Build

        public void Build(ParameterSourceLocator sourceLocator)
        {
            var rootDir = sourceLocator.Path;
            Build(rootDir, DefaultSearchPattern);
        }

        public void Build(string rootDir)
        {
            Build(rootDir, DefaultSearchPattern);
        }

        /// <summary>
        /// This override for the Build(...) method is for the case in which
        /// the parameter file extension has been changed. 
        /// Instead of searching for "*.params", the client can search 
        /// for whatever extension it might require.
        /// </summary>
        /// <param name="rootDir">The top-level directory where the tree will be "rooted".</param>
        /// <param name="fileSearchPattern"></param>
        public void Build(string rootDir, string fileSearchPattern)
        {
            // Clear the "registry" so that it does not reflect old information if something goes wrong along the way.
            NodeRegistry = null;
            if (String.IsNullOrEmpty(rootDir) || !Directory.Exists(rootDir) || Path.GetDirectoryName(rootDir).StartsWith("."))
                throw new DirectoryNotFoundException(String.Format("A valid directory must be specified: {0}", rootDir));

            Root = new FileDirectory(new DirectoryInfo(rootDir), fileSearchPattern);
            NodeRegistry = Root.Collect(); // New file information
        }

        #endregion // Build

        public FileDirectory Root { get; protected set; }

        public string FileSearchPattern { get; protected set; }

        /// <summary>
        /// At the end of the "Build" operation, this reflects all of the files that were found by all of the nodes.
        /// It is, in essence, a "flattened" view of the tree.
        /// </summary>
        public IDictionary<string, File> NodeRegistry { get; protected set; }

        #region IParameterSource

        public bool SourceExists { get { return Root == null ? false : Root.Exists; } }

        public bool ParameterExists(IParameter parameter, IParameter defaultParameter)
        {
            throw new NotImplementedException();
        }

        public void SetParameter(IParameter parameter, string paramValue)
        {
            throw new NotImplementedException();
        }

        #region ToString()   ****************************************************************************************************

        public override string ToString()
        {
            var sb = new StringBuilder();
            ProcessNodeForString(sb, Root, "");
            return sb.ToString();
        }

        public string ToString(string initialIndent)
        {
            var sb = new StringBuilder();
            ProcessNodeForString(sb, Root, "");
            return sb.ToString();
        }

        private static void ProcessNodeForString(StringBuilder sb, FileDirectory node, string indent)
        {
            sb.AppendFormat("{0}{1}{2}\n", indent, "Dir:    ", node.FullName);
            sb.AppendFormat("{0}{1}\n", indent, "Files:");
            foreach (var f in node.Files.Values)
            {
                sb.AppendFormat("{0}\t{1}\n", indent, f.Name);
                var props = f.Properties;
                if (props == null || props.Count == 0) continue;
                sb.AppendFormat("{0}\t{1}{2}\n", indent, "Properties: ", props.Keys.Count);
                foreach (var p in props)
                {
                    sb.AppendFormat("{0}\t\t[ {1} = {2} ]\n", indent, p.Key, p.Value);
                }
            }
            sb.AppendLine();
            // now indent for the next level in the branch
            indent += "\t";
            foreach (var di in node.Children)
            {
                ProcessNodeForString(sb, (FileDirectory)di, indent);
            }
        }

        #endregion

        #region ToXml()  ***************************************************************************************************************

        public XElement ToXml()
        {
            var xml = new XElement("root", new XAttribute("path", Root.Info.FullName));
            ProcessNodeForXml(xml, Root);
            return xml;
        }

        private static void ProcessNodeForXml(XElement xml, FileDirectory node)
        {
            var files = new XElement("files", new XAttribute("count", node.Files.Count));
            xml.Add(files);
            foreach (var f in node.Files.Values)
            {
                var props = f.Properties;
                if (props == null || props.Count == 0) continue;
                var file = new XElement("file", new XAttribute("name", f.Name), new XAttribute("propertyCount", props.Count));
                files.Add(file);
                foreach (var p in props)
                {
                    file.Add(new XElement("property", new XAttribute("key", p.Key), new XAttribute("value", p.Value)));
                }
            }
            foreach (FileDirectory d in node.Children)
            {
                var dir = new XElement("directory", new XAttribute("name", d.Name));
                xml.Add(dir);
                ProcessNodeForXml(dir, d);
            }
        }

        #endregion // ToXml()

        #endregion // IParameterSource
    }
}