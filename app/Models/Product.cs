using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class Product
{
    [Key]
    [Column("product_id")]
    public required int ProductId { get; set; }

    [Column("category_id")]
    public required int CategoryId { get; set; }

    [MaxLength(50)]
    [Column("name")]
    public required String Name { get; set; }

    [Column("price")]
    public required decimal Price { get; set; }

    [Column("description")]
    public String? Description { get; set; }

    [Column("creation_date")]
    public required DateTime CreationDate { get; set; }

    [Column("update_date")]
    public DateTime? UpdateDate { get; set; }

    [Column("is_available")]
    public required bool IsAvailable { get; set; }
}
