using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using PuppeteerSharp;
using System.Diagnostics;
using Tesseract;
using iText.Layout;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;

namespace Archiver.API.Helpers
{
    public class FileSaver
    {
        private static string tessDataPath = "tessdata";
        private static string sourceFiles = "files";
        private static string outputFile = "files.zip";
        private int pdfNum;
        private readonly object lk = new object();

        public FileSaver() 
        {
            if (!Directory.Exists(sourceFiles))
            {
                Directory.CreateDirectory(sourceFiles);
            }
            pdfNum = 0;
        }


        public async Task HandleTextFiles(IFormFileCollection files)
        {
            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var reader = new StreamReader(file.OpenReadStream());
                    var text = await reader.ReadToEndAsync();
                    ParseTextToPdf(text);
                }
            }

        }
        public async Task HandleImages(IFormFileCollection images)
        {
            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    using var engine = new TesseractEngine(tessDataPath, "rus+eng", EngineMode.LstmOnly);
                    var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    var bytes = memoryStream.ToArray();
                    var pix = Pix.LoadFromMemory(bytes);
                    using var page = engine.Process(pix);
                    var text = page.GetText();
                    ParseTextToPdf(text);
                }
            }
            
        }

        public async Task HanldeHtml(List<string> urls)
        {
            foreach(var url in urls)
            {
                await ParseHtmlToPdf(url);
            }
        }


        private void ParseTextToPdf(string text)
        {
            int curNum = -1;
            lock (lk)
            {
                curNum = pdfNum;
                pdfNum++;
            }
            var outPath = Path.Combine(sourceFiles, $"pdf{curNum}.pdf");
            using var pdfWriter = new PdfWriter(outPath);
            using var pdf = new PdfDocument(pdfWriter);
            pdf.AddEventHandler(PdfDocumentEvent.END_PAGE, new PdfFooterEventHandler());
            Document doc = new(pdf, iText.Kernel.Geom.PageSize.A4, false);
            doc.SetMargins(20, 20, 30, 20);
            var p = new Paragraph(text)
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(14);
            doc.Add(p);
            doc.Close();
        }


        private async Task ParseHtmlToPdf(string url)
        {
            int curNum = -1;
            lock (lk)
            {
                curNum = pdfNum;
                pdfNum++;
            }
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            var outPath = Path.Combine(sourceFiles, $"pdf{curNum}.pdf");
            await page.PdfAsync(outPath);
        }

        public void SaveToZip(string outputDir)
        {
            string outputSource = Path.Combine(outputDir, outputFile);
            ZipFile.CreateFromDirectory(sourceFiles, outputSource);
            Directory.Delete(sourceFiles);
        }
    }
}
