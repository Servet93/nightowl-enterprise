using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

namespace NightOwlEnterprise.Api;

public class ImageProcessor
{
    public static byte[] ResizeImage(MemoryStream inStream, int width, int height)
    {
        using (MemoryStream outStream = new MemoryStream())
        {
            using (var image = Image.Load(inStream))
            {
                image.Mutate(x => x.Resize(width, height));
                image.Save(outStream, new JpegEncoder());
            }
            
            return outStream.ToArray();
        }
    }
    
}