using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithEntities;
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
        //public ActionResult<List<tbl_master_designation>> Getdesignation()
        //{
        //    var data = _adminRepository.Getdesignation();

        //    if (data == null || data.Count == 0)
        //        return NotFound();

        //    return Ok(data);
        //}



        //[HttpPost("Add-designation")]
        //public ActionResult<tbl_master_designation> CreateMasterDesignation(tbl_master_designation designation)
        //{
        //    _adminRepository.CreateMasterDesignation(designation);
        //    return CreatedAtAction(nameof(Getdesignation), designation);
        //}

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

        [HttpPost("SaveISOApplication")]
        public IActionResult SaveISOApplication(addReviewerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
        }
        [HttpPost("SaveFSSCApplication")]
        public IActionResult SaveFSSCApplication(addFsscApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
        }
        [HttpPost("SaveICMEDApplication")]
        public IActionResult SaveICMEDApplication(addICMEDApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
        }
        [HttpPost("SaveICMED_Plus_Application")]
        public IActionResult SaveICMED_Plus_Application(addICMEDApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
        }

        [HttpPost("SaveIMDRApplication")]
        public IActionResult SaveIMDRApplication(addIMDRApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
        }
        [HttpPost("GetHistory")]
        public IActionResult GetHistory(gethistoryRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<gethistoryResponse>(model);
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
            else if (action == nameof(SaveISOApplication))
            {
                return _adminRepository.SaveISOApplication(request as addReviewerApplicationRequest).Result;
            }
            else if (action == nameof(SaveISOApplication))
            {
                return _adminRepository.SaveFSSCApplication(request as addFsscApplicationRequest).Result;
            }
            else if (action == nameof(SaveICMEDApplication))
            {
                return _adminRepository.SaveICMEDApplication(request as addICMEDApplicationRequest).Result;
            }
            else if (action == nameof(SaveICMED_Plus_Application))
            {
                return _adminRepository.SaveICMED_Plus_Application(request as addICMEDApplicationRequest).Result;
            }

            else if (action == nameof(SaveIMDRApplication))
            {
                return _adminRepository.SaveIMDRApplication(request as addIMDRApplicationRequest).Result;
            }
            else if (action == nameof(GetHistory))
            {
                return _adminRepository.GetHistory(request as gethistoryRequest).Result;
            }

            throw new NotImplementedException();
        }









        protected override void Disposing()
        {
            this._adminRepository.Dispose();
        }


    }
}
