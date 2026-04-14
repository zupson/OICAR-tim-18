using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RevenueTypeController : ControllerBase
    {
        private readonly RevenueTypeService _revenueTypeService;

        public RevenueTypeController(RevenueTypeService revenueTypeService)
        {
            _revenueTypeService = revenueTypeService;
        }

        // GET: api/<RoleController>
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseRevenueTypeDto>>> GetAllRevenueTypes()
        {
            try
            {
                var allRevenueTypes = await _revenueTypeService.GetAllAsync();
                return Ok(allRevenueTypes);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET api/<RoleController>/5
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseRevenueTypeDto>> GetRevenueTypeById(int id)
        {
            try
            {
                var findRevenueType = await _revenueTypeService.GetByIdAsync(id);
                return Ok(findRevenueType);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST api/<RoleController>
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseRevenueTypeDto>> CreateNewRevenueType(CreateRevenueTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newRevenueType = await _revenueTypeService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetRevenueTypeById), new { id = newRevenueType.Id }, newRevenueType);
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

        // PUT api/<RoleController>/5
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> EditRevenueType(int id, EditRevenueTypeDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _revenueTypeService.EditAsync(id, dto);

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

        // DELETE api/<RoleController>/5
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteRevenueType(int id)
        {
            try
            {
                var deletedRevenueType = await _revenueTypeService.DeleteAsync(id);
                if (!deletedRevenueType)
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