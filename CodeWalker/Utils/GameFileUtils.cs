using CodeWalker.GameFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker.Utils;

internal static class GameFileUtils
{
    public static Texture FindTexture(GameFile file, string texName)
    {
        Texture texture = null;
        var hash = JenkHash.GenHash(texName.ToLowerInvariant());
        if (file is YtdFile ytd)
        {
            texture = ytd.TextureDict.Lookup(hash);
        }
        else if (file is YddFile ydd)
        {
            foreach (var drawable in ydd.Drawables)
            {
                var tex = drawable.ShaderGroup.TextureDictionary.Lookup(hash);
                if (tex != null)
                {
                    texture = tex;
                    break;
                }
            }
        }
        else if (file is YdrFile ydr)
        {
            texture = ydr.Drawable.ShaderGroup.TextureDictionary.Lookup(hash);
        }
        else if (file is YftFile yft)
        {
            texture = yft.Fragment.Drawable.ShaderGroup.TextureDictionary.Lookup(hash);
        }
        return texture;
    }

    public static void ReplaceTexture(GameFile gameFile, Texture texture)
    {
        if (gameFile is YtdFile ytd)
        {
            ReplaceTexture(ytd.TextureDict, texture);
            UpdateEmbeddedTextures(gameFile);
        }
        else if (gameFile is YddFile ydd)
        {
            foreach (var drawable in ydd.Drawables)
            {
                ReplaceTexture(drawable.ShaderGroup.TextureDictionary, texture);
            }
            UpdateEmbeddedTextures(gameFile);
        }
        else if (gameFile is YdrFile ydr)
        {
            ReplaceTexture(ydr.Drawable.ShaderGroup.TextureDictionary, texture);
            UpdateEmbeddedTextures(gameFile);
        }
        else if (gameFile is YftFile yft)
        {
            ReplaceTexture(yft.Fragment.Drawable.ShaderGroup.TextureDictionary, texture);
            UpdateEmbeddedTextures(gameFile);
        }
    }

    public static void UpdateEmbeddedTextures(GameFile file)
    {
        if (!file.Loaded) return;
        switch (file)
        {
            case YdrFile ydr:
            {
                UpdateEmbeddedTextures(ydr.Drawable);
                break;
            }
            case YddFile ydd:
            {
                foreach (var kvp in ydd.Dict)
                {
                    UpdateEmbeddedTextures(kvp.Value);
                }
                break;
            }
            case YptFile ypt:
            {
                if (ypt.DrawableDict != null)
                {
                    foreach (var kvp in ypt.DrawableDict)
                    {
                        UpdateEmbeddedTextures(kvp.Value);
                    }
                }
                break;
            }
            case YftFile yft:
            {
                if (yft.Fragment != null)
                {
                    var f = yft.Fragment;
                    UpdateEmbeddedTextures(f.Drawable);
                    UpdateEmbeddedTextures(f.DrawableCloth);

                    if (f.DrawableArray?.data_items != null)
                    {
                        foreach (var d in f.DrawableArray.data_items)
                        {
                            UpdateEmbeddedTextures(d);
                        }
                    }
                    var c = f.PhysicsLODGroup?.PhysicsLOD1?.Children?.data_items;
                    if (c != null)
                    {
                        foreach (var child in c)
                        {
                            if (child != null)
                            {
                                UpdateEmbeddedTextures(child.Drawable1);
                                UpdateEmbeddedTextures(child.Drawable2);
                            }
                        }
                    }
                }
                break;
            }
        }
    }

    public static bool ReplaceTexture(TextureDictionary dictionary, Texture texture)
    {
        var textures = dictionary.Textures.data_items;
        for (var i = textures.Length - 1; i >= 0; i--)
        {
            if (textures[i].NameHash == texture.NameHash)
            {
                textures[i] = texture;
                return true;
            }
        }
        return false;
    }

    public static void UpdateEmbeddedTextures(DrawableBase dwbl)
    {
        if (dwbl == null) return;

        var sg = dwbl.ShaderGroup;
        var td = sg?.TextureDictionary;
        var sd = sg?.Shaders?.data_items;

        if (td == null) return;
        if (sd == null) return;

        var updated = false;
        foreach (var s in sd)
        {
            if (s?.ParametersList == null) continue;
            foreach (var p in s.ParametersList.Parameters)
            {
                if (p.Data is TextureBase tex)
                {
                    var tex2 = td.Lookup(tex.NameHash);
                    if (tex2 != null && tex != tex2)
                    {
                        p.Data = tex2; //swap the parameter out for the new embedded texture
                        updated = true;
                    }
                }
            }
        }

        if (!updated) return;

        foreach (var model in dwbl.AllModels)
        {
            if (model?.Geometries == null) continue;
            foreach (var geom in model.Geometries)
            {
                geom.UpdateRenderableParameters = true;
            }
        }
    }

    public static GameFileType GetFileTypeByExtension(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".ydd" => GameFileType.Ydd,
            ".ydr" => GameFileType.Ydr,
            ".yft" => GameFileType.Yft,
            ".ymap" => GameFileType.Ymap,
            ".ymf" => GameFileType.Ymf,
            ".ymt" => GameFileType.Ymt,
            ".ytd" => GameFileType.Ytd,
            ".ytyp" => GameFileType.Ytyp,
            ".ybn" => GameFileType.Ybn,
            ".ycd" => GameFileType.Ycd,
            ".ypt" => GameFileType.Ypt,
            ".ynd" => GameFileType.Ynd,
            ".ynv" => GameFileType.Ynv,
            ".rel" => GameFileType.Rel,
            ".ywr" => GameFileType.Ywr,
            ".yvr" => GameFileType.Yvr,
            ".gtxd" => GameFileType.Gtxd,

            // XML/meta types
            ".vehicles" => GameFileType.Vehicles,
            ".carcols" => GameFileType.CarCols,
            ".carmodcols" => GameFileType.CarModCols,
            ".carvariations" => GameFileType.CarVariations,
            ".vehiclelayouts" => GameFileType.VehicleLayouts,
            ".peds" => GameFileType.Peds,
            ".ped" => GameFileType.Ped,

            // less common but valid
            ".yed" => GameFileType.Yed,
            ".yld" => GameFileType.Yld,
            ".yfd" => GameFileType.Yfd,
            ".heightmap" => GameFileType.Heightmap,
            ".watermap" => GameFileType.Watermap,
            ".mrf" => GameFileType.Mrf,
            ".distantlights" => GameFileType.DistantLights,
            ".ypdb" => GameFileType.Ypdb,
            _ => (GameFileType)(-1)
        };
    }

    public static GameFile GetFileFromCache(GameFileCache cache, RpfEntry rpfEntry)
    {
        var ext = Path.GetExtension(rpfEntry.Path);
        var filetype = GetFileTypeByExtension(ext);
        return filetype switch
        {
            GameFileType.Ydr => cache.GetYdr(rpfEntry.ShortNameHash),
            GameFileType.Ydd => cache.GetYdd(rpfEntry.ShortNameHash),
            GameFileType.Ytd => cache.GetYtd(rpfEntry.ShortNameHash),
            GameFileType.Ymap => cache.GetYmap(rpfEntry.ShortNameHash),
            GameFileType.Yft => cache.GetYft(rpfEntry.ShortNameHash),
            GameFileType.Ybn => cache.GetYbn(rpfEntry.ShortNameHash),
            GameFileType.Ycd => cache.GetYcd(rpfEntry.ShortNameHash),
            GameFileType.Yed => cache.GetYed(rpfEntry.ShortNameHash),
            GameFileType.Ynv => cache.GetYnv(rpfEntry.ShortNameHash),
            _ => null
        };
    }

    public static GameFile CreateFileObject(GameFileType filetype)
    {
        return filetype switch
        {
            GameFileType.Ydr => new YdrFile(),
            GameFileType.Ydd => new YddFile(),
            GameFileType.Ytd => new YtdFile(),
            GameFileType.Ymap => new YmapFile(),
            GameFileType.Yft => new YftFile(),
            GameFileType.Ybn => new YbnFile(),
            GameFileType.Ycd => new YcdFile(),
            GameFileType.Yed => new YedFile(),
            GameFileType.Ynv => new YnvFile(),
            GameFileType.Ytyp => new YtypFile(),
            GameFileType.Ypt => new YptFile(),
            GameFileType.Ynd => new YndFile(),
            GameFileType.Rel => new RelFile(),
            GameFileType.Ywr => new YwrFile(),
            GameFileType.Yvr => new YvrFile(),
            GameFileType.Gtxd => new GtxdFile(),

            // meta/xml types
            GameFileType.Vehicles => new VehiclesFile(),
            GameFileType.CarCols => new CarColsFile(),
            GameFileType.CarModCols => new CarModColsFile(),
            GameFileType.CarVariations => new CarVariationsFile(),
            GameFileType.VehicleLayouts => new VehicleLayoutsFile(),
            GameFileType.Peds => new PedsFile(),
            GameFileType.Ped => new PedFile(),

            // other formats
            GameFileType.Yld => new YldFile(),
            GameFileType.Yfd => new YfdFile(),
            GameFileType.Heightmap => new HeightmapFile(),
            GameFileType.Watermap => new WatermapFile(),
            GameFileType.Mrf => new MrfFile(),
            GameFileType.DistantLights => new DistantLightsFile(),
            GameFileType.Ypdb => new YpdbFile(),
            _ => null
        };
    }

    public static GameFile LoadFile(string filepath)
    {
        var filename = Path.GetFileName(filepath);
        var extension = Path.GetExtension(filename);

        var filetype = GetFileTypeByExtension(extension);
        var file = CreateFileObject(filetype);

        if (file is PackedFile packedFile)
        {
            var bytes = File.ReadAllBytes(filepath);
            var entry = CreateFileEntry(filename, filepath, ref bytes);
            packedFile.Load(bytes, entry);
        }
        return file;
    }

    public static RpfFileEntry CreateFileEntry(string name, string path, ref byte[] data)
    {
        //this should only really be used when loading a file from the filesystem.
        RpfFileEntry e = null;
        uint rsc7 = (data?.Length > 4) ? BitConverter.ToUInt32(data, 0) : 0;
        if (rsc7 == 0x37435352) //RSC7 header present! create RpfResourceFileEntry and decompress data...
        {
            e = RpfFile.CreateResourceFileEntry(ref data, 0); //"version" should be loadable from the header in the data..
            data = ResourceBuilder.Decompress(data);
        }
        else
        {
            var be = new RpfBinaryFileEntry();
            be.FileSize = (uint)data?.Length;
            be.FileUncompressedSize = be.FileSize;
            e = be;
        }
        e.Name = name;
        e.NameLower = name?.ToLowerInvariant();
        e.NameHash = JenkHash.GenHash(e.NameLower);
        e.ShortNameHash = JenkHash.GenHash(Path.GetFileNameWithoutExtension(e.NameLower));
        e.Path = path;
        return e;
    }
}