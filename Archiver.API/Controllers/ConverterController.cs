using Archiver.API.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PuppeteerSharp;
using System.IO.Compression;    

namespace Archiver.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : Controller
    {
        private static string sourceFiles = "files";
        private static string outputFile = "files.zip";
        private OutputOptions _options;

        public ConverterController(IOptions<OutputOptions> options)
        {
            _options = options.Value;
        }
        
        [HttpPost("parse")]
        public async Task<IActionResult> ParseRawData([FromBody] RequestModel model)
        {
            try
            {
                if (!Directory.Exists(sourceFiles))
                {
                    Directory.CreateDirectory(sourceFiles);
                }
                if (model.HtmlUrls.Count != 0)
                {
                    int pdfNum = 1;
                    foreach (var url in model.HtmlUrls)
                    {
                        await ParseHtmlToPdf(url, pdfNum);
                        pdfNum++;
                    }
                }
                string outputSource = Path.Combine(_options.outputDir, outputFile);
                ZipFile.CreateFromDirectory(sourceFiles, outputSource);
                return Ok();
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
        }



        private async Task ParseHtmlToPdf(string url, int pdfNum)
        {
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            await page.PdfAsync($"files/{pdfNum}");
        }

    }
}
