using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostController : ControllerBase
    {
        private readonly CostService _costService;
        public CostController(CostService costService)
        {
            _costService = costService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseCostDto>>> GetAllCosts()
        {
            try
            {
                var allCosts = await _costService.GetAllAsync();
                return Ok(allCosts);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseCostDto>> GetCostById(int id)
        {
            try
            {
                var findCost = await _costService.GetByIdAsync(id);
                return Ok(findCost);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseCostDto>> CreateNewCost(CreateCostDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newCost = await _costService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetCostById), new { id = newCost.Id }, newCost);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }

        }

        [HttpPut("[action]/{id}")]

        public async Task<IActionResult> EditCostType(int id, EditCostDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _costService.EditAsync(id, dto);

                if (!isUpdated)
                    return NotFound();
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCost(int id)
        {
            try
            {
                var deletedCost = await _costService.DeleteAsync(id);
                if (!deletedCost)
                    return NotFound();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
