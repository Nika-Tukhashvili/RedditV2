using System.Text.Json.Serialization;

namespace Reddit.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        [JsonIgnore]
        public virtual List<Post> Posts { get; set; } = new();
        public virtual List<Comment> Comments { get; set; } = new();
    }
}
