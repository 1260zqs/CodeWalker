using System.Xml;

namespace CodeWalker.TexMod;

public static class TextureModProjectExtension
{
    public static void Save(this TextureModProject project, string file)
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