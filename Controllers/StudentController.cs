using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZenithApp.Models;
using ZenithApp.Services;

namespace ZenithApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentController : ControllerBase
    {
        private readonly StudentService _service;

        public StudentController(StudentService service)
        {
            _service = service;
        }

        [HttpGet]
        public ActionResult<List<Student>> Get() => _service.GetAll();

        //[HttpGet("{id}")]
        //public ActionResult<Student> Get(int id)
        //{
        //    var student = _service.GetById(id);
        //    if (student == null) return NotFound();
        //    return student;
        //}

        [HttpPost]
        public ActionResult<Student> Create(Student student)
        {
            _service.Create(student);
            return CreatedAtAction(nameof(Get), student);
        }

        //[HttpPut("{id}")]
        //public IActionResult Update(int id, Student student)
        //{
        //    var existing = _service.GetById(id);
        //    if (existing == null) return NotFound();
        //    _service.Update(id, student);
        //    return NoContent();
        //}

        //[HttpDelete("{id}")]
        //public IActionResult Delete(int id)
        //{
        //    var student = _service.GetById(id);
        //    if (student == null) return NotFound();
        //    _service.Delete(id);
        //    return NoContent();
        //}
    }
}
