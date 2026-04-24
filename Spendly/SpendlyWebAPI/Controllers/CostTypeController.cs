using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CostTypeController : ControllerBase
    {
        private readonly CostTypeService _costTypeService;

        public CostTypeController(CostTypeService costTypeService)
        {
            _costTypeService = costTypeService;
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseCostTypeDto>>> GetAllCostTypes()
        {
            try
            {
                var allCostTypes = await _costTypeService.GetAllAsync();
                return Ok(allCostTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseCostTypeDto>> GetCostTypeById(int id)
        {
            try
            {
                var findCostType = await _costTypeService.GetByIdAsync(id);
                return Ok(findCostType);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseCostTypeDto>> CreateNewCostType(CreateCostTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newCostType = await _costTypeService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetCostTypeById), new { id = newCostType.Id }, newCostType);
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
        public async Task<IActionResult> EditCostType(int id, EditCostTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _costTypeService.EditAsync(id, dto);

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
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteCostType(int id)
        {
            try
            {
                var deletedCostType = await _costTypeService.DeleteAsync(id);
                if (!deletedCostType)
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