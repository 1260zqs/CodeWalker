using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace CodeWalker
{
    public class PackageManifest : XmlDocument
    {
        class Content
        {
            public string filename;
            public List<string> archives = new List<string>();
        }

        public string Text => ToText();

        public string FindArchiveFileSource(string path)
        {
            var content = ParsePath(path);
            var list = FindArchiveFile(content);
            foreach (var xmlElement in list)
            {
                if (xmlElement.Name == "add" && xmlElement.InnerText == content.filename)
                {
                    return xmlElement.GetAttribute("source");
                }
            }
            return null;
        }

        public void SetPatchFile(string archive, string source)
        {
            var content = ParsePath(archive);
            var list = new List<XmlElement>();
            FindArchiveRecursive(GetContentNode(), content.archives, 0, list);
            if (list.Count == 0)
            {
                list.Add(AddArchive(archive));
            }

            XmlElement node = null;
            foreach (var archiveNode in list)
            {
                foreach (XmlElement xmlElement in archiveNode.GetElementsByTagName("add"))
                {
                    if (xmlElement.InnerText == content.filename)
                    {
                        node = xmlElement;
                        break;
                    }
                }
            }
            if (node == null)
            {
                node = CreateElement("add");
                list[0].AppendChild(node);
            }
            node.InnerText = content.filename;
            node.SetAttribute("source", source);
        }

        public List<XmlElement> FindArchive(string path)
        {
            var content = ParsePath(path);
            var list = new List<XmlElement>();
            FindArchiveRecursive(GetContentNode(), content.archives, 0, list);
            return list;
        }

        public List<XmlElement> FindArchiveFile(string path)
        {
            var content = ParsePath(path);
            return FindArchiveFile(content);
        }

        private List<XmlElement> FindArchiveFile(Content content)
        {
            var list = new List<XmlElement>();
            FindArchiveFileRecursive(GetContentNode(), content, 0, list);
            return list;
        }

        void FindArchiveRecursive(XmlElement parent, List<string> archives, int index, List<XmlElement> results)
        {
            if (index >= archives.Count)
            {
                results.Add(parent);
                return;
            }
            foreach (XmlElement child in parent.GetElementsByTagName("archive"))
            {
                if (IsArchiveWithPath(child, archives[index]))
                {
                    FindArchiveRecursive(child, archives, index + 1, results);
                }
            }
        }

        void FindArchiveFileRecursive(XmlElement parent, Content content, int index, List<XmlElement> results)
        {
            var archives = content.archives;
            if (index >= archives.Count)
            {
                foreach (XmlElement child in parent.GetElementsByTagName("add"))
                {
                    if (IsValue(child.InnerText, content.filename))
                    {
                        results.Add(child);
                    }
                }
                return;
            }

            foreach (XmlElement child in parent.GetElementsByTagName("archive"))
            {
                if (IsArchiveWithPath(child, archives[index]))
                {
                    FindArchiveFileRecursive(child, content, index + 1, results);
                }
            }
        }

        public XmlElement AddArchive(string path)
        {
            var content = ParsePath(path);
            return AddArchive(content);
        }

        private XmlElement AddArchive(Content content)
        {
            XmlElement current = null;
            XmlElement parent = GetContentNode();
            for (var i = 0; i < content.archives.Count; i++)
            {
                var archive = content.archives[i];
                var matches = parent.ChildNodes.OfType<XmlElement>()
                    .Where(e => IsArchiveWithPath(e, archive))
                    .ToList();

                current = null;
                foreach (var m in matches)
                {
                    if (MatchArchiveChain(m, content.archives, i + 1))
                    {
                        current = m;
                        break;
                    }
                }
                if (current == null)
                {
                    current = CreateElement("archive");
                    current.SetAttribute("path", archive);
                    current.SetAttribute("type", "RPF7");
                    current.SetAttribute("createIfNotExist", i == 0 ? "True" : "False");
                    parent.AppendChild(current);
                }
                parent = current;
            }
            return current;
        }

        private bool MatchArchiveChain(XmlElement node, List<string> archives, int index)
        {
            if (index >= archives.Count) return true;

            foreach (XmlElement child in node.GetElementsByTagName("archive"))
            {
                if (IsArchiveWithPath(child, archives[index]) && MatchArchiveChain(child, archives, index + 1))
                {
                    return true;
                }
            }
            return false;
        }

        private Content ParsePath(string fullPath)
        {
            var content = new Content();
            var parts = fullPath.Split('\\');
            var rpfIndexes = parts
                .Select((p, i) => new { p, i })
                .Where(x => x.p.EndsWith(".rpf", StringComparison.OrdinalIgnoreCase))
                .Select(x => x.i).ToList();
            for (var i = 0; i < rpfIndexes.Count; i++)
            {
                var start = i == 0 ? 0 : rpfIndexes[i - 1] + 1;
                var end = rpfIndexes[i];
                var archive = string.Join("\\", parts.Skip(start).Take(end - start + 1));
                content.archives.Add(archive);
            }
            var fileIndex = rpfIndexes[rpfIndexes.Count - 1] + 1;
            if (fileIndex < parts.Length)
            {
                content.filename = string.Join("\\", parts.Skip(fileIndex));
            }
            return content;
        }

        public string ToText()
        {
            using (var sw = new StringWriter())
            using (var xw = new XmlTextWriter(sw))
            {
                xw.Formatting = Formatting.Indented;
                WriteTo(xw);
                xw.Flush();
                return sw.ToString();
            }
        }

        public XmlElement GetContentNode()
        {
            return (XmlElement)SelectSingleNode("/package/content");
        }

        static bool IsValue(string value, string x)
        {
            return string.Equals(value, x, StringComparison.InvariantCultureIgnoreCase);
        }

        static bool HasAttributeValue(XmlElement xmlElement, string name, string value)
        {
            foreach (XmlAttribute attribute in xmlElement.Attributes)
            {
                if (IsValue(attribute.Name, name) && IsValue(attribute.Value, value))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsArchiveWithPath(XmlElement archive, string path)
        {
            return IsValue(archive.Name, "archive") && HasAttributeValue(archive, "path", path);
        }
    }
}