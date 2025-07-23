using gmkRepositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.Models;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = "Bearer")]
    public class CustomerController : BaseController
    {
        private readonly CustomerRepository _customerRepository;
        private readonly IHttpContextAccessor _acc;

        public CustomerController(CustomerRepository customerRepository, IHttpContextAccessor acc)
        {
            _customerRepository = customerRepository;
            _acc = acc;
        }

        [HttpGet]
        public ActionResult<List<tbl_customer_application>> Get() => _customerRepository.GetAll();

        [HttpPost]
        public ActionResult<tbl_customer_application> Create(tbl_customer_application customer_Application)
        {
            _customerRepository.Create(customer_Application);
            return CreatedAtAction(nameof(Get), customer_Application);
        }

        [HttpGet("customer-certificates")]
        public ActionResult<List<tbl_customer_certificates>> GetCustomerCertificates()
        {
            var data = _customerRepository.GetCustomerCertificates();

            if (data == null || data.Count == 0)
                return NotFound();

            return Ok(data);
        }



        [HttpPost("Add-certificates")]
        public ActionResult<tbl_customer_certificates> CreateMasterCertificate(tbl_customer_certificates customer_Certificates)
        {
            _customerRepository.CreateCustomerCertification(customer_Certificates);
            return CreatedAtAction(nameof(GetCustomerCertificates), customer_Certificates);
        }


        [HttpPost("AddCustomerApplication")]
        public IActionResult AddCustomerApplication(addCustomerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addCustomerApplicationResponse>(model);
        }

        [HttpPost("GetCustomerApplication")]
        public IActionResult GetCustomerApplication(getCustomerApplicationRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getCustomerApplicationResponse>(model);
        }

        [HttpPost("GetCustomerDashboard")]
        public IActionResult GetCustomerDashboard(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getDashboardResponse>(model);
        }

        [HttpPost("getCretificationsbyAppId")]
        public IActionResult getCretificationsbyAppId(getCretificationsbyAppIdRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<getCretificationsbyAppIdResponse>(model);
        }


        [HttpPost("CreateCustomerApplication")]
        public IActionResult CreateCustomerApplication(getDashboardRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<addCustomerApplicationResponse>(model);
        }
         [HttpPost("GetAllDropdown")]
        public IActionResult GetAllDropdown(userDropdownRequest model)
        {
            var claims = HttpContext.User.Claims;
            var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
            var UserId = userNameDetails.Value;
            _acc.HttpContext?.Session.SetString("UserId", UserId);
            return this.ProcessRequest<userDropdownResponse>(model);
        }





        protected override BaseResponse Execute(string action, BaseRequest request)
        {

            if (action == nameof(AddCustomerApplication))
            {
                return _customerRepository.AddCustomerApplication(request as addCustomerApplicationRequest);
            }
            else if(action == nameof(GetCustomerApplication))
            {
                return _customerRepository.GetCustomerApplication(request as getCustomerApplicationRequest);
            }
            else if (action == nameof(GetCustomerDashboard))
            {
                return _customerRepository.GetCustomerDashboard(request as getDashboardRequest).Result;
            }
            else if (action == nameof(getCretificationsbyAppId))
            {
                return _customerRepository.getCretificationsbyAppId(request as getCretificationsbyAppIdRequest).Result;
            }
            else if (action == nameof(CreateCustomerApplication))
            {
                return _customerRepository.CreateCustomerApplication(request as getDashboardRequest).Result;
            }
            else if (action == nameof(GetAllDropdown))
        {
                return _customerRepository.GetAllDropdown(request as userDropdownRequest).Result;
            }


            throw new NotImplementedException();
        }

        protected override void Disposing()
        {
            this._customerRepository.Dispose();
        }

        
    }
}
