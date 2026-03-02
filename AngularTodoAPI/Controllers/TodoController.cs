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
        public async Task<ActionResult<TodoItem>> Get(int id)
        {
            var item = await _context.Todos.FindAsync(id);
            if (item == null) return NotFound();
            return item;
        }

        [HttpPost] // HTTP POST api/todo
        public async Task<ActionResult<TodoItem>> Post(TodoItem item)
        {
            _context.Todos.Add(item);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
        }

        [HttpPut("{id}")] // PUT api/todo/{id}
        public async Task<IActionResult> Put(int id, TodoItem item)
        {
            if (id != item.Id) return BadRequest();
            _context.Entry(item).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

    }
}
