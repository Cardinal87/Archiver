﻿using Archiver.API.DTO;
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
using Archiver.API.Helpers;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;



namespace Archiver.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ConverterController : Controller
    {
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
                var saver = new FileSaver();
                if (model.HtmlUrls.Count != 0)
                {
                    await saver.HanldeHtml(model.HtmlUrls);

                }
                if (model.Images.Count > 0)
                {
                    await saver.HandleImages(model.Images);
                }
                if (model.TextFiles != null)
                {
                    await saver.HandleTextFiles(model.TextFiles);
                }
                saver.SaveToZip(_options.outputDir);
                return Ok();
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
        }

        

    }
}
