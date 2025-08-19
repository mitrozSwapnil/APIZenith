using gmkRepositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithMessage;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly MasterRepository _repository;
        private readonly IHttpContextAccessor _acc;
        public MasterController(MasterRepository masterRepository, IHttpContextAccessor acc)
        {
            _repository = masterRepository;
            _acc = acc;
        }


        [HttpGet("master-certificates")]
        public ActionResult<List<tbl_master_certificates>> GetMasterCertificates()
        {
            var data = _repository.GetAll();

            if (data == null || data.Count == 0)
                return NotFound();

            return Ok(data);
        }

        [HttpGet("product-certificates")]
        public ActionResult<List<tbl_master_product_certificates>> GetProductCertificates()
        {
            var data = _repository.GetAllCertificates();

            if (data == null || data.Count == 0)
                return NotFound();

            return Ok(data);
        }

        [HttpPost("AddMasterAudit")]
        public async Task<IActionResult> AddMasterAudit([FromBody] MasterAuditRequest model)
        {
            try
            {
                var claims = HttpContext.User.Claims;
                var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
                var userId = userNameDetails?.Value;

                _acc.HttpContext?.Session.SetString("UserId", userId);

                var result = await _repository.AddMasterAudit(model); // ✅ Await here
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        [HttpPost("AddMasterTechnicalArea")]
        public async Task<IActionResult> AddMasterTechnicalArea([FromBody] masterTechnicalAreaRequest model)
        {
            try
            {
                var claims = HttpContext.User.Claims;
                var userNameDetails = claims.FirstOrDefault(c => c.Type == "UserId");
                var UserId = userNameDetails.Value;
                _acc.HttpContext?.Session.SetString("UserId", UserId);
                var result =await _repository.AddMasterTechnicalArea(model);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

            throw new NotImplementedException();
        }

        [HttpPost("master-certificates")]
        public ActionResult<tbl_master_certificates> CreateMasterCertificate(tbl_master_certificates master_Certificates)
        {
            _repository.CreateCertificate(master_Certificates);
            return CreatedAtAction(nameof(GetMasterCertificates), master_Certificates);
        }

        [HttpPost("product-certificates")]
        public ActionResult<tbl_master_product_certificates> CreateProductCertificate(tbl_master_product_certificates productCertificate)
        {
            _repository.CreateProductCertificate(productCertificate);
            return CreatedAtAction(nameof(GetProductCertificates), productCertificate);
        }


        [HttpGet("master-remarks")]
        public ActionResult<IEnumerable<tbl_Master_Remark>> GetMasterRemarks()
        {
            var remarks = _repository.GetAllMasterRemarks();
            return Ok(remarks);
        }

        [HttpGet("master-threats")]
        public ActionResult<IEnumerable<tbl_Master_Threat>> GetMasterThreats()
        {
            var threats = _repository.GetAllMasterThreats();
            return Ok(threats);
        }
        [HttpGet("master-designation")]
        public ActionResult<IEnumerable<tbl_master_designation>> GetMasterDesignation()
        {
            var threats = _repository.GetAllMasterDesignation();
            return Ok(threats);
        }
        [HttpPost("master-designation")]
        public ActionResult<tbl_Master_Remark> CreateMasterDesignation(tbl_master_designation designation)
        {
            _repository.CreateMasterDesignation(designation);
            return CreatedAtAction(nameof(GetMasterDesignation), designation);
        }

        [HttpPost("master-remarks")]
        public ActionResult<tbl_Master_Remark> CreateMasterRemark(tbl_Master_Remark masterRemark)
        {
            _repository.CreateMasterRemark(masterRemark);
            return CreatedAtAction(nameof(GetMasterRemarks), masterRemark);
        }
       

        [HttpPost("master-threats")]
        public ActionResult<tbl_Master_Threat> CreateMasterThreat(tbl_Master_Threat masterThreat)
        {
            _repository.CreateMasterThreat(masterThreat);
            return CreatedAtAction(nameof(GetMasterThreats), masterThreat);
        }



    }
}
