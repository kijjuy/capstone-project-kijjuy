using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class Category
{
    [Key]
    [Column("category_id")]
    public long CategoryId { get; set; }

    [MaxLength(30)]
    [Column("category_name")]
    public String CategoryName { get; set; }
}
