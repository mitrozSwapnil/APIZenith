using gmkRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.Models;
using ZenithApp.Services;
using ZenithApp.ZenithMessage;
using LoginRequest = ZenithApp.ZenithMessage.LoginRequest;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly AuthRepository _authRepository;

        public AuthController(AuthRepository authRepository)
        {
            _authRepository = authRepository;
        }

       
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest model)
        {
            return this.ProcessRequest<LoginResponse>(model);
        }

       

        private IActionResult ProcessRequest<T>(LoginRequest model)
        {
            try
            {
                var result = _authRepository.Login(model);
                return Ok(result);
                
                return BadRequest("Invalid response type.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("VerifyOtp")]
        public IActionResult VerifyOtp([FromBody] VerifyOtpRequest model)
        {
            try
        {
                var result = _authRepository.VerifyOtp(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            throw new NotImplementedException();
        }
    }

}
