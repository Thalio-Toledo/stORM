using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entities_custumer;

[Table("DatabaseName..Item")]
public class Item
{
    [Key]
    public Guid Id { get; set; }

    [ForeignkeyFrom("Order")]
    public int OrderId { get; set; }
    public Order Order { get; set; }

    [ForeignkeyFrom("Product")]
    public int ProductId { get; set; }
    public Product Product { get; set; }
    public int Quantity { get; set; }
}
