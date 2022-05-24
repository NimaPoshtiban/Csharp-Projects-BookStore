using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookStore.Models;

public class CoverType
{
    [Key]
    public int Id { get; set; }
    [Required]
    [MaxLength(50)]
    [DisplayName("Cover Type")]
    public string Name { get; set; }
}