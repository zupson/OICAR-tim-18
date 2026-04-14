using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueController : ControllerBase
    {
        private readonly RevenueService _revenueService;

        public RevenueController(RevenueService revenueService)
        {
            _revenueService = revenueService;
        }

        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseRevenueDto>>> GetAllRevenues()
        {
            try
            {
                var allRevenues = await _revenueService.GetAllAsync();
                return Ok(allRevenues);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseRevenueDto>> GetRevenueById(int id)
        {
            try
            {
                var findRevenue = await _revenueService.GetByIdAsync(id);
                return Ok(findRevenue);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseRevenueDto>> CreateNewRevenue(CreateRevenueDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newRevenue = await _revenueService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetRevenueById), new { id = newRevenue.Id }, newRevenue);
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
        public async Task<IActionResult> EditRevenueType(int id, EditRevenueDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _revenueService.EditAsync(id, dto);

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

        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteRevenue(int id)
        {
            try
            {
                var deletedRevenue = await _revenueService.DeleteAsync(id);
                if (!deletedRevenue)
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
