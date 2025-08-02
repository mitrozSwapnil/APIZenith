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
        [HttpPost("SaveISOApplication")]
        public IActionResult SaveISOApplication(addReviewerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addReviewerApplicationResponse>(model);
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
            else if (action == nameof(SaveISOApplication))
            {
                return _reviwerRepository.SaveISOApplication(request as addReviewerApplicationRequest).Result;
            }




             throw new NotImplementedException();
            
        }
        protected override void Disposing()
        {
            this._reviwerRepository.Dispose();
        }
    }
}
