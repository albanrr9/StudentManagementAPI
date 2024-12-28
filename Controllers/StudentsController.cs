using Microsoft.AspNetCore.Mvc;
using StudentManagementAPI.Data;
using StudentManagementAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace StudentManagementAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly StudentManagementContext _context;

        public StudentsController(StudentManagementContext context)
        {
            _context = context;
        }

        // GET: api/Students
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetStudents()
        {
            return await _context.Students.ToListAsync();
        }

        // GET: api/Students/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);

            if (student == null)
            {
                return NotFound();
            }

            return student;
        }

        // POST: api/Students
        [HttpPost]
        public async Task<ActionResult<Student>> PostStudent(Student student)
        {
            _context.Students.Add(student);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetStudent", new { id = student.StudentId }, student);
        }

        // PUT: api/Students/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStudent(int id, Student student)
        {
            if (id != student.StudentId)
            {
                return BadRequest();
            }

            _context.Entry(student).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Students.Any(e => e.StudentId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Students/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            var student = await _context.Students.FindAsync(id);
            if (student == null)
            {
                return NotFound();
            }

            _context.Students.Remove(student);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Student>>> FilterStudents(
    [FromQuery] string? name,
    [FromQuery] int? minAge,
    [FromQuery] string? sortBy)
        {
            // Start with a queryable collection of students
            var query = _context.Students.AsQueryable();

            // Filter by name (partial match)
            if (!string.IsNullOrEmpty(name))
            {
                query = query.Where(s => s.Name.Contains(name));
            }

            // Filter by minimum age
            if (minAge.HasValue)
            {
                query = query.Where(s => DateTime.Now.Year - s.DateOfBirth.Year >= minAge.Value);
            }

            // Apply sorting based on the query parameter
            if (!string.IsNullOrEmpty(sortBy))
            {
                query = sortBy switch
                {
                    "name" => query.OrderBy(s => s.Name),
                    "age" => query.OrderBy(s => s.DateOfBirth),
                    _ => query // Default: no sorting
                };
            }

            // Execute the query and return the results
            return await query.ToListAsync();
        }

        [HttpGet("with-grades")]
        public async Task<ActionResult<IEnumerable<object>>> GetStudentsWithGrades()
        {
            var studentsWithGrades = await _context.Students
                .Include(s => s.Grades) // Load related grades
                .Select(s => new
                {
                    s.StudentId,
                    s.Name,
                    s.Email,
                    Grades = s.Grades.Select(g => new
                    {
                        g.Subject,
                        g.GradeValue
                    })
                })
                .ToListAsync();

            return Ok(studentsWithGrades);
        }

        [HttpGet("paginated")]
        public async Task<ActionResult<IEnumerable<Student>>> GetPaginatedStudents(
    [FromQuery] int pageNumber = 1,
    [FromQuery] int pageSize = 10)
        {
            var students = await _context.Students
                .Skip((pageNumber - 1) * pageSize) // Skip records based on page number
                .Take(pageSize) // Limit the number of records returned
                .ToListAsync();

            return Ok(students);
        }


    }
}
