using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserGroupController : ControllerBase
    {
        private readonly UserGroupService _userGroupService;
        public UserGroupController(UserGroupService userGroupService)
        {
            _userGroupService = userGroupService;
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseUserGroupDto>>> GetAllUserGroups()
        {
            try
            {
                var allUserGroups = await _userGroupService.GetAllAsync();
                return Ok(allUserGroups);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseUserGroupDto>> GetUserGroupById(int id)
        {
            try
            {
                var findUserGroup = await _userGroupService.GetByIdAsync(id);
                return Ok(findUserGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseUserGroupDto>> GetMemebersByGroup(int id)
        {
            try
            {
                var findUserGroup = await _userGroupService.GetMembersByGroup(id);
                return Ok(findUserGroup);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteUserGroup(int id)
        {
            try
            {
                var deletedUserGroup = await _userGroupService.DeleteAsync(id);
                if (!deletedUserGroup)
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

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> RemoveMember(int id)
        {
            try
            {
                var removed = await _userGroupService.RemoveMemberAsync(id);
                if (!removed) return NotFound();
                return NoContent();
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
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