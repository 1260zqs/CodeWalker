using SharpDX;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

// ReSharper disable AssignNullToNotNullAttribute

// ReSharper disable PossibleNullReferenceException

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
            WriteProjectDirectory(writer, project.directory);

            writer.WriteStartElement("TextureMappings");
            foreach (var replacement in project.textureMappings)
            {
                writer.WriteStartElement("TextureMapping");
                writer.WriteAttributeString("Id", $"{replacement.id:N}");
                writer.WriteElementString("Name", replacement.name);
                writer.WriteElementString("Tag", replacement.tag);

                writer.WriteElementString("ModTexture", $"{replacement.modTexture:N}");
                writer.WriteElementString("SourceTexture", $"{replacement.sourceTexture:N}");
                writer.Write("TargetRect", replacement.targetRect);

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
                writer.WriteAttributeString("Id", $"{modTexture.id:N}");
                writer.WriteElementString("Name", modTexture.name);
                writer.WriteElementString("CreatedAt", $"{modTexture.createdAt:G}");
                writer.WriteElementString("Filename", modTexture.filename);
                writer.Write("Position", modTexture.position);
                writer.Write("Rotation", modTexture.rotation);
                writer.Write("SourceRect", modTexture.sourceRect);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteStartElement("SourceTextures");
            foreach (var sourceTexture in project.sourceTextures.Values)
            {
                writer.WriteStartElement("SourceTexture");
                writer.WriteAttributeString("Id", $"{sourceTexture.id:N}");
                writer.WriteElementString("SourceFile", sourceTexture.sourceFile);
                writer.WriteElementString("LocalFile", sourceTexture.localFile);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }

    public static void WriteProjectDirectory(XmlWriter writer, ProjectDirectory root)
    {
        var allDirs = new List<ProjectDirectory>();
        CollectDirectories(root, allDirs);

        writer.WriteStartElement("ProjectDirectory");
        foreach (var dir in allDirs)
        {
            writer.WriteStartElement("Directory");
            writer.WriteAttributeString("path", dir.path);
            foreach (var file in dir.files)
            {
                writer.WriteElementString("File", $"{file:N}");
            }
            writer.WriteEndElement();
        }
        writer.WriteEndElement();
    }

    private static void CollectDirectories(ProjectDirectory dir, List<ProjectDirectory> list)
    {
        list.Add(dir);
        foreach (var child in dir.directories)
        {
            CollectDirectories(child, list);
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

        project.textureMappings.Clear();
        if (root["TextureMappings"] is XmlElement replacements)
        {
            foreach (XmlElement xmlElement in replacements.GetElementsByTagName("TextureMapping"))
            {
                var replacement = new TextureMapping();
                replacement.id = Guid.Parse(xmlElement.Attributes["Id"].InnerText);
                replacement.tag = xmlElement["Tag"].InnerText;
                replacement.name = xmlElement["Name"].InnerText;
                replacement.comment = xmlElement["Comment"].InnerText;

                replacement.modTexture = Guid.Parse(xmlElement["ModTexture"].InnerText);
                replacement.sourceTexture = Guid.Parse(xmlElement["SourceTexture"].InnerText);
                replacement.targetRect = ReadRectangleF(xmlElement["TargetRect"]);

                replacement.flipX = bool.Parse(xmlElement["FlipX"].InnerText);
                replacement.flipY = bool.Parse(xmlElement["FlipY"].InnerText);
                replacement.rotation = int.Parse(xmlElement["Rotation"].InnerText);
                project.textureMappings.Add(replacement);
            }
        }
        project.modTextures.Clear();
        if (root["ModTextures"] is XmlElement modtextures)
        {
            foreach (XmlElement xmlElement in modtextures.GetElementsByTagName("ModTexture"))
            {
                var modTexture = new ModTexture();
                modTexture.id = Guid.Parse(xmlElement.Attributes["Id"].InnerText);
                modTexture.createdAt = DateTime.Parse(xmlElement["CreatedAt"].InnerText);
                modTexture.filename = xmlElement["Filename"].InnerText;
                modTexture.name = xmlElement["Name"].InnerText;

                modTexture.position = ReadVector3(xmlElement["Position"]);
                modTexture.rotation = ReadVector3(xmlElement["Rotation"]);
                modTexture.sourceRect = ReadRectangleF(xmlElement["SourceRect"]);

                project.modTextures.Add(modTexture.id, modTexture);
            }
        }
        project.sourceTextures.Clear();
        if (root["SourceTextures"] is XmlElement sourceTextures)
        {
            foreach (XmlElement xmlElement in sourceTextures.GetElementsByTagName("SourceTexture"))
            {
                var sourceTexture = new SourceTexture();
                sourceTexture.id = Guid.Parse(xmlElement.Attributes["Id"].InnerText);
                sourceTexture.sourceFile = xmlElement["SourceFile"].InnerText;
                sourceTexture.localFile = xmlElement["LocalFile"].InnerText;
                project.sourceTextures.Add(sourceTexture.id, sourceTexture);
            }
        }
    }

    private static void Write(this XmlWriter writer, string name, Vector3 vector)
    {
        writer.WriteStartElement(name);
        writer.WriteAttributeString("X", vector.X.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("Y", vector.Y.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("Z", vector.Z.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static void Write(this XmlWriter writer, string name, System.Drawing.RectangleF rectangle)
    {
        writer.WriteStartElement(name);
        writer.WriteAttributeString("X", rectangle.X.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("Y", rectangle.Y.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("Width", rectangle.Width.ToString(CultureInfo.InvariantCulture));
        writer.WriteAttributeString("Height", rectangle.Height.ToString(CultureInfo.InvariantCulture));
        writer.WriteEndElement();
    }

    private static Vector3 ReadVector3(XmlElement element)
    {
        return new Vector3(
            float.Parse(element.GetAttribute("X"), CultureInfo.InvariantCulture),
            float.Parse(element.GetAttribute("Y"), CultureInfo.InvariantCulture),
            float.Parse(element.GetAttribute("X"), CultureInfo.InvariantCulture)
        );
    }

    private static System.Drawing.RectangleF ReadRectangleF(XmlElement element)
    {
        return new System.Drawing.RectangleF(
            float.Parse(element.GetAttribute("X"), CultureInfo.InvariantCulture),
            float.Parse(element.GetAttribute("Y"), CultureInfo.InvariantCulture),
            float.Parse(element.GetAttribute("Width"), CultureInfo.InvariantCulture),
            float.Parse(element.GetAttribute("Height"), CultureInfo.InvariantCulture)
        );
    }
}