using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class Order
{

    [Key]
    public required int OrderId { get; set; }

    //Add UserId when auth added

    public required decimal TaxPaid { get; set; }

    public required decimal ShippingPaid { get; set; }

    public required decimal TotalPaid { get; set; }

    [MaxLength(100)]
    public required String ShippingAddress { get; set; }

    [MaxLength(80)]
    public String? ShippingName { get; set; }

    [Length(4, 4)]
    public String? CreditCardLastFourDigits { get; set; }

    public required DateTime OrderDate { get; set; }
}
