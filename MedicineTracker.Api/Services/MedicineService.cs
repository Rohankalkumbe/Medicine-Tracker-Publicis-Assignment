using System.Text.Json;
using MedicineTracker.Api.Models;

namespace MedicineTracker.Api.Services
{
    // Simple JSON-file-backed repository for Medicines.
    // Registered as a Singleton so the in-process lock actually protects
    // concurrent requests against the same file.
    public class MedicineService : IMedicineService
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        public MedicineService(IWebHostEnvironment env)
        {
            var dataDir = Path.Combine(env.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            _filePath = Path.Combine(dataDir, "medicines.json");

            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public List<Medicine> GetAll(string search = null)
        {
            lock (_lock)
            {
                var all = ReadAll();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    all = all
                        .Where(m => m.FullName != null &&
                                    m.FullName.Contains(search, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                }

                return all.OrderBy(m => m.Id).ToList();
            }
        }

        public Medicine GetById(int id)
        {
            lock (_lock)
            {
                return ReadAll().FirstOrDefault(m => m.Id == id);
            }
        }

        public Medicine Add(Medicine medicine)
        {
            lock (_lock)
            {
                var all = ReadAll();
                medicine.Id = all.Count == 0 ? 1 : all.Max(m => m.Id) + 1;
                all.Add(medicine);
                WriteAll(all);
                return medicine;
            }
        }

        public Medicine Update(Medicine medicine)
        {
            lock (_lock)
            {
                var all = ReadAll();
                var index = all.FindIndex(m => m.Id == medicine.Id);
                if (index == -1)
                {
                    return null;
                }

                all[index] = medicine;
                WriteAll(all);
                return medicine;
            }
        }

        public bool Delete(int id)
        {
            lock (_lock)
            {
                var all = ReadAll();
                var removed = all.RemoveAll(m => m.Id == id);
                if (removed > 0)
                {
                    WriteAll(all);
                    return true;
                }
                return false;
            }
        }

        // Reduces stock quantity when a sale is recorded. Returns false if
        // the medicine doesn't exist or there isn't enough stock.
        public bool DecrementStock(int id, int quantity)
        {
            lock (_lock)
            {
                var all = ReadAll();
                var medicine = all.FirstOrDefault(m => m.Id == id);
                if (medicine == null || medicine.Quantity < quantity)
                {
                    return false;
                }

                medicine.Quantity -= quantity;
                WriteAll(all);
                return true;
            }
        }

        private List<Medicine> ReadAll()
        {
            var json = File.ReadAllText(_filePath);
            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<Medicine>();
            }

            return JsonSerializer.Deserialize<List<Medicine>>(json, _jsonOptions)
                   ?? new List<Medicine>();
        }

        private void WriteAll(List<Medicine> medicines)
        {
            var json = JsonSerializer.Serialize(medicines, _jsonOptions);
            File.WriteAllText(_filePath, json);
        }
    }
}
