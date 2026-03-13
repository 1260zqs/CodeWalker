using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Xml.Serialization;

namespace CodeWalker
{
    public class TextureModProject
    {
        public string package;
        public List<TextureReplacement> replacements = new List<TextureReplacement>();
        public Dictionary<Guid, ModTexture> modTextures = new Dictionary<Guid, ModTexture>();
        public Dictionary<Guid, SourceTexture> sourceTextures = new Dictionary<Guid, SourceTexture>();

        public static void Save(TextureModProject project, string file)
        {
            // var serializer = new XmlSerializer();
            // using (var fs = new FileStream(file, FileMode.Create, FileAccess.Write))
            // {
            //     serializer.Serialize(fs, project);
            // }
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
        public SourceLocation src;
        public ModLocation local;
    }

    /// <summary>
    /// mod package archive location
    /// </summary>
    public struct ModLocation
    {
        public string path;
        public string filename;
    }

    /// <summary>
    /// game file source location
    /// </summary>
    public struct SourceLocation
    {
        public string archive;
        public string path;
        public string filename;
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