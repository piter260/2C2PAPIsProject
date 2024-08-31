using APIsProject.Models;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;
using System.Xml;

namespace APIsProject.Services
{
    public interface IProcessingFilesService
    {
        Task ProcessCsv(IFormFile file);
        Task ProcessXml(IFormFile file);
        IEnumerable<Transactions> GetTransactionsByCurrency(string currency);
        IEnumerable<Transactions> GetTransactionsByDateRange(DateTime startDate, DateTime endDate);
        IEnumerable<Transactions> GetTransactionsByStatus(string status);
    }
    public class ProcessingFilesService
    {
        private readonly AppDbContext _context;

        public ProcessingFilesService(AppDbContext context)
        {
            _context = context;
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
        public async Task ProcessCsv(IFormFile file)
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
                var transaction = new Transactions
                {
                    TransactionId = record.TransactionId.Trim('"', '“', '”'),
                    Amount = record.Amount,
                    CurrencyCode = record.CurrencyCode.Trim('"', '“', '”'),
                    TransactionDate = record.TransactionDate,
                    Status = MapStatusToCode(record.Status.Trim('"', '“', '”'))
                };

                _context.Transactions.Add(transaction);
            }

            await _context.SaveChangesAsync();
        }

        private class TransactionCsvRecord
        {
            public string TransactionId { get; set; }
            public decimal Amount { get; set; }
            public string CurrencyCode { get; set; }
            public DateTime TransactionDate { get; set; }
            public string Status { get; set; }
        }

        public async Task ProcessXml(IFormFile file)
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

                    _context.Transactions.Add(record);
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



        private class CustomDecimalConverter : DefaultTypeConverter
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

        private class CustomDateTimeConverter : DefaultTypeConverter
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

        public IEnumerable<Transactions> GetTransactionsByCurrency(string currency)
        {
            return _context.Transactions
                           .Where(t => t.CurrencyCode == currency)
                           .ToList();
        }

        public IEnumerable<Transactions> GetTransactionsByDateRange(DateTime startDate, DateTime endDate)
        {
            return _context.Transactions
                           .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                           .ToList();
        }

        public IEnumerable<Transactions> GetTransactionsByStatus(string status)
        {
            return _context.Transactions
                           .Where(t => t.Status == status)
                           .ToList();
        }
    }
}
