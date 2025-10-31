using System.ComponentModel.DataAnnotations.Schema;

namespace app.Models;

public class ImageDataModel
{
    [Column("file_path")]
    public String ImageName { get; set; }
}
