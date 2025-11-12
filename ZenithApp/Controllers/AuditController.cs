using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuditController : BaseController
    {
        private readonly AuditRepository _auditRepository;
        private readonly IHttpContextAccessor _acc;

        public AuditController(AuditRepository auditRepository, IHttpContextAccessor acc)
        {
            _auditRepository = auditRepository;
            _acc = acc;
        }
       
        [HttpPost("CreateAudit")]
        public IActionResult CreateAudit(AuditRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }
        [HttpPost("GenerateFileNumber")]
        public IActionResult GenerateFileNumber(BaseRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<AuditResponse>(model);
        }
        [HttpPost("GetAuditDashboard")]
        public IActionResult GetAuditDashboard(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getAuditResponse>(model);
        }
        [HttpPost("GetDynamicAuditTemplate")]
        public IActionResult GetDynamicAuditTemplate(GetDynamicAuditRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }

        [HttpPost("CreateDynamicAuditForm")]
        public IActionResult CreateDynamicAuditForm(formRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetDynamicAuditFormResponse>(model);
        }
        [HttpPost("SaveAuditNomination")]
        public IActionResult SaveAuditNomination(AssignAuditRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }
        [HttpPost("GetAuditNomination")]
        public IActionResult GetAuditNomination(getAuditNominationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getAuditNominationResponse>(model);
        }
         [HttpPost("GetAuditorList")]
        public IActionResult GetAuditorList(BaseRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetAuditorListResponse>(model);
        }
        [HttpPost("SaveCompetency")]
        public IActionResult SaveCompetency(CompetencyRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }
        [HttpPost("GetCompetency")]
        public IActionResult GetCompetency(GetCompetencyRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetCompetencyResponse>(model);
        }
        [HttpPost("SaveAuditAdministration")]
        public IActionResult SaveAuditAdministration(AuditAdministrationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }
        [HttpPost("GetAuditAdministration")]
        public IActionResult GetAuditAdministration(GetAuditAdministrationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetAuditAdministrationResponse>(model);
        }
        [HttpPost("SaveAuditAdministrationTechnical")]
        public IActionResult SaveAuditAdministrationTechnical(SaveAuditTechRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<SaveAuditTechResponse>(model);
        }
        [HttpPost("GetAuditTechnicalReview")]
        public IActionResult GetAuditTechnicalReview(GetAuditAdministrationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetAuditTechnicalReviewResponse>(model);
        }
        [HttpPost("SaveAuditProcess")]
        public IActionResult SaveAuditProcess(saveAuditProcessRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<BaseResponse>(model);
        }
         [HttpPost("GetAuditProcess")]
        public IActionResult GetAuditProcess(GetAuditProcessRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetAuditProcessResponse>(model);
        }
        [HttpPost("GetRecentUpdates")]
        public IActionResult GetRecentUpdates(GetRecentUpdateRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetRecentUpdateResponse>(model);
        }
        [HttpPost("GetAuditSidePannelDetails")]
        public IActionResult GetAuditSidePannelDetails(GetAuditAdministrationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<GetAuditDetailsResponse>(model);
        }
        
        protected override BaseResponse Execute(string action, BaseRequest request)
        {

            if (action == nameof(CreateAudit))
            {
                return _auditRepository.CreateAudit(request as AuditRequest).Result;
            }
            else if (action == nameof(GenerateFileNumber))
            {
                return _auditRepository.GenerateFileNumber(request as BaseRequest).Result;
            }
            else if (action == nameof(GetAuditDashboard))
            {
                return _auditRepository.GetAuditDashboard(request as getDashboardRequest).Result;
            }
            else if (action == nameof(GetDynamicAuditTemplate))
            {
                return _auditRepository.GetDynamicAuditTemplate(request as GetDynamicAuditRequest).Result;
            }
            else if (action == nameof(CreateDynamicAuditForm))
            {
                return _auditRepository.CreateDynamicAuditForm(request as formRequest).Result;
            }
            else if (action == nameof(SaveAuditNomination))
            {
                return _auditRepository.SaveAuditNomination(request as AssignAuditRequest).Result;
            }
            else if (action == nameof(GetAuditNomination))
            {
                return _auditRepository.GetAuditNomination(request as getAuditNominationRequest).Result;
            }
            else if (action == nameof(GetAuditorList))
            {
                return _auditRepository.GetAuditorList(request as BaseRequest).Result;
            }
            else if (action == nameof(SaveCompetency))
            {
                return _auditRepository.SaveCompetency(request as CompetencyRequest).Result;
            }
            else if (action == nameof(GetCompetency))
            {
                return _auditRepository.GetCompetency(request as GetCompetencyRequest).Result;
            }
            else if (action == nameof(SaveAuditAdministration))
            {
                return _auditRepository.SaveAuditAdministration(request as AuditAdministrationRequest).Result;
            }
            else if (action == nameof(GetAuditAdministration))
            {
                return _auditRepository.GetAuditAdministration(request as GetAuditAdministrationRequest).Result;
            }
            else if (action == nameof(SaveAuditAdministrationTechnical))
            {
                return _auditRepository.SaveAuditAdministrationTechnical(request as SaveAuditTechRequest).Result;
            }
            else if (action == nameof(GetAuditTechnicalReview))
            {
                return _auditRepository.GetAuditTechnicalReview(request as GetAuditAdministrationRequest).Result;
            }
            else if (action == nameof(SaveAuditProcess))
            {
                return _auditRepository.SaveAuditProcess(request as saveAuditProcessRequest).Result;
            }
            else if (action == nameof(GetAuditProcess))
            {
                return _auditRepository.GetAuditProcess(request as GetAuditProcessRequest).Result;
            }
            else if (action == nameof(GetRecentUpdates))
            {
                return _auditRepository.GetRecentUpdates(request as GetRecentUpdateRequest).Result;
            }
            else if (action == nameof(GetAuditSidePannelDetails))
            {
                return _auditRepository.GetAuditSidePannelDetails(request as GetAuditAdministrationRequest).Result;
            }

            throw new NotImplementedException();

        }
        protected override void Disposing()
        {
            this._auditRepository.Dispose();
        }
    }
}
