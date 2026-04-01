using AngularTodoAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace AngularTodoAPI.Data
{
    public class TodoContext : DbContext
    {
        // Inside context, specify what tables we want to have in our database
        public TodoContext(DbContextOptions<TodoContext> options) : base(options) { }
        public DbSet<TodoItem> Todos => Set<TodoItem>();

    }
}
