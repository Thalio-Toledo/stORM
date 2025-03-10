using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("ControlePermissao..TB_MUNICIPIO")]
public class Municipio
{
    [Key]
    public int Id { get; set; }
    public string Descricao { get; set; }
    public string UF { get; set; }
}
