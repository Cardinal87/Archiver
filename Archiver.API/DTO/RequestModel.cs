using Archiver.API.Helpers;
using System.ComponentModel.DataAnnotations;

namespace Archiver.API.DTO
{
    public class RequestModel
    {
        
        public List<string> HtmlUrls { get; set; } = new List<string>();

        [AllowedExtensions("jpeg jpg png")]
        public List<MyFileOptions> Images { get; set; } = new();

        [AllowedExtensions("txt csv json xml log config ini html htm css py java cs")]
        public List<MyFileOptions> TextFiles { get; set; } = new();

    }
}
