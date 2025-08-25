using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithEntities;
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


        //[HttpPost("AddFees")]
        //public IActionResult AddFees([FromBody] tbl_master_quotation_fees fees)
        //{
        //    if (fees == null)
        //        return BadRequest("Invalid data.");

        //    _quotationRepository.AddFees(fees); 

        //    return Ok(new { message = "Successfully added" });
        //}

        [HttpPost("AddTerms")]
        public IActionResult AddTerms([FromBody] tbl_master_terms term)
        {
            if (term == null)
                return BadRequest("Invalid data.");

            _quotationRepository.AddTerms(term);

            return Ok(new { message = "Successfully added" });
        }


        [HttpPost("GetQuotationDashboard")]
        public IActionResult GetQuotationDashboard(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getQuotationDashboardResponse>(model);
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


        [HttpPost("GetQuotation")]
        public IActionResult GetQuotation(getmandaysbyapplicationIdRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getmandaysbyapplicationIdResponse>(model);
        }

        [HttpPost("GetQuotationPreview")]
        public IActionResult GetQuotationPreview(getmandaysbyapplicationIdRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getmandaysbyapplicationIdResponse>(model);
        }


        protected override BaseResponse Execute(string action, BaseRequest request)
        {
            if (action == nameof(CreateQuotation))
            {
                return _quotationRepository.CreateQuotation(request as createQuotationRequest).Result;
            }
            else if (action == nameof(GetQuotation))
            {
                return _quotationRepository.GetQuotation(request as getmandaysbyapplicationIdRequest).Result;
            }
            else if (action == nameof(GetQuotationPreview))
            {
                return _quotationRepository.GetQuotationPreview(request as getmandaysbyapplicationIdRequest).Result;
            }
            else if (action == nameof(GetQuotationDashboard))
            {
                return _quotationRepository.GetQuotationDashboard(request as getDashboardRequest).Result;
            }

            throw new NotImplementedException();
        }
        
        protected override void Disposing()
        {
            this._quotationRepository.Dispose();
        }
    }
}
