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
            texture = ytd.TextureDict?.Lookup(hash);
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
        }
        return texture;
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
            e = RpfFile.CreateResourceFileEntry(ref data, 0);//"version" should be loadable from the header in the data..
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
