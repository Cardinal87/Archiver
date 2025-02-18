namespace Archiver.API.DTO
{
    public class MyFileOptions
    {
        public IFormFile? File { get; set; }
        public PdfOptions Options { get; set; } = new PdfOptions();

    }
}
