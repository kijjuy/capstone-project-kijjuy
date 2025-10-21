using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class CategoryDataModel
{
    [Column("category_id")]
    public int CategoryId { get; set; }

    [Column("category_name")]
    public String? CategoryName { get; set; }
}
