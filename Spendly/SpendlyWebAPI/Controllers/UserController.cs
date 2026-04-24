using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpendlyWebAPI.Dal.Repo;
using SpendlyWebAPI.Dtos;

namespace SpendlyWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IAuthentication<ResponseUserDto> _userService;
        public UserController(IAuthentication<ResponseUserDto> userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "GeneralAdmin")]
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<ResponseUserDto>>> GetAllUssers()
        {
            try
            {
                var people = await _userService.GetAllAsync();
                return Ok(people);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin")]
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<ResponseUserDto>> GetUserById(int id)
        {
            try
            {
                var person = await _userService.GetByIdAsync(id);
                if (person == null)
                    return NotFound();

                return Ok(person);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseUserDto>> RegisterUser(RegisterUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var (user, token) = await _userService.RegisterAsync(dto);
                var response = new
                {
                    User = user,
                    Token = token
                };

                return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, response);
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

        [AllowAnonymous]
        [HttpPost("[action]")]
        public async Task<ActionResult<ResponseUserDto>> LoginUser(LoginUserDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                (var user, var token) = await _userService.LoginAsync(dto);
                var response = new
                {
                    User = user,
                    Token = token
                };
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin,User")]
        [HttpPut("[action]")]
        public async Task<ActionResult<ChangePasswordDto>> ChangePassword(ChangePasswordDto dto)
        {
            try
            {
                var passwordChanged = await _userService.PasswordChangeAsync(dto);
                return Ok(passwordChanged);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [Authorize(Roles = "GeneralAdmin")]
        [HttpPut("[action]")]
        public async Task<IActionResult> EditPerson(int id, EditUserDto dto)
        {
            try
            {
                var passwordChanged = await _userService.EditAsync(id, dto);
                return Ok(passwordChanged);
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

        [Authorize(Roles = "GeneralAdmin")]
        [HttpDelete("[action]/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                bool success = await _userService.DeleteAsync(id);
                if (!success)
                    return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}