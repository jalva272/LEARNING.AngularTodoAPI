using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularTodoAPI.Data;
using AngularTodoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AngularTodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        private readonly TodoContext _context;

        public TodoController(TodoContext context)
        {
            _context = context;
        }

        [HttpGet] 
        public async Task<ActionResult<IEnumerable<TodoItem>>> Get()
        {
            return await _context.Todos.ToListAsync();
        }

        [HttpGet("{id}")] // HTTP GET api/todo/{id}
        public async Task<ActionResult<TodoItem>> Get(int id) // returns a single TodoItem (or 404)
        {
            var item = await _context.Todos.FindAsync(id); // find by PK
            if (item == null) return NotFound(); // return 404 if not found
            return item; // return the found item (200 OK with JSON body)
        }

    }
}
