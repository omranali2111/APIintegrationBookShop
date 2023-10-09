using System.ComponentModel.DataAnnotations;

namespace APIintegrationBookShop.models
{
    public class Patron
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string Name { get; set; }

        [MaxLength(255)]
        public string ContactNumber { get; set; }

        [MaxLength(255)]
        public int Age { get; set; }

    }
}
