using Archiver.API.DTO;
using PuppeteerSharp;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.IO.Compression;
using Tesseract;
using Microsoft.AspNetCore.Mvc.Formatters;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Kernel.Pdf.Event;
using iText.Layout.Element;
using iText.Layout.Properties;
using static System.Net.Mime.MediaTypeNames;
using Archiver.API.PdfEventHandlers;



namespace Archiver.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : Controller
    {
        private static string tessDataPath = "tessdata";
        private static string sourceFiles = "files";
        private static string outputFile = "files.zip";
        private OutputOptions _options;

        public ConverterController(IOptions<OutputOptions> options)
        {
            _options = options.Value;
        }
        
        [HttpPost("parse")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ParseRawData([FromForm] RequestModel model)
        {
            try
            {
                if (!Directory.Exists(sourceFiles))
                {
                    Directory.CreateDirectory(sourceFiles);
                }
                int pdfNum = 1;
                if (model.HtmlUrls.Count != 0)
                {
                    foreach (var url in model.HtmlUrls)
                    {
                        await ParseHtmlToPdf(url, pdfNum);
                        pdfNum++;
                    }
                     
                }
                if (model.Pictures != null)
                {
                    foreach (var image in model.Pictures)
                    {
                        if (image.Length > 0)
                        {
                            await HandleImages(image, pdfNum);
                            pdfNum++;
                        }
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

        private async Task HandleImages(IFormFile image, int docnum)
        {
            using var engine = new TesseractEngine("tessdata", "rus+eng", EngineMode.LstmOnly);
            var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var bytes = memoryStream.ToArray();
            var pix = Pix.LoadFromMemory(bytes);
            using var page = engine.Process(pix);
            var text = page.GetText();
            ParseTextToPdf(text, docnum);
        }


        private void ParseTextToPdf(string text, int pdfNum)
        {
            var outPath = Path.Combine(sourceFiles, $"pdf{pdfNum}");
            using var pdfWriter = new PdfWriter(outPath);
            using var pdf = new PdfDocument(pdfWriter);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PdfFooterEventHandler());
            Document doc = new(pdf, iText.Kernel.Geom.PageSize.A4, false);
            doc.SetMargins(20, 20, 30, 20);
            var p = new Paragraph(text)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(14)
                .SetFirstLineIndent(50);
            doc.Add(p);
            doc.Close();
        }


        private async Task ParseHtmlToPdf(string url, int pdfNum)
        {
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            var outPath = Path.Combine(sourceFiles, $"pdf{pdfNum}");
            await page.PdfAsync(outPath);
        }

    }
}
