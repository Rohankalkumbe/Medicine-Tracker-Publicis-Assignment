using MedicineTracker.Api.Models;

namespace MedicineTracker.Api.Services
{
    public interface IMedicineService
    {
        List<Medicine> GetAll(string search = null);
        Medicine GetById(int id);
        Medicine Add(Medicine medicine);
        Medicine Update(Medicine medicine);
        bool Delete(int id);
        bool DecrementStock(int id, int quantity);
    }
}
