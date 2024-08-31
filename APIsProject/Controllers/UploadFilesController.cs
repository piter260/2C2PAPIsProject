using APIsProject.Models;
using APIsProject.Services;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Formats.Asn1;
using System.Globalization;
using System.Xml;

namespace APIsProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UploadFilesController : ControllerBase
    {
        private readonly ProcessingFilesService _processingFilesService;

        public UploadFilesController(ProcessingFilesService processingFilesService)
        {
            _processingFilesService = processingFilesService;
        }

        [HttpPost("FileUpload")]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string extension = Path.GetExtension(file.FileName).ToLower();

            try
            {
                if (extension == ".csv")
                {
                    await _processingFilesService.ProcessCsv(file);
                }
                else if (extension == ".xml")
                {
                    await _processingFilesService.ProcessXml(file);
                }
                else
                {
                    return BadRequest("Unsupported file format.");
                }
            }
            catch (FormatException ex)
            {
                // Handle format exceptions such as invalid date or decimal formats
                return BadRequest($"File format error: {ex.Message}");
            }
            catch (XmlException ex)
            {
                // Handle XML parsing errors
                return BadRequest($"XML parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle any other unexpected exceptions
                return StatusCode(500, "Bad Request");
            }

            return Ok("File uploaded and processed successfully.");
        }

    }
}