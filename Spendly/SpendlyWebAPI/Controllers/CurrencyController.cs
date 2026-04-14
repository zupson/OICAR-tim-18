using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ControllerBase
    {
        private readonly CurrencyService _currencyService;

        public CurrencyController(CurrencyService currencyService)
        {
            _currencyService = currencyService;
        }

        // GET: api/<RoleController>
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseCurrencyDto>>> GetAllCurrencies()
        {
            try
            {
                var allCurrencies = await _currencyService.GetAllAsync();
                return Ok(allCurrencies);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET api/<RoleController>/5
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseCurrencyDto>> GetCurrencyById(int id)
        {
            try
            {
                var findCurrency = await _currencyService.GetByIdAsync(id);
                return Ok(findCurrency);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST api/<RoleController>
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseCurrencyDto>> CreateNewCurrency(CreateCurrencyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newCurrency = await _currencyService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetCurrencyById), new { id = newCurrency.Id }, newCurrency);
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
        public async Task<IActionResult> EditCurrency(int id, EditCurrencyDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _currencyService.EditAsync(id, dto);

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
        public async Task<IActionResult> DeleteCurrency(int id)
        {
            try
            {
                var deletedCurrency = await _currencyService.DeleteAsync(id);
                if (!deletedCurrency)
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