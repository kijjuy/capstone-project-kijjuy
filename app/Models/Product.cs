using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Product
{
    [Key]
    public required int ProductId { get; set; }

    public required int CategoryId { get; set; }

    [MaxLength(50)]
    public required String Name { get; set; }

    public required decimal Price { get; set; }

    public String? Description { get; set; }

    public required DateTime CreationDate { get; set; }

    public DateTime? UpdateDate { get; set; }

    public required bool IsAvailable { get; set; }
}
