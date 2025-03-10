using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace stORM_unit_tests.Entities.Entities_custumer;

[Table("DatabaseName..Product")]
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public decimal SecondPrice { get; set; }
}
