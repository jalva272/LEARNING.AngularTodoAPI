using AngularTodoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularTodoAPI.Data
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }
        public DbSet<TodoItem> Todos { get; set; } 
    }
}
