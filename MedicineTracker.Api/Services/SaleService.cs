using System.Text.Json;
using MedicineTracker.Api.Models;

namespace MedicineTracker.Api.Services
{
    public class SaleService : ISaleService
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly IMedicineService _medicineService;
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public SaleService(IWebHostEnvironment env, IMedicineService medicineService)
        {
            _medicineService = medicineService;

            var dataDir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "sales.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public List<SaleRecord> GetAll()
        {
            lock (_lock)
            {
                return ReadAll().OrderByDescending(s => s.SaleDate).ToList();
            }
        }

        public SaleRecord RecordSale(SaleRequest request)
        {
            if (request == null || request.Quantity <= 0)
            {
                return null;
            }

            var medicine = _medicineService.GetById(request.MedicineId);
            if (medicine == null)
            {
                return null;
            }

            // Attempt to reduce stock first; this also validates availability.
            var success = _medicineService.DecrementStock(request.MedicineId, request.Quantity);
            if (!success)
            {
                return null;
            }

            lock (_lock)
            {
                var all = ReadAll();
                var record = new SaleRecord
                {
                    Id = all.Count == 0 ? 1 : all.Max(s => s.Id) + 1,
                    MedicineId = medicine.Id,
                    MedicineName = medicine.FullName,
                    QuantitySold = request.Quantity,
                    UnitPrice = medicine.Price,
                    TotalAmount = medicine.Price * request.Quantity,
                    SaleDate = DateTime.UtcNow
                };

                all.Add(record);
                WriteAll(all);
                return record;
            }
        }

        private List<SaleRecord> ReadAll()
        {
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<SaleRecord>();
            }

            return JsonSerializer.Deserialize<List<SaleRecord>>(json, _jsonOptions)
                   ?? new List<SaleRecord>();
        }

        private void WriteAll(List<SaleRecord> records)
        {
            var json = JsonSerializer.Serialize(records, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
