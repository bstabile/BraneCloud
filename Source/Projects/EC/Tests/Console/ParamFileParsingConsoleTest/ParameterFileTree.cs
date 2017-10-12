using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using BraneCloud.Evolution.EC.Configuration;

namespace ParamFileParsingConsoleTest
{
    public class ParameterFileTree
    {
        public const string DefaultFileSearchPattern = "*.params";

        private const string RegexSloppy = @"^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]*\=(\s)*(?<Value>([^\n^\s]*(\n){0,1})))";
        private const string RegexStrict = @"^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]*\=(\s)*(?<Value>([^\n^\s]*(\n){0,1})))";

        public ParameterFileTree(string rootDir)
        {
            Build(rootDir, DefaultFileSearchPattern);
        }

        public ParameterFileTree(string rootDir, string fileSearchPattern)
        {
            FileSearchPattern = fileSearchPattern;
            Build(rootDir, fileSearchPattern);
        }

        public ParameterDirectoryNode Root { get; protected set; }
        public string FileSearchPattern { get; protected set; }

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
            if (String.IsNullOrEmpty(rootDir) || !Directory.Exists(rootDir) || Path.GetDirectoryName(rootDir).StartsWith("."))
                throw new DirectoryNotFoundException(String.Format("A valid directory must be specified: {0}", rootDir));

            Root = new ParameterDirectoryNode(new DirectoryInfo(rootDir), fileSearchPattern);
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

        private static void ProcessNodeForString(StringBuilder sb, ParameterDirectoryNode node, string indent)
        {
            sb.AppendFormat("{0}{1}{2}\n", indent, "Dir:    ", node.Info.FullName);
            sb.AppendFormat("{0}{1}\n", indent, "Files:");
            foreach (var fi in node.Files)
            {
                sb.AppendFormat("{0}\t{1}\n", indent, fi.Name);
                var dict = ParseForParameters(fi);
                if (dict == null || dict.Count == 0) continue;
                sb.AppendFormat("{0}\t{1}{2}\n", indent, "Properties: ", dict.Keys.Count);
                foreach (var p in dict)
                {
                    sb.AppendFormat("{0}\t\t[ {1} = {2} ]\n", indent, p.Key, p.Value);
                }
            }
            sb.AppendLine();
            // now indent for the next level in the branch
            indent += "\t";
            foreach (var di in node.Children)
            {
                ProcessNodeForString(sb, (ParameterDirectoryNode)di, indent);
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

        private static void ProcessNodeForXml(XElement xml, ParameterDirectoryNode node)
        {
            var files = new XElement("files", new XAttribute("count", node.Files.Count));
            xml.Add(files);
            foreach (var fi in node.Files)
            {
                var dict = ParseForParameters(fi);
                if (dict == null || dict.Count == 0) continue;
                var file = new XElement("file", new XAttribute("name", fi.Name), new XAttribute("propertyCount", dict.Count));
                files.Add(file);
                foreach (var p in dict)
                {
                    file.Add(new XElement("property", new XAttribute("key", p.Key), new XAttribute("value", p.Value)));
                }
            }
            foreach (ParameterDirectoryNode di in node.Children)
            {
                var dir = new XElement("directory", new XAttribute("name", di.Info.Name));
                xml.Add(dir);
                ProcessNodeForXml(dir, di);
            }
        }
        #endregion // ToXml()

        #region ParseForParameters   **********************************************************************************************

        static IDictionary<string, string> ParseForParameters(FileInfo fileInfo)
        {
            if (fileInfo == null || !fileInfo.Exists) return null;
            using (var reader = new StreamReader(fileInfo.FullName))
            {
                return ParseForParametersAllowSpaceDelim(reader);
            }
        }

        static IDictionary<string, string> ParseForParameters(Stream stream)
        {
            if (stream == null || stream.Length == 0) return null;
            using (var reader = new StreamReader(stream))
            {
                return ParseForParametersAllowSpaceDelim(reader);
            }
        }

        static IDictionary<string, string> ParseForParametersAllowSpaceDelim(TextReader reader)
        {
            if (reader == null) return null;
            var dict = new Dictionary<string, string>();
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                line = line.Trim();
                if (line.Length == 0 || line.StartsWith("#")) continue;
                if (line.Contains("="))
                {
                    var a = line.Split(new[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length != 2) continue;
                    a[0] = a[0].Trim();
                    a[1] = a[1].Trim();

                    if (dict.ContainsKey(a[0]))
                        dict[a[0]] = a[1];
                    else
                        dict.Add(a[0], a[1]);
                }
                else
                {
                    var a = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (a.Length < 2) continue;

                    if (dict.ContainsKey(a[0]))
                        dict[a[0].Trim()] = a[1].Trim();
                    else
                        dict.Add(a[0].Trim(), a[1].Trim());
                }
            }
            return dict;
        }

        static IDictionary<string, string> ParseForParameters(TextReader reader)
        {
            if (reader == null) return null;
            var dict = new Dictionary<string, string>();

            var matches = Regex.Matches(reader.ReadToEnd(), RegexStrict, RegexOptions.Multiline);
            foreach (Match m in matches)
            {
                var arr = m.Value.Split('=');
                if (arr.Length == 2)
                {
                    var k = arr[0].Trim();
                    var v = arr[1].Trim();
                    if (dict.ContainsKey(k))
                    {
                        dict.Add(k + "_", v);
                    }
                    else
                    {
                        dict.Add(k, v);
                    }
                }
            }
            return dict;
        }

        #endregion // ParseForParameters
    }

}
// Potential regexes that might be usefull
// ^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]* \= (\s)*(?<Value>([^\n^\s]+(\n){0,1})))
// @"^\s*[^#]((\s)*(?<Key>([^\=^\s^\n]+))[\s^\n]* \=(\s){0,1}(?:\s*)(?<Value>([^\n^\s]+(\n){0,1})))"