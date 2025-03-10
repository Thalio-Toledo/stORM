using System.ComponentModel.DataAnnotations.Schema;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_VERSAO")]
public class Versao
{
    public long Id { get; set; } = 0;
    public string Titulo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public DateTime Data { get; set; } = DateTime.Now;
}