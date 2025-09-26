using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class ProductDataModel
{


    [Key]
    [Column("product_id")]
    public long ProductId { get; set; }

    [Column("category_id")]
    public long CategoryId { get; set; }

    [MaxLength(50)]
    [Column("name")]
    public String? Name { get; set; }

    [Column("price")]
    public double Price { get; set; }

    [Column("description")]
    public String? Description { get; set; }

    [Column("creation_date")]
    public DateTime CreationDate { get; set; }

    [Column("update_date")]
    public DateTime UpdateDate { get; set; }

    [Column("is_available")]
    public long IsAvailable { get; set; }
}
