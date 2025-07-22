using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.ZenithEntities;
using ZenithApp.ZenithRepository;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly MasterRepository _repository;

        public MasterController(MasterRepository masterRepository)
        {
            _repository = masterRepository;
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
