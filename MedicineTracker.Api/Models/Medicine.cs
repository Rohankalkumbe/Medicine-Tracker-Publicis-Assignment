using System.ComponentModel.DataAnnotations;

namespace MedicineTracker.Api.Models
{
    public class Medicine
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [Required]
        public string Brand { get; set; }
    }
}
