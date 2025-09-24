using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class OrderProduct
{
    [Key]
    public required int OrderproductId { get; set; }

    //Foreign Key
    public required int ProductId { get; set; }

    //Foreign Key
    public required int OrderId { get; set; }
}
