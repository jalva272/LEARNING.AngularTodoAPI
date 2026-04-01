using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AngularTodoAPI.Data;
using AngularTodoAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace AngularTodoAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TodoController : ControllerBase
    {
        // declare variable to store database context
        private readonly TodoContext _db;

        public TodoController(TodoContext context)
        {
            // initialize database context
            _db = context;
        }


        // Read all tasks
        [HttpGet] // GET api/todo
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetAll()
        {
            // build query string
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_GetAll")
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
            // build query string
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_GetById @p0", id)
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
            // build query string
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_Create @p0, @p1", dto.Title, dto.IsComplete)
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
            // prepare the query
            var query = _db.Todos
                           .FromSqlRaw("EXEC dbo.Todo_Update @p0, @p1, @p2", id, req.Title, req.IsComplete)
                           // add meta data to the query
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
            // execute query statement and return only number of affected rows
            var rowsAffected = await _db.Database
                                       // returns the number of rows affected
                                       .ExecuteSqlRawAsync("EXEC dbo.Todo_Delete @p0", id);

            // check any rows affected
            if (rowsAffected == 0)
                return NotFound();

            // row deleted
            return NoContent();
        }

    }
}





