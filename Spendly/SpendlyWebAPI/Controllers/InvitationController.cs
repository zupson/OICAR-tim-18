using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dtos;
using SpendlyWebAPI.Services;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly InvitationService _invitationService;

        public InvitationController(InvitationService invitationService)
        {
            _invitationService = invitationService;
        }


        [Authorize(Roles = "GeneralAdmin")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseInvitationDto>>> GetAllInvitations()
        {
            try
            {
                var allInvitations = await _invitationService.GetAllAsync();
                return Ok(allInvitations);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "User,GeneralAdmin")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseInvitationDto>> GetInvitationById(int id)
        {
            try
            {
                var findInvitation = await _invitationService.GetByIdAsync(id);
                return Ok(findInvitation);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "User,GeneralAdmin")]
        [HttpPost("[action]/{groupId}")]
        public async Task<ActionResult<ResponseInvitationDto>> CreateNewInvitation(int groupId, CreateInvitationDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var newInvitation = await _invitationService.CreateAsync(dto, groupId);

                return CreatedAtAction(nameof(GetInvitationById), new { id = newInvitation.Id }, newInvitation);
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


        [Authorize(Roles = "User,GeneralAdmin")]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteInvitation(int id)
        {
            try
            {
                var deletedInvitation = await _invitationService.DeleteAsync(id);
                if (!deletedInvitation)
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

        [Authorize(Roles = "User")]
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseUserGroupDto>> ClaimInvitation(string token)
        {
            try
            {
                var responseUserGroupDto = await _invitationService.ClaimAsync(token);
                return Ok(responseUserGroupDto);

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