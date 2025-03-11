using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace stORM_unit_tests.Entities.Entities_custumer;

[Table("DatabaseName..Custumer")]
public class Custumer
{
    [Key]
    public int Id { get; set; } = 0;
    public string Name { get; set; } = "";
    public bool Active { get; set; } = true;
    public DateTime Date { get; set; }

    [ForeignkeyFrom("Address")]
    public int AddressId { get; set; } = 0;
    public Address Address { get; set; }
    public List<Order> Orders { get; set; }
}
