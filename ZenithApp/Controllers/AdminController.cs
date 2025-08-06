using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class AdminController : BaseController
    {
        private readonly AdminRepository _adminRepository;
        private readonly IHttpContextAccessor _acc;

        public AdminController(AdminRepository adminRepository, IHttpContextAccessor acc)
        {
            _adminRepository = adminRepository;
            _acc = acc;
        }

        [HttpPost("GetAdminDashboard")]
        public IActionResult GetAdminDashboard(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getDashboardResponse>(model);
        }

        [HttpPost("GetAdminApplication")]
        public IActionResult GetAdminApplication(getReviewerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getReviewerApplicationResponse>(model);
        }

        [HttpPost("AssignReviewerTwoApplication")]
        public IActionResult AssignReviewerTwoApplication(assignUserRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<assignUserResponse>(model);
        }

        [HttpPost("AssignApplication")]
        public IActionResult AssignApplication(assignUserRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getDashboardResponse>(model);
        }

         [HttpPost("GetDropdown")]
        public IActionResult GetDropdown(userDropdownRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<userDropdownResponse>(model);
        }

        protected override BaseResponse Execute(string action, BaseRequest request)
        {

            if (action == nameof(GetAdminDashboard))
            {
                return _adminRepository.GetAdminDashboard(request as getDashboardRequest).Result;
            }
            else if (action == nameof(AssignApplication))
            {
                return _adminRepository.AssignApplication(request as assignUserRequest).Result;
            }
            else if (action == nameof(GetDropdown))
            {
                return _adminRepository.GetDropdown(request as userDropdownRequest).Result;
            }
            else if (action == nameof(GetAdminApplication))
            {
                return _adminRepository.GetAdminApplication(request as getReviewerApplicationRequest).Result;
            }
            else if (action == nameof(AssignReviewerTwoApplication))
            {
                return _adminRepository.AssignReviewerTwoApplication(request as assignUserRequest).Result;
            }
             

            throw new NotImplementedException();
        }









        protected override void Disposing()
        {
            this._adminRepository.Dispose();
        }


    }
}
