using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml;
using System.Xml.Serialization;
using Spine.Collections;

namespace CodeWalker
{
    public class TextureModProject
    {
        public string workingPath;
        public List<TextureReplacement> replacements = new List<TextureReplacement>();
        public OrderedDictionary<Guid, ModTexture> modTextures = new OrderedDictionary<Guid, ModTexture>();
        public OrderedDictionary<Guid, SourceTexture> sourceTextures = new OrderedDictionary<Guid, SourceTexture>();

        public List<SourceTexture> FindSourceTextures(Guid modeTexId)
        {
            var list = new List<SourceTexture>();
            foreach (var replacement in replacements)
            {
                if (replacement.modTexture == modeTexId)
                {
                    if (sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
                    {
                        list.Add(sourceTexture);
                    }
                }
            }
            return list;
        }

        public static void Save(TextureModProject project, string file)
        {
            var settings = new XmlWriterSettings
            {
                Indent = true
            };

            using (var writer = XmlWriter.Create(file, settings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("TextureModProject");
                writer.WriteElementString("WorkingPath", project.workingPath);

                writer.WriteStartElement("Replacements");
                foreach (var replacement in project.replacements)
                {
                    writer.WriteStartElement("TextureReplacement");
                    writer.WriteElementString("Id", $"{replacement.id:N}");
                    writer.WriteElementString("ModTexture", $"{replacement.modTexture:N}");
                    writer.WriteElementString("SourceTexture", $"{replacement.sourceTexture:N}");
                    writer.WriteElementString("Tag", replacement.tag);
                    writer.WriteElementString("FlipX", $"{replacement.flipX}");
                    writer.WriteElementString("FlipY", $"{replacement.flipY}");
                    writer.WriteElementString("Rotation", $"{replacement.rotation}");
                    writer.WriteElementString("Comment", replacement.comment);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("ModTextures");
                foreach (var modTexture in project.modTextures.Values)
                {
                    writer.WriteStartElement("ModTexture");
                    writer.WriteElementString("Id", $"{modTexture.id:N}");
                    writer.WriteElementString("CreatedAt", $"{modTexture.createdAt:G}");
                    writer.WriteElementString("Filename", modTexture.filename);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteStartElement("SourceTextures");
                foreach (var sourceTexture in project.sourceTextures.Values)
                {
                    writer.WriteStartElement("SourceTexture");
                    writer.WriteElementString("Id", $"{sourceTexture.id:N}");
                    writer.WriteElementString("SourceFile", sourceTexture.sourceFile);
                    writer.WriteElementString("LocalFile", sourceTexture.localFile);
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
        //
        // public static TextureModProject Load(string file)
        // {
        //     using (var fs = new FileStream(file, FileMode.Open, FileAccess.Read))
        //     {
        //         return (TextureModProject)serializer.Deserialize(fs);
        //     }
        // }
    }

    public class ModTexture
    {
        public Guid id;
        public DateTimeOffset createdAt;
        public Rectangle sourceRect;
        public string filename;

        public Vector3 position;
        public Vector3 lookAtDirection;
    }

    public class SourceTexture
    {
        public Guid id;
        public string sourceFile;
        public string localFile;
    }

    public class TextureReplacement
    {
        public Guid id;
        public Guid modTexture;
        public Guid sourceTexture;
        public Rectangle targetRect;
        public string tag;

        public bool flipX;
        public bool flipY;
        public float rotation;

        public string comment;
    }

    public struct Rectangle
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}