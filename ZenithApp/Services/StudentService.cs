using Microsoft.Extensions.Options;
using MongoDB.Driver;
using ZenithApp.Models;
using ZenithApp.Settings;

namespace ZenithApp.Services
{
    public class StudentService
    {
        private readonly IMongoCollection<Student> _students;

        public StudentService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _students = database.GetCollection<Student>("Students");
        }
        public List<Student> GetAll() => _students.Find(s => true).ToList();

        //public Student? GetById(int id) =>
        //_students.Find(s => s.Id == id).FirstOrDefault();

        public void Create(Student student) =>
            _students.InsertOne(student);

        //public void Update(int id, Student student) =>
        //    _students.ReplaceOne(s => s.Id == id, student);

        //public void Delete(int id) =>
        //    _students.DeleteOne(s => s.Id == id);
    }
}
