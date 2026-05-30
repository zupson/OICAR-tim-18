using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly GroupService _groupService;

        public GroupController(GroupService groupService)
        {
            _groupService = groupService;
        }

        [Authorize(Roles = "GeneralAdmin")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseGroupDto>>> GetAllGroups()
        {
            try
            {
                var allGroups = await _groupService.GetAllAsync();
                return Ok(allGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseGroupDto>> GetGroupById(int id)
        {
            try
            {
                var findGroup = await _groupService.GetByIdAsync(id);
                return Ok(findGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseGroupDto>> CreateNewGroup(CreateGroupDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newGroup = await _groupService.CreateAsync(dto);

                return CreatedAtAction(nameof(GetGroupById), new { id = newGroup.Id }, newGroup);
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
        public async Task<IActionResult> EditGroup(int id, EditGroupDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                bool isUpdated = await _groupService.EditAsync(id, dto);

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
        public async Task<IActionResult> DeleteGroup(int id)
        {
            try
            {
                var deletedGroup = await _groupService.DeleteAsync(id);
                if (!deletedGroup)
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
