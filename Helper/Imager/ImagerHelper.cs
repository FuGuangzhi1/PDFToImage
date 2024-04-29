using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

using System.IO.Compression;

namespace WebApplication1.Helper.Imager;

public static class ImagerHelper
{
    public static byte[] FrameCombine(this byte[][] bytes, int fps)
    {
        Image? firstFrame = null;
        int delay = 100 / fps; //根据帧率技术延迟
        GifDisposalMethod disposalMethod = GifDisposalMethod.NotDispose; //背景处理方式

        for (int i = 0; i < bytes.Length; i++)
        {
            Image tempImage = Image.Load(bytes[i]);
            if (i == 0) //第一帧做底图
            {
                firstFrame = tempImage;
                firstFrame.Frames.RootFrame.Metadata.GetGifMetadata().FrameDelay = delay;
                firstFrame.Metadata.GetGifMetadata().RepeatCount = 0;
            }
            else
            {
                //把其他帧合到第一帧上
                _ = firstFrame!.Frames.AddFrame(tempImage.Frames.RootFrame);
                GifFrameMetadata meta = firstFrame.Frames[i].Metadata.GetGifMetadata();
                meta.FrameDelay = delay;
                meta.DisposalMethod = disposalMethod;
            }
        }
        MemoryStream ms = new();
        firstFrame.SaveAsGif(ms);

        return ms.ToArray();
    }
    public static async Task<byte[]> MergeImages(this byte[][] imageArray)
    {
        int width = 0;
        int height = 0;
        int index = 0;

        Image[] images = new Image[imageArray.Length];

        foreach (byte[] imageData in imageArray)
        {
            Image image = Image.Load(imageData);
            height += image.Height;
            width = image.Width > width ? image.Width : width;
            images[index] = image;
            index++;
        }
        using Image<Rgba32> mergedImage = new(width, height);

        int yOffset = 0;

        foreach (Image image in images)
        {
            mergedImage.Mutate(ctx => ctx.DrawImage(image, new Point(0, yOffset), 1));

            yOffset += image.Height;
            image.Dispose();
        }

        using MemoryStream ms = new();
        await mergedImage.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
    public static byte[] CompressToZip(this byte[][] imageArray, int quality)
    {
        using MemoryStream memoryStream = new();
        using (ZipArchive archive = new(memoryStream, ZipArchiveMode.Create, true))
        {
            for (int i = 0; i < imageArray.Length; i++)
            {
                byte[] imageData = imageArray[i];
                string entryName = $"image{i + 1}.jpg";

                ZipArchiveEntry entry = archive.CreateEntry(entryName);

                using Stream entryStream = entry.Open();
                using MemoryStream imageStream = new(imageData);
                using Image image = Image.Load(imageStream);
                SixLabors.ImageSharp.Formats.Jpeg.JpegEncoder encoder = new()
                {
                    Quality = quality
                };

                image.Save(entryStream, encoder);
            }
        }

        return memoryStream.ToArray();
    }

}
