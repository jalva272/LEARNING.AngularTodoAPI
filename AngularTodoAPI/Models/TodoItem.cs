using System;
using System.ComponentModel.DataAnnotations;

namespace AngularTodoAPI.Models
{
    public class TodoItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; }

        public bool IsComplete { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}