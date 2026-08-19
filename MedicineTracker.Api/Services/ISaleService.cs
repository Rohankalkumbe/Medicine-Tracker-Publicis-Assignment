using MedicineTracker.Api.Models;

namespace MedicineTracker.Api.Services
{
    public interface ISaleService
    {
        List<SaleRecord> GetAll();

        // Returns the created SaleRecord, or null if the sale could not be
        // completed (e.g. medicine not found or insufficient stock).
        SaleRecord RecordSale(SaleRequest request);
    }
}
