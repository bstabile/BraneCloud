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
using System.Xml.Linq;
using BraneCloud.Evolution.Collections.Generic;
using BraneCloud.Evolution.Collections.Generic.Tree;

namespace BraneCloud.Evolution.EC.Configuration
{
    public class FileDirectory : ComplexTreeNode<FileDirectory>, IFileDirectory
    {
        public const string DefaultFileSearchPattern = "*.params";

        #region Constructors

        public FileDirectory(string rootDir)
        {
            Build(rootDir, DefaultFileSearchPattern);
        }

        public FileDirectory(DirectoryInfo info, string fileSearchPattern)
        {
            if (info == null) throw new ArgumentNullException("info", "The DirectoryInfo argument must not be null.");
            Info = info;
            Files = new Dictionary<string, File>();
            FileSearchPattern = fileSearchPattern;
            Build();
        }

        #endregion // Constructors

        public void Build(string rootDir)
        {
            Build(rootDir, DefaultFileSearchPattern);
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
            Files = null;

            // NOTE: The check for subdirectories with a leading dot is a special case to avoid subversion source control entries
            if (String.IsNullOrEmpty(rootDir) || Path.GetDirectoryName(rootDir).StartsWith(".") || !Directory.Exists(rootDir))
                throw new DirectoryNotFoundException(String.Format("A valid directory must be specified: {0}", rootDir));

            Info = new DirectoryInfo(rootDir);
            Files = Collect(new Dictionary<string, File>()); // New file information
        }

        // The following properties just help make things a little cleaner for the client (for binding etc.)

        #region IParameterNamespace

        public string Name { get { return Info == null ? "" : Info.Name; } }
        public string FullName { get { return Info == null ? "" : Info.FullName; } }
        public bool Exists { get { return Info.Exists; } }

        #endregion // IParameterNamespace

        #region IParameterDirectory

        public IFileDirectory this[string subDirName]
        {
            get
            {
                var subdirs = subDirName.Split(new char[] {Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar});
                var curr = this;
                return subdirs.Aggregate(curr, (current, dir) => GetSubdirectory(current, dir));
            }
        }
        private static FileDirectory GetSubdirectory(FileDirectory root, string subDirName)
        {
            return (from c in root.Children.OfType<FileDirectory>()
                    where c.Name == subDirName
                    select c).First();
        }

        public DirectoryInfo Info { get; protected set; }
        public string FileSearchPattern { get; protected set; }
        public IDictionary<string, File> Files { get; protected set; }
        public void Build()
        {
            Children.Clear();
            Files.Clear();

            if (!Info.Exists)
            {
                throw new DirectoryNotFoundException(String.Format("The directory could not be found: {0}", Info.FullName));
            }

            // Search all subdirectories first
            var subdirs = Info.GetDirectories();
            foreach (var d in subdirs)
            {
                if (!d.Exists) continue; // This shouldn't happen! But if it does, we won't get all bothered about it.

                // Ignore special case of Subversion directories
                // Ignore when no files beneath match search pattern
                if (d.Name.StartsWith(".")) continue; // || d.GetFiles(FileSearchPattern, SearchOption.AllDirectories).Length == 0) continue;

                Children.Add(new FileDirectory(d, FileSearchPattern));
            }

            // Now load any relevant files for the current directory
            LoadFiles();

            // By the time we get here, all nodes beneath us should have been built.
            // If there are no children and there are no matching files in this directory,
            // then it is appropriate to prune ourselves out of the tree.
            // Note that we first check to make sure we aren't the root!
            if (Parent != null && Children.Count == 0 && Files.Count == 0)
            {
                Parent.Children.Remove(this);
                Parent = null;
                Dispose();
            }
        }

        public IDictionary<string, File> Collect()
        {
            return Collect(null);
        }

        public IDictionary<string, File> Collect(IDictionary<string, File> registry)
        {
            var reg = registry ?? new Dictionary<string, File>();
            foreach (var pfile in Files)
            {
                // It's important to use the "FullName" here so there there is no chance of name collision
                reg.Add(pfile.Value.FullName, pfile.Value);
            }
            foreach (var dir in Children.OfType<FileDirectory>())
            {
                dir.Collect(reg);
            }
            return reg;
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

        public string ToXml()
        {
            var xml = new XElement("root", new XAttribute("path", Root.Info.FullName));
            ProcessNodeForXml(xml, Root);
            return xml.ToString();
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
        #endregion

        private void LoadFiles()
        {
            if (String.IsNullOrWhiteSpace(FileSearchPattern)) return;

            // Load the files that match our extension.

            var arr = Info.GetFiles(FileSearchPattern, SearchOption.TopDirectoryOnly);
            foreach (var fi in arr)
            {
                Files.Add(fi.Name, new File(fi));
            }
        }

    }
}