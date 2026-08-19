using Microsoft.AspNetCore.Mvc;
using MedicineTracker.Api.Models;
using MedicineTracker.Api.Services;

namespace MedicineTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicinesController : ControllerBase
    {
        private readonly IMedicineService _medicineService;

        public MedicinesController(IMedicineService medicineService)
        {
            _medicineService = medicineService;
        }

        // GET api/medicines?search=para
        [HttpGet]
        public ActionResult<List<Medicine>> GetAll([FromQuery] string search)
        {
            return Ok(_medicineService.GetAll(search));
        }

        // GET api/medicines/5
        [HttpGet("{id}")]
        public ActionResult<Medicine> GetById(int id)
        {
            var medicine = _medicineService.GetById(id);
            if (medicine == null)
            {
                return NotFound(new { message = $"Medicine with id {id} was not found." });
            }
            return Ok(medicine);
        }

        // POST api/medicines
        [HttpPost]
        public ActionResult<Medicine> Add([FromBody] Medicine medicine)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var created = _medicineService.Add(medicine);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }

        // PUT api/medicines/5
        [HttpPut("{id}")]
        public ActionResult<Medicine> Update(int id, [FromBody] Medicine medicine)
        {
            if (id != medicine.Id)
            {
                return BadRequest(new { message = "Route id does not match body id." });
            }

            var updated = _medicineService.Update(medicine);
            if (updated == null)
            {
                return NotFound(new { message = $"Medicine with id {id} was not found." });
            }

            return Ok(updated);
        }

        // DELETE api/medicines/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var deleted = _medicineService.Delete(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Medicine with id {id} was not found." });
            }

            return NoContent();
        }
    }
}
