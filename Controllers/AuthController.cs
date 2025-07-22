using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.Models;
using ZenithApp.Services;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _service;

        public AuthController(AuthService service)
        {
            _service = service;
        }
        [HttpGet]
        public ActionResult<List<tbl_user>> Get() => _service.GetAllUsers();

        [HttpPost]
        public ActionResult<tbl_user> Create(tbl_user user)
        {
            _service.Create(user);
            return CreatedAtAction(nameof(Get), user);
        }

    }
}
