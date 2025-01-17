namespace Al_Amal
{
    public class AttachmentsOptions
    {
        public string AllowedExtensions { get; set; }
        public int MaxSizeInImageInKB { get; set; }
        public int MaxSizeInVideoInKB { get; set; }
        public string UploadPath { get; set; }
        public bool EnableCompression { get; set; }
    }
}
