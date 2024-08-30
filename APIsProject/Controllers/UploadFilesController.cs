using APIsProject.Models;
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
        private readonly AppDbContext _context;

        public UploadFilesController(AppDbContext context)
        {
            _context = context;
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
                    await ProcessCsv(file);
                }
                else if (extension == ".xml")
                {
                    await ProcessXml(file);
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
                return StatusCode(500, $"An error occurred while processing the file: {ex.Message}");
            }

            return Ok("File uploaded and processed successfully.");
        }

        public static string MapStatusToCode(string status)
        {
            return status switch
            {
                "Approved" => "A",
                "Failed" => "R",
                "Rejected" => "R",
                "Finished" => "D",
                "Done" => "D",
                _ => throw new ArgumentException($"Unknown status: {status}")
            };
        }
        private async Task ProcessCsv(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                TrimOptions = TrimOptions.Trim
            });

            csv.Context.TypeConverterCache.AddConverter<decimal>(new CustomDecimalConverter());
            csv.Context.TypeConverterCache.AddConverter<DateTime>(new CustomDateTimeConverter());

            var records = csv.GetRecords<TransactionCsvRecord>();

            foreach (var record in records)
            {
                var res = record.Status.Trim('"', '“', '”');
                var TransactionId = record.TransactionId.Trim('"', '“', '”');
                var    Amount = record.Amount;
                var CurrencyCode = record.CurrencyCode.Trim('"', '“', '”');
                var    TransactionDate = record.TransactionDate;
                var Status = MapStatusToCode(record.Status.Trim('"', '“', '”'));

                var transaction = new Transactions
                {
                    //Id = null,
                    TransactionId = record.TransactionId.Trim('"', '“', '”'),
                    Amount = record.Amount,
                    CurrencyCode = record.CurrencyCode.Trim('"', '“', '”'),
                    TransactionDate = record.TransactionDate,
                    Status = MapStatusToCode(record.Status.Trim('"', '“', '”'))
            };

                _context.Transaction.Add(transaction);
            }

            await _context.SaveChangesAsync();
        }

        public class TransactionCsvRecord
        {
            public string TransactionId { get; set; }
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Status { get; set; }
        }

        private async Task ProcessXml(IFormFile file)
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var xmlContent = await reader.ReadToEndAsync();

            // Replace curly quotes with straight quotes
            xmlContent = ReplaceCurlyQuotesWithStraightQuotes(xmlContent);

            var xmlDoc = new XmlDocument();

            try
            {
                xmlDoc.LoadXml(xmlContent);
            }
            catch (XmlException ex)
            {
                throw new XmlException($"Error parsing XML content: {ex.Message}");
            }

            var transactions = xmlDoc.SelectNodes("//Transaction");

            foreach (XmlNode transaction in transactions)
            {
                try
                {
                    var record = new Transactions
                    {
                        TransactionId = transaction.Attributes["id"].Value,
                        TransactionDate = DateTime.Parse(transaction.SelectSingleNode("TransactionDate").InnerText),
                        Amount = decimal.Parse(transaction.SelectSingleNode("PaymentDetails/Amount").InnerText),
                        CurrencyCode = transaction.SelectSingleNode("PaymentDetails/CurrencyCode").InnerText,
                        Status = MapStatusToCode(transaction.SelectSingleNode("Status").InnerText)
                    };

                    _context.Transaction.Add(record);
                }
                catch (FormatException ex)
                {
                    throw new FormatException($"Error parsing XML transaction: {transaction.OuterXml}. Details: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
        }

        private string ReplaceCurlyQuotesWithStraightQuotes(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Replace curly double quotes with straight double quotes
            input = input.Replace("“", "\"").Replace("”", "\"");

            // Replace curly single quotes with straight single quotes
            input = input.Replace("‘", "'").Replace("’", "'");

            return input;
        }


       
        public class CustomDecimalConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                // Replace non-standard quotes and format the amount correctly
                text = text.Replace('“', '"').Replace('”', '"').Trim('"');

                // Convert the amount from non-standard format to standard decimal
                if (decimal.TryParse(text, NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint,
                                     new CultureInfo("de-DE"), out var result))
                {
                    return result;
                }

                throw new CsvHelper.TypeConversion.TypeConverterException(this, memberMapData, text, row.Context, $"Cannot convert '{text}' to decimal.");
            }
        }

        public class CustomDateTimeConverter : DefaultTypeConverter
        {
            public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
            {
                text = text.Replace('“', '"').Replace('”', '"').Trim('"');

                // Specify the exact format used in the CSV
                string[] formats = { "dd/MM/yyyy HH:mm:ss" };

                if (DateTime.TryParseExact(text, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                {
                    return date;
                }

                throw new CsvHelper.TypeConversion.TypeConverterException(this, memberMapData, text, row.Context, $"Cannot convert '{text}' to DateTime.");
            }
        }

    }
}