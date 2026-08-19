using Microsoft.AspNetCore.Mvc;
using MedicineTracker.Api.Models;
using MedicineTracker.Api.Services;

namespace MedicineTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;

        public SalesController(ISaleService saleService)
        {
            _saleService = saleService;
        }

        // GET api/sales
        [HttpGet]
        public ActionResult<List<SaleRecord>> GetAll()
        {
            return Ok(_saleService.GetAll());
        }

        // POST api/sales
        // Body: { "medicineId": 1, "quantity": 2 }
        [HttpPost]
        public ActionResult<SaleRecord> RecordSale([FromBody] SaleRequest request)
        {
            if (request == null || request.Quantity <= 0)
            {
                return BadRequest(new { message = "Quantity must be greater than zero." });
            }

            var record = _saleService.RecordSale(request);
            if (record == null)
            {
                return BadRequest(new { message = "Sale could not be completed. Check that the medicine exists and has sufficient stock." });
            }

            return CreatedAtAction(nameof(GetAll), record);
        }
    }
}
