using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Services
{
    public class ImageService : IImageService
    {
        // don't use a ~ here on the filename but keep leading /
        private readonly string _defaultProjectImageSrc = "/img/noImage.png";
        private readonly string _defaultCompanyImageSrc = "/img/noImage.png";
        private readonly string _defaultUserImageSrc = "/img/noImage.png";
        private readonly string _defaultTicketAttachmentImageSrc = "/img/noImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage)
        {
            if (fileData == null || fileData.Length == 0)
            {
                switch (defaultImage)
                {
                    case 1: return _defaultUserImageSrc;
                    case 2: return _defaultProjectImageSrc;
                    case 3: return _defaultCompanyImageSrc;
                    case 4: return _defaultTicketAttachmentImageSrc;
                }
            }

            try
            {
                string imageBase64Data = Convert.ToBase64String(fileData!);
                string imageSrcString = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageSrcString;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}