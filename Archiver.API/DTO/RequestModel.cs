using System.ComponentModel.DataAnnotations;

namespace Archiver.API.DTO
{
    public class RequestModel
    {
        
        public List<string> HtmlUrls { get; set; } = new List<string>();
        public IFormFileCollection? Images { get; set; }
        public IFormFileCollection? TextFiles { get; set; }

    }
}
