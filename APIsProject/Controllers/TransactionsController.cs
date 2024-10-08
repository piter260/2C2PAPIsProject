using APIsProject.Models;
using APIsProject.Services;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

namespace APIsProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ProcessingService _processingFilesService;

        public TransactionsController(ProcessingService processingFilesService)
        {
            _processingFilesService = processingFilesService;
        }

        [HttpPost("file-upload")]
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

        [HttpGet("get-transations-by-currency/{currency}")]
        public ActionResult<IEnumerable<Transactions>> GetTransactionsByCurrency(string currency)
        {
            var transactions = _processingFilesService.GetTransactionsByCurrency(currency);

            if (!transactions.Any())
            {
                return NotFound();
            }

            return Ok(transactions);
        }

        [HttpGet("get-transations-by-daterange")]
        public ActionResult<IEnumerable<Transactions>> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            var transactions = _processingFilesService.GetTransactionsByDateRange(startDate, endDate);

            if (!transactions.Any())
            {
                return NotFound();
            }

            return Ok(transactions);
        }

        [HttpGet("get-transations-by-status/{status}")]
        public ActionResult<IEnumerable<Transactions>> GetTransactionsByStatus(string status)
        {
            var transactions = _processingFilesService.GetTransactionsByStatus(status);

            if (!transactions.Any())
            {
                return NotFound();
            }

            return Ok(transactions);
        }

    }
}