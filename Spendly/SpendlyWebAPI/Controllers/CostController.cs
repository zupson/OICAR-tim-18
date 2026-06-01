using Microsoft.AspNetCore.Authorization;
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

        [Authorize(Roles = "GeneralAdmin,User")]
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

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseCostDto>>> GetAllCostsByGroup([FromQuery] int groupId)
        {
            try
            {
                var costs = await _costService.GetAllByGroupAsync(groupId);
                return Ok(costs);
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
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

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPost("[action]/{groupId}")]
        public async Task<ActionResult<ResponseCostDto>> CreateNewCost(int groupId, CreateCostDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newCost = await _costService.CreateAsync(dto, groupId);

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

        [Authorize(Roles = "GeneralAdmin,User")]
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

        [Authorize(Roles = "GeneralAdmin,User")]
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
