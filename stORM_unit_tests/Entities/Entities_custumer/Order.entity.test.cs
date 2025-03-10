using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BonesORMUnitTests.Entities.Entities_custumer.Enums;
using stORM.DataAnotattions;
using stORM_unit_tests.Entities.Entities_custumer;

namespace BonesORMUnitTests.Entities.Entities_custumer;

[Table("DatabaseName..Order")]
public class Order
{
    [Key]
    public int Id { get; set; }
    public DateTime CreationDate { get; set; }
    public Status Status { get; set; }

    [ForeignkeyFrom("Custumer")]
    public int CustumerId { get; set; }
    public Custumer Custumer { get; set; }
    public List<Item> Items { get; set; }
}
