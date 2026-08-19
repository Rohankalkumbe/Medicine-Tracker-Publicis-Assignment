namespace MedicineTracker.Api.Models
{
    public class SaleRecord
    {
        public int Id { get; set; }

        public int MedicineId { get; set; }

        public string MedicineName { get; set; }

        public int QuantitySold { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime SaleDate { get; set; }
    }

    // Request payload used when the client records a new sale
    public class SaleRequest
    {
        public int MedicineId { get; set; }

        public int Quantity { get; set; }
    }
}
