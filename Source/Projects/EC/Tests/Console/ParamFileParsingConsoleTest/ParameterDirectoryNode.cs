using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using BraneCloud.Evolution.Collections.Generic;

namespace ParamFileParsingConsoleTest
{
    public class ParameterDirectoryNode : ComplexTreeNode<ParameterDirectoryNode>
    {
        public ParameterDirectoryNode(DirectoryInfo info, string fileSearchPattern)
        {
            if (info == null) throw new ArgumentNullException("info", "The DirectoryInfo argument must not be null.");
            Info = info;
            Files = new List<FileInfo>();
            FileSearchPattern = fileSearchPattern;
            Build();
        }

        public DirectoryInfo Info { get; protected set; }
        public string FileSearchPattern { get; protected set; }
        public List<FileInfo> Files { get; protected set; }


        public void Build()
        {
            Children.Clear();
            Files.Clear();

            if (!Info.Exists)
            {
                throw new DirectoryNotFoundException(String.Format("The directory could not be found: {0}",
                                                                   Info.FullName));
            }

            // Search all subdirectories first
            var subdirs = Info.GetDirectories();
            foreach (var d in subdirs)
            {
                if (!d.Exists) continue; // This shouldn't happen! But if it does, we won't get all bothered about it.

                // Ignore special case of Subversion directories
                // Ignore when no files beneath match search pattern
                if (d.Name.StartsWith(".")) continue; // || d.GetFiles(FileSearchPattern, SearchOption.AllDirectories).Length == 0) continue;

                Children.Add(new ParameterDirectoryNode(d, FileSearchPattern));
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

        private void LoadFiles()
        {
            if (String.IsNullOrWhiteSpace(FileSearchPattern)) return;

            // Load the files that match our extension.
            Files.AddRange(Info.GetFiles(FileSearchPattern, SearchOption.TopDirectoryOnly));
        }
    }
}
