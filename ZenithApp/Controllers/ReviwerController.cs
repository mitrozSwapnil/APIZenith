using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviwerController : BaseController
    {
        private readonly ReviwerRepository _reviwerRepository;
        private readonly IHttpContextAccessor _acc;

        public ReviwerController(ReviwerRepository reviwerRepository, IHttpContextAccessor acc)
        {
            _reviwerRepository = reviwerRepository;
            _acc = acc;
        }

        [HttpPost("GetReviewerDashboard")]
        public IActionResult GetReviewerDashboard(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getDashboardResponse>(model);
        }


        [HttpPost("GetReviewerApplication")]
        public IActionResult GetReviewerApplication(getReviewerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getReviewerApplicationResponse>(model);
        }
        
        [HttpPost("GetApplicationHistory")]
        public IActionResult GetApplicationHistory(getApplicationHistoryRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getReviewerApplicationResponse>(model);
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
        [HttpPost("AddFieldComment")]
        public IActionResult AddFieldComment(FieldCommentRequest model)
        {
             var claims = HttpContext.User.Claims;
             var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
             var UserId = userNameDetails.Value;
             _acc.HttpContext?.Session.SetString("UserId", UserId);
             return this.ProcessRequest<BaseResponse>(model);
        }


        protected override BaseResponse Execute(string action, BaseRequest request)
        {

            if (action == nameof(GetReviewerDashboard))
            {
                return _reviwerRepository.GetReviewerDashboard(request as getDashboardRequest).Result;
            }
            else if (action == nameof(GetReviewerApplication))
            {
                return _reviwerRepository.GetReviewerApplication(request as getReviewerApplicationRequest).Result;
            }
            else if (action == nameof(GetApplicationHistory))
            {
                return _reviwerRepository.GetApplicationHistory(request as getApplicationHistoryRequest).Result;
            }
            else if (action == nameof(SaveISOApplication))
            {
                return _reviwerRepository.SaveISOApplication(request as addReviewerApplicationRequest).Result;
            }
            else if (action == nameof(SaveICMEDApplication))
            {
                return _reviwerRepository.SaveICMEDApplication(request as addICMEDApplicationRequest).Result;
            }
            else if (action == nameof(SaveICMED_Plus_Application))
            {
                return _reviwerRepository.SaveICMED_Plus_Application(request as addICMEDApplicationRequest).Result;
            }

            else if (action == nameof(SaveFSSCApplication))
            {
                return _reviwerRepository.SaveFSSCApplication(request as addFsscApplicationRequest).Result;
            }
            else if (action == nameof(SaveIMDRApplication))
            {
                return _reviwerRepository.SaveIMDRApplication(request as addIMDRApplicationRequest).Result;
            }
            else if (action == nameof(AddFieldComment))
            {
                return _reviwerRepository.AddFieldComment(request as FieldCommentRequest).Result;
            }





            throw new NotImplementedException();
            
        }
        protected override void Disposing()
        {
            this._reviwerRepository.Dispose();
        }
    }
}
