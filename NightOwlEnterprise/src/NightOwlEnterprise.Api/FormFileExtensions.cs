namespace NightOwlEnterprise.Api;

public static class FormFileExtensions
{
    public static string ToBase64String(this IFormFile file)
    {
        using (MemoryStream memoryStream = new MemoryStream())
        {
            // IFormFile'dan gelen veriyi memory stream'e kopyala
            file.CopyTo(memoryStream);

            // Memory stream'i byte dizisine dönüştür
            byte[] bytes = memoryStream.ToArray();

            // Byte dizisini base64 formatına dönüştür
            string base64String = Convert.ToBase64String(bytes);

            return base64String;
        }
    }
}