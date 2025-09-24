using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Category
{
    [Key]
    public required int CategoryId { get; set; }

    [MaxLength(30)]
    public required String CategoryName { get; set; }
}
