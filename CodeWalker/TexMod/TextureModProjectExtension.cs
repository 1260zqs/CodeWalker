using SharpDX.Direct2D1;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using System.Xml;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace CodeWalker.TexMod;

public static class TextureModProjectExtension
{
    public static void Save(this TextureModProject project, string filename)
    {
        var settings = new XmlWriterSettings
        {
            Indent = true
        };

        using (var writer = XmlWriter.Create(filename, settings))
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("TextureModProject");
            writer.WriteElementString("manifestFile", project.manifestFile);

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
                writer.WriteElementString("Name", modTexture.name);
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

    public static void LoadPackageManifest(this TextureModProject project)
    {
        var manifest = new PackageManifest();
        manifest.Load(project.manifestFile);
        project.manifest = manifest;
    }

    public static void Load(this TextureModProject project, string filename)
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(filename);

        var root = (XmlElement)xmlDoc.SelectSingleNode("/TextureModProject");
        project.manifestFile = root["manifestFile"].InnerText;

        project.replacements.Clear();
        if (root["Replacements"] is XmlElement replacements)
        {
            foreach (XmlElement xmlElement in replacements.GetElementsByTagName("TextureReplacement"))
            {
                var replacement = new TextureReplacement();
                replacement.id = Guid.Parse(xmlElement["Id"].InnerText);
                replacement.modTexture = Guid.Parse(xmlElement["ModTexture"].InnerText);
                replacement.sourceTexture = Guid.Parse(xmlElement["SourceTexture"].InnerText);
                replacement.tag = xmlElement["Tag"].InnerText;
                replacement.flipX = bool.Parse(xmlElement["FlipX"].InnerText);
                replacement.flipY = bool.Parse(xmlElement["FlipY"].InnerText);
                replacement.rotation = int.Parse(xmlElement["Rotation"].InnerText);
                replacement.comment = xmlElement["Comment"].InnerText;
                project.replacements.Add(replacement);
            }
        }
        project.modTextures.Clear();
        if (root["ModTextures"] is XmlElement modtextures)
        {
            foreach (XmlElement xmlElement in modtextures.GetElementsByTagName("ModTexture"))
            {
                var modTexture = new ModTexture();
                modTexture.id = Guid.Parse(xmlElement["Id"].InnerText);
                modTexture.createdAt = DateTime.Parse(xmlElement["CreatedAt"].InnerText);
                modTexture.filename = xmlElement["Filename"].InnerText;
                modTexture.name = xmlElement["Name"].InnerText;
                project.modTextures.Add(modTexture.id, modTexture);
            }
        }
        project.sourceTextures.Clear();
        if (root["SourceTextures"] is XmlElement sourceTextures)
        {
            foreach (XmlElement xmlElement in sourceTextures.GetElementsByTagName("SourceTexture"))
            {
                var sourceTexture = new SourceTexture();
                sourceTexture.id = Guid.Parse(xmlElement["Id"].InnerText);
                sourceTexture.sourceFile = xmlElement["SourceFile"].InnerText;
                sourceTexture.localFile = xmlElement["LocalFile"].InnerText;
                project.sourceTextures.Add(sourceTexture.id, sourceTexture);
            }
        }
    }
}