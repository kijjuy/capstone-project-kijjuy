using System.ComponentModel.DataAnnotations;

namespace app.Models;

public class CreateProductModel
{
    [Range(1, 20)]
    public required int CategoryId { get; set; }

    [Length(minimumLength: 3, maximumLength: 100)]
    public required String Name { get; set; }

    [Range(1, 1000)]
    public required decimal Price { get; set; }

    [Length(minimumLength: 10, maximumLength: 1000)]
    public required String Description { get; set; }

    [MinLength(1)]
    [MaxLength(20)]
    public required List<IFormFile> Files { get; set; }
}
