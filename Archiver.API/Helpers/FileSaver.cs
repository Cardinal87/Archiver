using iText.Kernel.Pdf.Event;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using PuppeteerSharp;
using System.Diagnostics;
using Archiver.API.DTO.Manifest;
using Tesseract;
using iText.Layout;
using System.IO.Compression;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography.Xml;
using Archiver.API.DTO.Request;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace Archiver.API.Helpers
{
    public class FileSaver
    {
        private static string tessDataPath = "tessdata";
        private static string manifestPath = "manifest.json";
        private static string sourceFiles = "files";
        private static string outputFile = "files.zip";
        private ManifestModel model;
        public FileSaver() 
        {
            if (Directory.Exists(sourceFiles))
            {
                Directory.Delete(sourceFiles, true);
            }
            Directory.CreateDirectory(sourceFiles);
            if (File.Exists(manifestPath))
            {
                File.Delete(manifestPath);
            }
            model = new();
        }


        public async Task HandleRawFiles(List<IFormFile> files)
        {
            foreach(var file in files)
            {
                if (file.Length > 0)
                {
                    string outPath = Path.Combine(sourceFiles, file.FileName);
                    using var fileStr = new FileStream(outPath, FileMode.Create);
                    await file.CopyToAsync(fileStr);
                }
            }
        }

        public async Task HandleTextFiles(List<MyFileOptions> files)
        {
            foreach (var file in files.Select(x => x.File))
            {
                if (file != null && file.Length > 0)
                {
                    var reader = new StreamReader(file.OpenReadStream());
                    var text = await reader.ReadToEndAsync();
                    string name = Path.GetFileNameWithoutExtension(file.FileName);
                    var info = ParseTextToPdf(text, name);
                    model.Content.Add(info);
                }
            }

        }
        public async Task HandleImages(List<MyFileOptions> images)
        {
            foreach (var image in images.Select(x => x.File))
            {
                if (image != null && image.Length > 0)
                {
                    using var engine = new TesseractEngine(tessDataPath, "rus", EngineMode.LstmOnly);
                    var memoryStream = new MemoryStream();
                    await image.CopyToAsync(memoryStream);
                    var bytes = memoryStream.ToArray();
                    var pix = Pix.LoadFromMemory(bytes);
                    using var page = engine.Process(pix);
                    var text = page.GetText();
                    string name = Path.GetFileNameWithoutExtension(image.FileName);
                    var info = ParseTextToPdf(text, name);
                    model.Content.Add(info);
                }
            }
            
        }

        public async Task HanldeHtml(List<string> urls)
        {
            foreach(var url in urls)
            {
                var uri = new Uri(url);
                string name = uri.Host;
                var info = await ParseHtmlToPdf(Uri.UnescapeDataString(url), name);
                model.Content.Add(info);

            }
        }


        private MyFileInfo ParseTextToPdf(string text, string name)
        {
            var outPath = Path.Combine(sourceFiles, $"{name}.pdf");
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

            var bytes = File.ReadAllBytes(outPath);
            string crc = GetCRC(bytes);

            var info = new MyFileInfo
            {
                Path = outPath,
                Name = name,
                Crc = crc,
                Size = bytes.Length
            };

            return info;

        }


        private async Task<MyFileInfo> ParseHtmlToPdf(string url, string name)
        {
            
            await new BrowserFetcher().DownloadAsync();
            using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            using var page = await browser.NewPageAsync();
            await page.GoToAsync(url);
            var outPath = Path.Combine(sourceFiles, $"{name}.pdf");
            await page.PdfAsync(outPath);

            var bytes = File.ReadAllBytes(outPath);
            string crc = GetCRC(bytes);

            var info = new MyFileInfo
            {
                Path = outPath,
                Name = name,
                Crc = crc,
                Size = bytes.Length
            };

            return info;
        }

        public async Task SaveToZip(string outputDir)
        {
            string outputSource = Path.Combine(outputDir, outputFile);

            using var zipStream = new FileStream(outputSource, FileMode.Create);
            using var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            AddDirectoryToArchive(archive);
            await AddManifestFile(archive);            

            Directory.Delete(sourceFiles, true);
        }

        private async Task AddManifestFile(ZipArchive archive)
        {
            model.ArchiveInfo.Compression = "Zip/Deflate";
            model.ArchiveInfo.CreatedAt = DateTime.Now;
            string json = JsonConvert.SerializeObject(model, Formatting.Indented);
            await File.WriteAllTextAsync(manifestPath, json);
            archive.CreateEntryFromFile(manifestPath, manifestPath);
            File.Delete(manifestPath);
        }

        private void AddDirectoryToArchive(ZipArchive archive)
        {
            foreach(var file in Directory.GetFiles(sourceFiles))
            {
                string destPath = Path.Combine(sourceFiles, Path.GetFileName(file));
                archive.CreateEntryFromFile(file, destPath);
            }
        }

        private string GetCRC(byte[] bytes)
        {
            using var sha = SHA256.Create();
            var crc = sha.ComputeHash(bytes);
            string crsStr = BitConverter.ToString(crc);
            return crsStr;
        }
    }
}
