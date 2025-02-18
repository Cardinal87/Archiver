namespace Archiver.API.DTO.Request
{
    public class MyFileOptions
    {
        public IFormFile? File { get; set; }
        public PdfOptions Options { get; set; } = new PdfOptions();

    }
}
