using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class RoleController : ControllerBase
    {

        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        // GET: api/<RoleController>
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseRoleDto>>> GetAllRoles()
        {
            try
            {
                var allRoles = await _roleService.GetAllAsync();
                return Ok(allRoles);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // GET api/<RoleController>/5
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseRoleDto>> GetRoleById(int id)
        {
            try
            {
                var findRole = await _roleService.GetByIdAsync(id);
                return Ok(findRole);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // POST api/<RoleController>
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseRoleDto>> CreateNewRole(CreateRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newRole = await _roleService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetRoleById), new { id = newRole.Id }, newRole);
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
        public async Task<IActionResult> EditRole(int id, EditRoleDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _roleService.EditAsync(id, dto);

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
        public async Task<IActionResult> DeleteRole(int id)
        {
            try
            {
                var deletedRole = await _roleService.DeleteAsync(id);
                if (!deletedRole)
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
