using System;
using System.Runtime.InteropServices;

namespace CodeWalker.Utils;

public static class NVTT
{
    [DllImport("nvtt_compress.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern bool Compress(
        IntPtr data,
        int width,
        int height,
        InputFormat inputFormat,
        Format outputFormat,
        Quality quality,
        out IntPtr outputBuffer,
        out UIntPtr outputSize
    );

    [DllImport("nvtt_compress.dll", CallingConvention = CallingConvention.Cdecl)]
    public static extern void FreeBuffer(IntPtr buffer);
    
    //@formatter:off
    public enum InputFormat
    {
        InputFormat_BGRA_8UB,   ///< [0, 255] 8 bit uint
        InputFormat_BGRA_8SB,	///< [-127, 127] 8 bit int
        InputFormat_RGBA_16F,   ///< 16 bit floating point.
        InputFormat_RGBA_32F,   ///< 32 bit floating point.
        InputFormat_R_32F,      ///< Single channel 32 bit floating point.
    };
    
    public enum Quality
	{
		Quality_Fastest,
		Quality_Normal,
		Quality_Production,
		Quality_Highest,
	};
    
    public enum Format
	{
		// No block-compression (linear).
		Format_RGB,  ///< Linear RGB format
		Format_RGBA = Format_RGB, ///< Linear RGBA format

		// DX9 formats.
		Format_DXT1,    ///< DX9 - DXT1 format
		Format_DXT1a,   ///< DX9 - DXT1 with binary alpha.
		Format_DXT3,    ///< DX9 - DXT3 format
		Format_DXT5,    ///< DX9 - DXT5 format
		Format_DXT5n,   ///< DX9 - DXT5 normal format. Stores a normal (x, y, z) as (R, G, B, A) = (1, y, 0, x).

		// DX10 formats.
		Format_BC1 = Format_DXT1, ///< DX10 - BC1 (DXT1) format
		Format_BC1a = Format_DXT1a, ///< DX10 - BC1 (DXT1) format
		Format_BC2 = Format_DXT3, ///< DX10 - BC2 (DXT3) format
		Format_BC3 = Format_DXT5, ///< DX10 - BC3 (DXT5) format
		Format_BC3n = Format_DXT5n, ///< DX10 - BC3 (DXT5) normal format for improved compression, storing a normal (x, y, z) as (1, y, 0, x).
		Format_BC4,     ///< DX10 - BC4U (ATI1) format (one channel, unsigned)
		Format_BC4S,     ///< DX10 - BC4S format (one channel, signed)
		Format_ATI2,     ///< DX10 - ATI2 format, similar to BC5U, channel order GR instead of RG
		Format_BC5,     ///< DX10 - BC5U format (two channels, unsigned)
		Format_BC5S,     ///< DX10 - BC5S format (two channels, signed)

		Format_DXT1n,   ///< Not supported.
		Format_CTX1,    ///< Not supported.

		Format_BC6U,     ///< DX10 - BC6 format (three-channel HDR, unsigned)
		Format_BC6S,     ///< DX10 - BC6 format (three-channel HDR, signed)

		Format_BC7,     ///< DX10 - BC7 format (four channels, UNORM)

		//Format_BC5_Luma,    // Two DXT alpha blocks encoding a single float.
		/// DX10 - BC3(DXT5) - using a magnitude encoding to approximate
		/// three-channel HDR data in four UNORM channels. The input should be
		/// in the range [0,1], and this should give more accurate values
		/// closer to 0. On most devices, consider using BC6 instead.
		/// 
		/// To decompress this format, decompress it like a standard BC3 texture,
		/// then compute `(R, G, B)` from `(r, g, b, m)` using `fromRGBM()` with
		/// `range = 1` and `threshold = 0.25`:
		/// 
		/// `M = m * 0.75 + 0.25`;
		/// 
		/// `(R, G, B) = (r, g, b) * M`
		/// 
		/// The idea is that since BC3 uses separate compression for the RGB
		/// and alpha blocks, the RGB and M signals can be independent.
		/// Additionally, the compressor can account for the RGB compression
		/// error.
		/// This will print warnings if any of the computed m values were
		/// greater than 1.0.
		Format_BC3_RGBM,

		// 14 ASTC LDR Formats 
		// Added by Fei Yang
		Format_ASTC_LDR_4x4, ///< ASTC - LDR - format, tile size 4x4
		Format_ASTC_LDR_5x4, ///< ASTC - LDR - format, tile size 5x4
		Format_ASTC_LDR_5x5, ///< ASTC - LDR - format, tile size 5x5
		Format_ASTC_LDR_6x5, ///< ASTC - LDR - format, tile size 6x5
		Format_ASTC_LDR_6x6, ///< ASTC - LDR - format, tile size 6x6
		Format_ASTC_LDR_8x5, ///< ASTC - LDR - format, tile size 8x5
		Format_ASTC_LDR_8x6, ///< ASTC - LDR - format, tile size 8x6
		Format_ASTC_LDR_8x8, ///< ASTC - LDR - format, tile size 8x8
		Format_ASTC_LDR_10x5, ///< ASTC - LDR - format, tile size 10x5
		Format_ASTC_LDR_10x6, ///< ASTC - LDR - format, tile size 10x6
		Format_ASTC_LDR_10x8, ///< ASTC - LDR - format, tile size 10x8
		Format_ASTC_LDR_10x10, ///< ASTC - LDR - format, tile size 10x10
		Format_ASTC_LDR_12x10, ///< ASTC - LDR - format, tile size 12x10
		Format_ASTC_LDR_12x12, ///< ASTC - LDR - format, tile size 12x12

		Format_Count,

		/// Placeholder in structs to produce errors if a format is not
		/// explicitly set, since format 0 is Format_RGB.
		Format_Unset = 255
	};
}