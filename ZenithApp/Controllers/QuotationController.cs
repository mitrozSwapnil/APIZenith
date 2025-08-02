using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class QuotationController : BaseController
    {
        private readonly QuotationRepository _quotationRepository;
        private readonly IHttpContextAccessor _acc;

        public QuotationController(QuotationRepository QuotationRepository, IHttpContextAccessor acc)
        {
            _quotationRepository = QuotationRepository;
            _acc = acc;
        }


        [HttpPost("CreateQuotation")]
        public IActionResult CreateQuotation(createQuotationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<createQuotationResponse>(model);
        }


        [HttpPost("GetMandaysbyapplicationId")]
        public IActionResult GetMandaysbyapplicationId(getmandaysbyapplicationIdRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getCretificationsbyAppIdResponse>(model);
        }

        protected override BaseResponse Execute(string action, BaseRequest request)
        {
            if (action == nameof(CreateQuotation))
            {
                return _quotationRepository.CreateQuotation(request as createQuotationRequest).Result;
            }
            else if (action == nameof(GetMandaysbyapplicationId))
            {
                return _quotationRepository.GetMandaysbyapplicationId(request as getmandaysbyapplicationIdRequest).Result;
            }

            throw new NotImplementedException();
        }
        
        protected override void Disposing()
        {
            this._quotationRepository.Dispose();
        }
    }
}
