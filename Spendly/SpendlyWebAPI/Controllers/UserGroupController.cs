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

        //[HttpPost("[action]/{groupId}")]
        //public async Task<ActionResult<ResponseUserGroupDto>> CreateNewUserGroup(int groupId)
        //{
        //    try
        //    {
        //        if (!ModelState.IsValid)
        //            return BadRequest(ModelState);

        //        var newUserGroup = await _userGroupService.CreateAsync(groupId);

        //        return CreatedAtAction(nameof(GetUserGroupById), new { id = newUserGroup.Id }, newUserGroup);
        //    }
        //    catch (KeyNotFoundException ex)
        //    {
        //        return NotFound(ex.Message);
        //    }
        //    catch (InvalidOperationException ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        //    }
        //}


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
    }
}