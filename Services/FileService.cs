using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Services
{
    public class FileService : IFileService
    {
        // don't use a ~ here on the filename but keep leading /
        // private readonly string suffix[] = { "Bytes" }...
        private readonly string _defaultProjectImageSrc = "/img/defaultProjectImage.png";
        private readonly string _defaultCompanyImageSrc = "/img/defaultCompanyImage.png";
        private readonly string _defaultBTUserImageSrc = "/img/defaultUserImage.png";
        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage)
        {
            if (fileData == null || fileData.Length == 0)
            {
                switch (defaultImage)
                {
                    case 1: return _defaultBTUserImageSrc;
                    case 2: return _defaultProjectImageSrc;
                    case 3: return _defaultCompanyImageSrc;
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