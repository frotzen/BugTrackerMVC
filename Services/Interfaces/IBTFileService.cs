namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTFileService
    {
        // Needs to be implemented for Interface, No logic, all is public, all
        // methods have to be implemented here for class intended to be a service
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file);

        public string ConvertByteArrayToFile(byte[] fileData, string extension);

        public string ConvertByteArrayToFile(byte[] fileData, string extension, int defaultImage);

        public string GetFileIcon(string file);

        public string FormatFileSize(long bytes);
    }
}
