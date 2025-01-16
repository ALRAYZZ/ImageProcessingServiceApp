using ImageProcessingService.Models;
using ImageProcessingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessingService.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class UsersController : Controller
	{
		private readonly UserServices _userServices;

		public UsersController(UserServices userServices)
		{
			_userServices = userServices;
		}

		[HttpPost("register")]
		[AllowAnonymous]
		public async Task<ActionResult<UserModel>> RegisterUser(RegisterUser registerModel)
		{
			var userModel = new UserModel()
			{
				Username = registerModel.Username,
				Password = registerModel.Password
			};

			var createdUser = await _userServices.RegisterUserAsync(userModel);
			return Ok(createdUser);
		}


		[HttpPost("login")]
		[AllowAnonymous]
		public async Task<IActionResult> LoginUser(LoginModel loginModel)
		{
			var user = await _userServices.LoginUserAsync(loginModel);
			if (user == null)
			{
				return Unauthorized("Wrong username or password");
			}
			var token = _userServices.GenerateJwtToken(user);
			return Ok(token);
		}
	}
}
