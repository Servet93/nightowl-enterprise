using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace NightOwlEnterprise.Api;

public class ImageProcessor
{
    public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
    {
        using (MemoryStream inStream = new MemoryStream(imageBytes))
        using (Image image = Image.FromStream(inStream))
        using (Bitmap resizedImage = new Bitmap(width, height))
        {
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                graphics.DrawImage(image, 0, 0, width, height);
            }

            using (MemoryStream outStream = new MemoryStream())
            {
                resizedImage.Save(outStream, ImageFormat.Jpeg); // İstenilen formatı burada belirleyebilirsiniz.
                return outStream.ToArray();
            }
        }
    }
}