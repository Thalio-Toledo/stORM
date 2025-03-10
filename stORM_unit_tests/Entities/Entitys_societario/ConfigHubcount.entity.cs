using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonesORMUnitTests.Entities.Entitys_societario
{
    [Table("SocietarioDigital..TB_CONFIG_HUBCOUNT")]
    public class ConfigHubcount
    {
        [Key]
        public int Id { get; set; }
        public string URL { get; set; }
        public string Instance { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
