using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace APIintegrationBookShop.models
{
    public class User
    {
        [JsonIgnore]
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string email { get; set; }

        [Required]
        [MaxLength(255)]
        public string password { get; set; }
    }
}
