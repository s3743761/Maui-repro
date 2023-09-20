
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Storage;
using LibHeifSharp;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Metadata.Profiles.Icc;
using SixLabors.ImageSharp.Metadata.Profiles.Xmp;
using SixLabors.ImageSharp.PixelFormats;
using System.Diagnostics;
using System.Reflection;
using Image = SixLabors.ImageSharp.Image;

namespace MauiApp1
{
    class Decoder
    {
        private static readonly Lazy<char[]> InvalidFileNameChars = new(Path.GetInvalidFileNameChars);

        public async Task Main(Stream inputPath)
        {


            bool extractDepthImages = false;
            bool extractThumbnailImages = false;
            bool extractVendorAuxiliaryImages = false;
            bool extractPrimaryImage = true;
            string decoderId = null;
            string chromaUpsampling = null;
            //bool listDecoders = true;
            bool convertHdrToEightBit = true;
            bool strict = true;
            //bool showHelp = false;
            //bool showVersion = true;

            //var options = new OptionSet
            //{
            //    "Usage: heif-dec [OPTIONS] input.heif output.png",
            //    "",
            //    "Options:",
            //    { "p|primary", "Extract the primary image (default: extract all top-level images).", (v) => extractPrimaryImage = v != null },
            //    { "d|depth", "Extract the depth images (if present).", (v) => extractDepthImages = v != null },
            //    { "t|thumb", "Extract the thumbnail images (if present).", (v) => extractThumbnailImages = v != null },
            //    { "x|vendor-auxiliary", "Extract the vendor-specific auxiliary images (if present).", (v) => extractVendorAuxiliaryImages = v != null },
            //    { "decoder=", "Use a specific decoder. See the list-decoders option.", (v) => decoderId = v },
            //    { "list-decoders", "Show a list of the available decoders.", (v) => listDecoders = v != null },
            //    { "C|chroma-upsampling=", "Force chroma upsampling algorithm (nearest-neighbor / bilinear).", (v) => chromaUpsampling = v },
            //    { "no-hdr", "Convert HDR images to 8 bits-per-channel.", (v) => convertHdrToEightBit = v != null },
            //    { "s|strict", "Return an error for invalid inputs.", (v) => strict = v != null },
            //    { "h|help", "Print out this message and exit.", (v) => showHelp = v != null },
            //    { "v|version", "Print out the application and library version information and exit.", (v) => showVersion = v != null }
            //};



            //var remaining = options.Parse(args);

           
                PrintVersionInfo();
                //return;
            
                if (LibHeifInfo.HaveVersion(1, 15, 0))
                {
                    Console.WriteLine("HERE2____________________");
                    ListDecoders();
                }
                else
                {
                    Console.WriteLine("HERE2____________________2");
                    Console.WriteLine("The list-decoders option requires LibHeif version 1.15.0 or later.");
                }
                //return;
           
                if (LibHeifInfo.HaveVersion(1, 15, 0))
                {
                    var decoderDescriptors = LibHeifInfo.GetDecoderDescriptors();
                    bool isValidDecoderId = false;

                    foreach (var descriptor in decoderDescriptors)
                    {
                        //if (decoderId.Equals(descriptor.IdName, StringComparison.OrdinalIgnoreCase))
                        //{
                            isValidDecoderId = true;
                            break;
                        //}
                    }

                    if (!isValidDecoderId)
                    {
                        Console .WriteLine("Invalid decoder ID, please choose one from the list below:");
                        PrintDecoderList(decoderDescriptors);
                        //return;
                    }
                }
                else
                {
                    Console.WriteLine("The decoder option will be ignored, it requires LibHeif version 1.15.0 or later.");
                }
            

            //if (showHelp || remaining.Count != 2)
            //{
            //    options.WriteOptionDescriptions(Console.Out);
            //    return;
            //}

            //string inputPath = inputPath;
            //string outputPath = outputPath;

            try
            {
                Console.WriteLine(decoderId);
                var decodingOptions = new HeifDecodingOptions
                {
                    ConvertHdrToEightBit = convertHdrToEightBit,
                    Strict = strict,
                    DecoderId = decoderId
                };

                if (!string.IsNullOrWhiteSpace(chromaUpsampling))
                {
                    if (LibHeifInfo.HaveVersion(1, 16, 0))
                    {
                        if (chromaUpsampling.Equals("nearest-neighbor", StringComparison.OrdinalIgnoreCase))
                        {
                            decodingOptions.ColorConversionOptions.PreferredChromaUpsamplingAlgorithm = HeifChromaUpsamplingAlgorithm.NearestNeighbor;
                        }
                        else if (chromaUpsampling.Equals("bilinear", StringComparison.OrdinalIgnoreCase))
                        {
                            decodingOptions.ColorConversionOptions.PreferredChromaUpsamplingAlgorithm = HeifChromaUpsamplingAlgorithm.Bilinear;
                        }
                        else
                        {
                            Console.WriteLine("Invalid chroma upsampling value, it must either nearest-neighbor or bilinear.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("The chroma upsampling option will be ignored, it requires LibHeif 1.16.0 or later.");
                    }
                }

                using (var context = new HeifContext(inputPath))
                {
                    if (extractPrimaryImage)
                    {
                        Console.WriteLine("herenew");
                        using (var primaryImage = context.GetPrimaryImageHandle())
                        {
                            Console.WriteLine("nerw");
                            await ProcessImageHandle(primaryImage,
                                               decodingOptions,
                                               extractDepthImages,
                                               extractThumbnailImages,
                                               extractVendorAuxiliaryImages
                                               );
                        }
                    }
                    else
                    {
                        var topLevelImageIds = context.GetTopLevelImageIds();

                        //string imageFileName = AddSuffixToFileName( "-{0}");

                        for (int i = 0; i < topLevelImageIds.Count; i++)
                        {
                            using (var imageHandle = context.GetImageHandle(topLevelImageIds[i]))
                            {
                                await ProcessImageHandle(imageHandle,
                                                   decodingOptions,
                                                   extractDepthImages,
                                                   extractThumbnailImages,
                                                   extractVendorAuxiliaryImages
                                               );
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        static void ListDecoders()
        {
            Console.WriteLine("HEVC decoders:");
            PrintDecoderList(LibHeifInfo.GetDecoderDescriptors(HeifCompressionFormat.Hevc));
            Console.WriteLine("AV1 decoders:");
            PrintDecoderList(LibHeifInfo.GetDecoderDescriptors(HeifCompressionFormat.Av1));
        }

        static void PrintDecoderList(IReadOnlyList<HeifDecoderDescriptor> decoderDescriptors)
        {
            Console.WriteLine("LiST______________________________________");
            for (int i = 0; i < decoderDescriptors.Count; i++)
            {
                var decoderDescriptor = decoderDescriptors[i];

                Console.WriteLine("{0} = {1}", decoderDescriptor.IdName, decoderDescriptor.Name);
            }
        }

        static void PrintVersionInfo()
        {
            Console.WriteLine("heif-dec {0} LibHeifSharp v{1} libheif v{2}",
                                GetAssemblyFileVersion(typeof(Decoder)),
                              GetAssemblyFileVersion(typeof(LibHeifInfo)),
                              LibHeifInfo.Version.ToString(3));

            static string GetAssemblyFileVersion(Type type)
            {
                var fileVersionAttribute = type.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();

#pragma warning disable IDE0270 // Use coalesce expression
                if (fileVersionAttribute is null)
                {
                    throw new InvalidOperationException($"Failed to get the AssemblyFileVersion for {type.Assembly.FullName}.");
                }
#pragma warning restore IDE0270 // Use coalesce expression

                var trimmedVersion = new Version(fileVersionAttribute.Version);

                return trimmedVersion.ToString(3);
            }
        }

        //static string AddSuffixToFileName(string path, string suffix)
        //{
        //    string outputDir = Path.GetDirectoryName(path);
        //    string fileName = Path.GetFileNameWithoutExtension(path);
        //    string extension = Path.GetExtension(path);

        //    return Path.Combine(outputDir, fileName + suffix + extension);
        //}

        static string SanitizeFileName(string fileName)
        {
            char[] invalidChars = InvalidFileNameChars.Value;

            foreach (char invalid in invalidChars)
            {
                fileName = fileName.Replace(invalid, '_');
            }

            return fileName;
        }

        static unsafe Image CreateEightBitImageWithAlpha(HeifImage heifImage, bool premultiplied)
        {
            var image = new Image<Rgba32>(heifImage.Width, heifImage.Height);

            var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

            byte* srcScan0 = (byte*)heifPlaneData.Scan0;
            int srcStride = heifPlaneData.Stride;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    byte* src = srcScan0 + (y * srcStride);
                    var dst = accessor.GetRowSpan(y);

                    for (int x = 0; x < accessor.Width; x++)
                    {
                        ref var pixel = ref dst[x];

                        if (premultiplied)
                        {
                            byte alpha = src[3];

                            switch (alpha)
                            {
                                case 0:
                                    pixel.R = 0;
                                    pixel.G = 0;
                                    pixel.B = 0;
                                    break;
                                case 255:
                                    pixel.R = src[0];
                                    pixel.G = src[1];
                                    pixel.B = src[2];
                                    break;
                                default:
                                    pixel.R = (byte)Math.Min(MathF.Round(src[0] * 255f / alpha), 255);
                                    pixel.G = (byte)Math.Min(MathF.Round(src[1] * 255f / alpha), 255);
                                    pixel.B = (byte)Math.Min(MathF.Round(src[2] * 255f / alpha), 255);
                                    break;
                            }
                        }
                        else
                        {
                            pixel.R = src[0];
                            pixel.G = src[1];
                            pixel.B = src[2];
                        }
                        pixel.A = src[3];

                        src += 4;
                    }
                }
            });

            return image;
        }

        static unsafe Image CreateEightBitImageWithoutAlpha(HeifImage heifImage)
        {
            var image = new Image<Rgb24>(heifImage.Width, heifImage.Height);

            var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

            byte* srcScan0 = (byte*)heifPlaneData.Scan0;
            int srcStride = heifPlaneData.Stride;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    byte* src = srcScan0 + (y * srcStride);
                    var dst = accessor.GetRowSpan(y);

                    for (int x = 0; x < accessor.Width; x++)
                    {
                        ref var pixel = ref dst[x];

                        pixel.R = src[0];
                        pixel.G = src[1];
                        pixel.B = src[2];

                        src += 3;
                    }
                }
            });

            return image;
        }

        static unsafe Image CreateSixteenBitImageWithAlpha(HeifImage heifImage, bool premultiplied, int bitDepth)
        {
            var image = new Image<Rgba64>(heifImage.Width, heifImage.Height);

            var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

            byte* srcScan0 = (byte*)heifPlaneData.Scan0;
            int srcStride = heifPlaneData.Stride;

            int maxChannelValue = (1 << bitDepth) - 1;
            float maxChannelValueFloat = maxChannelValue;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    ushort* src = (ushort*)(srcScan0 + (y * srcStride));
                    var dst = accessor.GetRowSpan(y);

                    for (int x = 0; x < accessor.Width; x++)
                    {
                        ref var pixel = ref dst[x];

                        if (premultiplied)
                        {
                            ushort alpha = src[3];

                            if (alpha == maxChannelValue)
                            {
                                pixel.R = src[0];
                                pixel.G = src[1];
                                pixel.B = src[2];
                            }
                            else
                            {
                                switch (alpha)
                                {
                                    case 0:
                                        pixel.R = 0;
                                        pixel.G = 0;
                                        pixel.B = 0;
                                        break;
                                    default:
                                        pixel.R = (ushort)Math.Min(MathF.Round(src[0] * maxChannelValueFloat / alpha), maxChannelValue);
                                        pixel.G = (ushort)Math.Min(MathF.Round(src[1] * maxChannelValueFloat / alpha), maxChannelValue);
                                        pixel.B = (ushort)Math.Min(MathF.Round(src[2] * maxChannelValueFloat / alpha), maxChannelValue);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            pixel.R = src[0];
                            pixel.G = src[1];
                            pixel.B = src[2];
                        }
                        pixel.A = src[3];

                        src += 4;
                    }
                }
            });

            return image;
        }

        static unsafe Image CreateSixteenBitImageWithoutAlpha(HeifImage heifImage)
        {
            var image = new Image<Rgb48>(heifImage.Width, heifImage.Height);

            var heifPlaneData = heifImage.GetPlane(HeifChannel.Interleaved);

            byte* srcScan0 = (byte*)heifPlaneData.Scan0;
            int srcStride = heifPlaneData.Stride;

            image.ProcessPixelRows(accessor =>
            {
                for (int y = 0; y < accessor.Height; y++)
                {
                    ushort* src = (ushort*)(srcScan0 + (y * srcStride));
                    var dst = accessor.GetRowSpan(y);

                    for (int x = 0; x < accessor.Width; x++)
                    {
                        ref var pixel = ref dst[x];

                        pixel.R = src[0];
                        pixel.G = src[1];
                        pixel.B = src[2];

                        src += 3;
                    }
                }
            });

            return image;
        }

        static async Task ProcessImageHandle(HeifImageHandle imageHandle,
                                       HeifDecodingOptions decodingOptions,
                                       bool extractDepthImages,
                                       bool extractThumbnailImages,
                                       bool extractVendorAuxiliaryImages
                                   )
        {
           await WriteOutputImage(imageHandle, decodingOptions);

            if (extractDepthImages)
            {
                if (imageHandle.HasDepthImage)
                {
                    var depthImageIds = imageHandle.GetDepthImageIds();

                    //string depthImageFileName;

                    if (depthImageIds.Count == 1)
                    {
                        //depthImageFileName = AddSuffixToFileName(outputPath, "-depth");

                        using (var depthImageHandle = imageHandle.GetDepthImage(depthImageIds[0]))
                        {
                            //Task.Run(() => WriteOutputImage(depthImageHandle, decodingOptions));
                            await WriteOutputImage(depthImageHandle, decodingOptions);
                        }
                    }
                    else
                    {
                        //depthImageFileName = AddSuffixToFileName(outputPath, "-depth-{0}");

                        for (int i = 0; i < depthImageIds.Count; i++)
                        {
                            using (var depthImageHandle = imageHandle.GetDepthImage(depthImageIds[i]))
                            {
                                await WriteOutputImage(depthImageHandle, decodingOptions);
                            }
                        }
                    }
                }
            }

            if (extractThumbnailImages)
            {
                var thumbnailImageIds = imageHandle.GetThumbnailImageIds();

                if (thumbnailImageIds.Count > 0)
                {
                    string thumbnailFileName;

                    if (thumbnailImageIds.Count == 1)
                    {
                        //thumbnailFileName = AddSuffixToFileName(outputPath, "-thumb");
                        using (var thumbnailImageHandle = imageHandle.GetThumbnailImage(thumbnailImageIds[0]))
                        {
                            await WriteOutputImage(thumbnailImageHandle, decodingOptions);
                        }
                    }
                    else
                    {
                        //thumbnailFileName = AddSuffixToFileName(outputPath, "-thumb-{0}");

                        for (int i = 0; i < thumbnailImageIds.Count; i++)
                        {
                            using (var thumbnailImageHandle = imageHandle.GetThumbnailImage(thumbnailImageIds[i]))
                            {
                                await WriteOutputImage(thumbnailImageHandle, decodingOptions);
                            }
                        }
                    }
                }
            }

            if (extractVendorAuxiliaryImages)
            {
                var vendorAuxImageIds = imageHandle.GetAuxiliaryImageIds();

                if (vendorAuxImageIds.Count > 0)
                {
                    string vendorAuxFileName;

                    if (vendorAuxImageIds.Count == 1)
                    {
                        using (var vendorAuxImageHandle = imageHandle.GetAuxiliaryImage(vendorAuxImageIds[0]))
                        {
                            string type = vendorAuxImageHandle.GetAuxiliaryType();

                            //vendorAuxFileName = AddSuffixToFileName(outputPath, "-" + SanitizeFileName(type));
                            await WriteOutputImage(vendorAuxImageHandle, decodingOptions);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < vendorAuxImageIds.Count; i++)
                        {
                            using (var vendorAuxImageHandle = imageHandle.GetThumbnailImage(vendorAuxImageIds[i]))
                            {
                                string type = vendorAuxImageHandle.GetAuxiliaryType();

                                //vendorAuxFileName = AddSuffixToFileName(outputPath, string.Format(CultureInfo.CurrentCulture,
                                //                                                                  "-{0}-{1}",
                                //                                                                  SanitizeFileName(type),
                                //                                                                  i));

                                await WriteOutputImage(vendorAuxImageHandle, decodingOptions);
                            }
                        }
                    }
                }
            }
        }
    

        static async Task WriteOutputImage(HeifImageHandle imageHandle, HeifDecodingOptions decodingOptions)
        {
            Image outputImage;
            Console.WriteLine("HERE00___________________________________________________________________________________");
            HeifChroma chroma;
            Console.WriteLine("HERE02___________________________________________________________________________________");
            bool hasAlpha = imageHandle.HasAlphaChannel;
            int bitDepth = imageHandle.BitDepth;
            Console.WriteLine("HERE01___________________________________________________________________________________" + bitDepth);
            if (bitDepth == 8 || decodingOptions.ConvertHdrToEightBit)
            {
                chroma = hasAlpha ? HeifChroma.InterleavedRgba32 : HeifChroma.InterleavedRgb24;
            }
            else
            {
                // Use the native byte order of the operating system.
                if (BitConverter.IsLittleEndian)
                {
                    chroma = hasAlpha ? HeifChroma.InterleavedRgba64LE : HeifChroma.InterleavedRgb48LE;
                }
                else
                {
                    chroma = hasAlpha ? HeifChroma.InterleavedRgba64BE : HeifChroma.InterleavedRgb48BE;
                }
            }
            Console.WriteLine("HERE12___________________________________________________________________________________");
            var i = imageHandle.Decode(HeifColorspace.Rgb, chroma, decodingOptions);
           
            using (var image = imageHandle.Decode(HeifColorspace.Rgb, chroma, decodingOptions))
            {
                Console.WriteLine("new");
                var decodingWarnings = image.DecodingWarnings;

                foreach (var item in decodingWarnings)
                {
                    Console.WriteLine("Warning: " + item);
                }

                switch (chroma)
                {
                    case HeifChroma.InterleavedRgb24:
                        outputImage = CreateEightBitImageWithoutAlpha(image);
                        break;
                    case HeifChroma.InterleavedRgba32:
                        outputImage = CreateEightBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha);
                        break;
                    case HeifChroma.InterleavedRgb48BE:
                    case HeifChroma.InterleavedRgb48LE:
                        outputImage = CreateSixteenBitImageWithoutAlpha(image);
                        break;
                    case HeifChroma.InterleavedRgba64BE:
                    case HeifChroma.InterleavedRgba64LE:
                        outputImage = CreateSixteenBitImageWithAlpha(image, imageHandle.IsPremultipliedAlpha, imageHandle.BitDepth);
                        break;
                    default:
                        throw new InvalidOperationException("Unsupported HeifChroma value.");
                }

                if (image.IccColorProfile != null)
                {
                    outputImage.Metadata.IccProfile = new IccProfile(image.IccColorProfile.GetIccProfileBytes());
                }
            }
            Console.WriteLine("HERE13___________________________________________________________________________________");
            byte[] exif = imageHandle.GetExifMetadata();

            if (exif != null)
            {
                outputImage.Metadata.ExifProfile = new ExifProfile(exif);
                // The HEIF specification states that the EXIF orientation tag is only
                // informational and should not be used to rotate the image.
                // See https://github.com/strukturag/libheif/issues/227#issuecomment-642165942
                outputImage.Metadata.ExifProfile.RemoveValue(ExifTag.Orientation);
            }
            Console.WriteLine("HERE14___________________________________________________________________________________");

            byte[] xmp = imageHandle.GetXmpMetadata();

            if (xmp != null)
            {
                outputImage.Metadata.XmpProfile = new XmpProfile(xmp);
            }
            Console.WriteLine("HERE15___________________________________________________________________________________");
            using (var stream = new MemoryStream())
            {
                Console.WriteLine("newinside");

                await outputImage.SaveAsync(stream, new PngEncoder());
                stream.Seek(0, SeekOrigin.Begin);
                Console.WriteLine("newinside2");

                var fileSaverResult = await FileSaver.Default.SaveAsync("test.png", stream, new CancellationToken());
                if (fileSaverResult.IsSuccessful)
                {
                  
                    await Toast.Make("New ISOSpeed tag with val 200 added (temporarily)", ToastDuration.Long).Show(new CancellationToken());
                   
                }
                else
                {
                    Console.WriteLine("Exception: " + fileSaverResult.Exception.Message);
                    await Toast.Make($"The file was not saved successfully with error: {fileSaverResult.Exception.Message}", ToastDuration.Long).Show(new CancellationToken());
                  
                }
                //outputImage.SaveAsPng(outputPath);
            }
        }
    }
}
