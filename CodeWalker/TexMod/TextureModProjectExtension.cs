using CodeWalker.GameFiles;
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

            writer.WriteStartElement("ProjectDirectory");
            WriteProjectDirectory(writer, project.directory);
            writer.WriteEndElement();

            writer.WriteStartElement("TextureMappings");
            foreach (var mapping in project.textureMappings)
            {
                writer.WriteStartElement("TextureMapping");
                writer.WriteAttributeString("Id", $"{mapping.id:N}");
                writer.WriteElementString("Name", mapping.name);
                writer.WriteElementString("Tag", mapping.tag);
                writer.WriteElementString("Lod", $"{mapping.lod}");

                writer.WriteElementString("ModTexture", $"{mapping.modTexture:N}");
                writer.WriteElementString("SourceTexture", $"{mapping.sourceTexture:N}");
                writer.Write("TargetRect", mapping.targetRect);

                writer.WriteElementString("FlipX", $"{mapping.flipX}");
                writer.WriteElementString("FlipY", $"{mapping.flipY}");
                writer.WriteElementString("Swap", $"{mapping.swap}");
                writer.WriteElementString("Rotation", $"{mapping.rotation}");
                writer.WriteElementString("Comment", mapping.comment);
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

    public static void WriteProjectDirectory(XmlWriter writer, Dictionary<string, ProjectDirectory> allDirs)
    {
        foreach (var pair in allDirs)
        {
            writer.WriteStartElement("Directory");
            writer.WriteAttributeString("Path", pair.Key);
            foreach (var file in pair.Value.files)
            {
                writer.WriteElementString("File", $"{file:N}");
            }
            writer.WriteEndElement();
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
                var mapping = new TextureMapping();
                mapping.id = Guid.Parse(xmlElement.Attributes["Id"].InnerText);
                //mapping.tag = xmlElement["Tag"].InnerText;
                mapping.name = xmlElement["Name"].InnerText;
                mapping.comment = xmlElement["Comment"].InnerText;
                mapping.lod = (TextureLod)Enum.Parse(typeof(TextureLod), xmlElement["Lod"].InnerText);

                mapping.modTexture = Guid.Parse(xmlElement["ModTexture"].InnerText);
                mapping.sourceTexture = Guid.Parse(xmlElement["SourceTexture"].InnerText);
                mapping.targetRect = ReadRectangleF(xmlElement["TargetRect"]);

                mapping.flipX = bool.Parse(xmlElement["FlipX"].InnerText);
                mapping.flipY = bool.Parse(xmlElement["FlipY"].InnerText);
                mapping.rotation = int.Parse(xmlElement["Rotation"].InnerText);
                project.textureMappings.Add(mapping);
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
        project.directory.Clear();
        if (root["ProjectDirectory"] is XmlElement projectDirectory)
        {
            foreach (XmlElement xmlElement in projectDirectory.GetElementsByTagName("Directory"))
            {
                var directory = new ProjectDirectory();
                directory.path = xmlElement.Attributes["Path"].InnerText;
                foreach (XmlElement xmlElement2 in xmlElement.GetElementsByTagName("File"))
                {
                    directory.files.Add(Guid.Parse(xmlElement2.InnerText));
                }
                project.directory.Add(directory.path, directory);
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

    public static TextureLod Conv(this rage__eLodType lod)
    {
        return lod switch
        {
            rage__eLodType.LODTYPES_DEPTH_ORPHANHD => TextureLod.HD,
            rage__eLodType.LODTYPES_DEPTH_HD => TextureLod.HD,
            rage__eLodType.LODTYPES_DEPTH_LOD => TextureLod.LOD,
            rage__eLodType.LODTYPES_DEPTH_SLOD1 => TextureLod.SLOD1,
            rage__eLodType.LODTYPES_DEPTH_SLOD2 => TextureLod.SLOD2,
            rage__eLodType.LODTYPES_DEPTH_SLOD3 => TextureLod.SLOD3,
            rage__eLodType.LODTYPES_DEPTH_SLOD4 => TextureLod.SLOD4,
            _ => TextureLod.Unknown
        };
    }

    public static rage__eLodType Conv(this TextureLod lod)
    {
        return lod switch
        {
            TextureLod.HiDR => rage__eLodType.LODTYPES_DEPTH_ORPHANHD,
            TextureLod.HD => rage__eLodType.LODTYPES_DEPTH_HD,
            TextureLod.LOD => rage__eLodType.LODTYPES_DEPTH_LOD,
            TextureLod.SLOD1 => rage__eLodType.LODTYPES_DEPTH_SLOD1,
            TextureLod.SLOD2 => rage__eLodType.LODTYPES_DEPTH_SLOD2,
            TextureLod.SLOD3 => rage__eLodType.LODTYPES_DEPTH_SLOD3,
            TextureLod.SLOD4 => rage__eLodType.LODTYPES_DEPTH_SLOD4,
            _ => rage__eLodType.LODTYPES_DEPTH_HD
        };
    }
}