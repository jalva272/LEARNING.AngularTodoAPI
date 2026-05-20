using AngularTodoAPI.Data;
using AngularTodoAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace AngularTodoAPI.Controllers
{
    [Authorize] // require authentication for all endpoints in this controller
    [ApiController]
    [Route("api/[controller]")] // why api/[controller]? [controller] is a placeholder that will be replaced by the name of the controller class without the "Controller" suffix. So in this case, it will be "api/todo".
    public class TodoController : ControllerBase
    {
        // declare variable to store database context
        private readonly TodoContext _db;

        public TodoController(TodoContext context)
        {
            // initialize database context
            _db = context;
        }

        private int GetCurrentUserId()
        {
            var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idValue, out var id)) return id;
            throw new UnauthorizedAccessException("User id claim missing or invalid.");
        }


        // Read all tasks
        [HttpGet] // GET api/todo
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll()
        {
            var currentUserId = GetCurrentUserId();
            // build query string
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_GetAll @p0", currentUserId)
                           .AsNoTracking();

            // execute query
            var rows = await query.ToListAsync();

            // return response to frontend
            return Ok(rows);
        }


        // Read a task
        [HttpGet("{id:int}")] // GET api/todo/{id}
        public async Task<ActionResult<TodoItem>> GetById(int id)
        {
            var currentUserId = GetCurrentUserId();
            // build query string
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_GetById @p0, @p1", id, currentUserId)
                           .AsNoTracking();

            // execute query
            var rows = await query.ToListAsync();

            // get first row
            var row = rows.FirstOrDefault();

            // return response with row if found, otherwise indicate row not found
            return row is null ? NotFound() : Ok(row);
        }


        // Create task
        [HttpPost] // POST api/todo
        public async Task<ActionResult<TodoItem>> Create([FromBody] TodoItem dto)
        {
            var currentUserId = GetCurrentUserId();
            // build query string
            var query = _db.Todos
                              .FromSqlRaw("EXEC dbo.Todo_Create @p0, @p1, @p2", dto.Title, dto.IsComplete, currentUserId)
                              .AsNoTracking();

            // execute query
            var rows = await query.ToListAsync();

            // get first row
            var row = rows.FirstOrDefault();

            // check if a row is returned
            if (row is null) return StatusCode(500, "Create failed.");

            // return response with row created
            return CreatedAtAction(nameof(GetById), new { id = row.Id }, row);
        }


        // Update a task
        [HttpPut("{id:int}")] // PUT api/todo/{id}
        public async Task<ActionResult<TodoItem>> Update(int id, [FromBody] TodoItem req)
        {
            var currentUserId = GetCurrentUserId();
            // prepare the query
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_Update @p0, @p1, @p2, @p3", id, req.Title, req.IsComplete, currentUserId)
                           .AsNoTracking();

            // execute
            var rows = await query.ToListAsync();

            // get first row
            var updatedRow = rows.FirstOrDefault();

            // check if row returned
            if (updatedRow is null)
            {
                return NotFound();
            }

            // return response with the updated row
            return Ok(updatedRow);
        }


        // Delete a task
        [HttpDelete("{id:int}")] // DELETE api/todo/{id}
        public async Task<IActionResult> Delete(int id)
        {
            var currentUserId = GetCurrentUserId();
            // execute query statement and return only number of affected rows
            var rowsAffected = await _db.Database
                                       // returns the number of rows affected
                                       .ExecuteSqlRawAsync("EXEC dbo.Todo_Delete @p0, @p1", id, currentUserId);

            // check any rows affected
            if (rowsAffected == 0)
                return NotFound();

            // row deleted
            return NoContent();
        }

    }
}