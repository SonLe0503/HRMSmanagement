using HRManagement.DTOs;
using HRManagement.Services.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Task = System.Threading.Tasks.Task;

namespace HRManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [Authorize(Roles = "ADMIN,HR,MANAGE")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUserAsync(id);
            return user is null ? NotFound() : Ok(user);
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPost]
        public async Task<IActionResult> CreateUser(CreateUserDTO dto)
        {
            var (success, error, username) = await _userService.CreateUserAsync(dto);
            if (!success)
                return BadRequest(error);

            return Ok(new { message = "User created successfully", username });
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO dto)
        {
            var (success, error) = await _userService.UpdateUserAsync(id, dto);
            if (error is null && !success)
                return NotFound();
            if (!success)
                return BadRequest(error);

            return Ok("User updated successfully");
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/deactivate")]
        public async Task<IActionResult> DeactivateUser(int id)
        {
            var (success, _) = await _userService.DeactivateUserAsync(id);
            return success ? Ok("User deactivated") : NotFound();
        }

        [Authorize(Roles = "ADMIN")]
        [HttpPatch("{id}/activate")]
        public async Task<IActionResult> ActivateUser(int id)
        {
            var (success, error) = await _userService.ActivateUserAsync(id);
            if (error is null && !success)
                return NotFound();
            if (!success)
                return BadRequest(error);

            return Ok("User activated");
        }
    }
}
