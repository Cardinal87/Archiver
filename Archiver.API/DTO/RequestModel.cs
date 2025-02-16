using System.ComponentModel.DataAnnotations;

namespace Archiver.API.DTO
{
    public class RequestModel
    {
        
        public List<string> HtmlUrls { get; set; } = new List<string>();

        [FileExtensions(Extensions = "png, jpeg, tiff, bmp")]
        public IFormFileCollection? Pictures { get; set; }

    }
}
