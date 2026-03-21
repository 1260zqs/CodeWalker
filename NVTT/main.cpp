#include "pch.h"
#include "framework.h"
#include <vector>
#pragma comment(lib, "nvtt30205.lib")

class MemoryOutputHandler : public nvtt::OutputHandler
{
public:
	std::vector<uint8_t> data;

	void beginImage(int size, int width, int height, int depth, int face, int miplevel) override {}

	bool writeData(const void* buffer, int size) override
	{
		const uint8_t* bytes = (const uint8_t*)buffer;
		data.insert(data.end(), bytes, bytes + size);
		return true;
	}

	void endImage() override {}
};

DLL_EXPORT bool Compress(
	void* data,
	int width,
	int height,
	nvtt::InputFormat inputFormat,
	nvtt::Format outputFormat,
	nvtt::Quality quality,
	void** outputBuffer,
	size_t* outputSize
)
{
	if (outputBuffer == nullptr || outputSize == nullptr)
		return false;

	*outputSize = 0;
	*outputBuffer = nullptr;

	nvtt::Context context;
	nvtt::Surface surface;
	nvtt::CompressionOptions compression;
	surface.setImage(inputFormat, width, height, 1, data);

	compression.setFormat(outputFormat);
	compression.setQuality(quality);

	MemoryOutputHandler handler;
	nvtt::OutputOptions output;

	output.setContainer(nvtt::Container_DDS);
	output.setOutputHandler(&handler);

	int numMipmaps = surface.countMipmaps();
	if (!context.outputHeader(surface, numMipmaps, compression, output))
	{
		return false;
	}
	for (int mip = 0; mip < numMipmaps; mip++)
	{
		if (!context.compress(surface, 0, mip, compression, output))
		{
			return false;
		}
		if (mip == numMipmaps - 1) break;

		// Prepare the next mip:

		// Convert to linear premultiplied alpha. Note that toLinearFromSrgb()
		// will clamp HDR images; consider e.g. toLinear(2.2f) instead.
		surface.toLinearFromSrgb();
		surface.premultiplyAlpha();

		// Resize the image to the next mipmap size.
		// NVTT has several mipmapping filters; Box is the lowest-quality, but
		// also the fastest to use.
		surface.buildNextMipmap(nvtt::MipmapFilter_Box);
		// For general image resizing. use image.resize().

		// Convert back to unpremultiplied sRGB.
		surface.demultiplyAlpha();
		surface.toSrgb();
	}

	*outputBuffer = malloc(handler.data.size());
	if (*outputBuffer)
	{
		memcpy(*outputBuffer, handler.data.data(), handler.data.size());
		*outputSize = handler.data.size();
	}
	return true;
}

DLL_EXPORT void FreeBuffer(void* buffer)
{
	free(buffer);
}