using System.ComponentModel.DataAnnotations;

namespace AngularTodoAPI.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public string UserName { get; set; }

        public string PasswordHash { get; set; } = null!; // null! means that we are telling the compiler that we will ensure this property is not null, even though it is not initialized here. This is often used when the property will be set through other means, such as through a constructor or by an ORM like Entity Framework.

        public string PasswordSalt { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
