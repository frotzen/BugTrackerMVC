using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Services
{
    public class BTFileService : IBTFileService
    {
        // don't use a ~ here on the filename but keep leading '/'
        // private readonly string suffix[] = { "Bytes" }...
        private readonly string _defaultProjectImageSrc = "/img/defaultProjectImage.png";
        private readonly string _defaultCompanyImageSrc = "/img/defaultCompanyImage.png";
        private readonly string _defaultBTUserImageSrc = "/img/defaultUserImage.png";

		private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };


		public string ConvertByteArrayToFile(byte[] fileData, string extension)
		{
            try
            {
				string imageBase64Data = Convert.ToBase64String(fileData);
				return string.Format($"data:image/{extension};base64,{imageBase64Data}");
			}
            catch (Exception)
            {
                throw;
            }
		}

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
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();
                memoryStream.Dispose();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public string GetFileIcon(string file)
        {
            string ext = Path.GetExtension(file).Replace(".", "");
            return $"/img/contenttype/{ext}.png";
        }

        public string FormatFileSize(long bytes)
		{
			int counter = 0;
			decimal number = bytes;
			while (Math.Round(number / 1024) >= 1)
			{
				number /= 1024;
				counter++;
			}
			return string.Format("{0:n1}{1}", number, suffixes[counter]);
		}
	}
}