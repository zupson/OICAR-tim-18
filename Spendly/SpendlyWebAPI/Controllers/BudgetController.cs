using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetController : ControllerBase
    {
        private readonly BudgetService _budgetService;

        public BudgetController(BudgetService budgetService)
        {
            _budgetService = budgetService;
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPost("[action]/{userGroupId}")]
        public async Task<ActionResult<ResponseBudgetDto>> CreateBudget(int userGroupId, CreateBugdetDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdBudget = await _budgetService.CreateAsync(userGroupId, dto);
                return CreatedAtAction(nameof(GetBudgetById), new { id = createdBudget.Id }, createdBudget);
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
        public async Task<IActionResult> DeleteBudget(int id)
        {
            try
            {
                var deletedBudget = await _budgetService.DeleteAsync(id);
                if (!deletedBudget) return NotFound();

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

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseBudgetDto>>> GetAllBudgets()
        {
            try
            {
                var budgets = await _budgetService.GetAllAsync();
                return Ok(budgets);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseBudgetDto>> GetBudgetById(int id)
        {
            try
            {
                var country = await _budgetService.GetByIdAsync(id);
                if (country == null)
                    return NotFound();

                return Ok(country);
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

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPut("[action]/{id}")]
        public async Task<IActionResult> UpdateBudget(int id, EditBudgetDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedBudget = await _budgetService.EditAsync(id, dto);
                if (!updatedBudget)
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
    }
}