
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using Archiver.API.Helpers;
using Newtonsoft.Json;
using Archiver.API.DTO.Request;
using Newtonsoft.Json.Linq;
using System.IO;



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
                if (!Directory.Exists(_options.outputDir))
                {
                    return BadRequest(new { Message = "output folder does not exist" });
                }
                var saver = new FileSaver();

                await saver.HanldeHtml(model.HtmlUrls);
                await saver.HandleImages(model.Images);
                await saver.HandleTextFiles(model.TextFiles);
                await saver.HandleRawFiles(model.OtherFiles);
                await saver.SaveToZip(_options.outputDir);
                return Ok();
            }
            catch(Exception ex)
            {
                return Problem(ex.Message);
            }
        }
        [HttpPost("savepath")]
        [Consumes("applicetion/json")]
        public async Task<IActionResult> SavePath([FromBody] JObject data)
        {
            string path = data["data"]?["path"]?.ToString()!;
            if (!String.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                string config = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                var jObj = JsonConvert.DeserializeObject<JObject>(System.IO.File.ReadAllText(config)) ?? throw new FileNotFoundException($"appsettings.json by path {config} was not found");
                var section = jObj["OutputOptions"]!;
                section["Path"] = path;
                jObj["OutputOptions"] = JObject.Parse(JsonConvert.SerializeObject(section));
                string json = JsonConvert.SerializeObject(jObj, Formatting.Indented);
                await System.IO.File.WriteAllTextAsync(config, json);
                return Ok();
            }
            else
            {
                return BadRequest(new { Message = "directory does not exist" });
            }
        }

    }
}
