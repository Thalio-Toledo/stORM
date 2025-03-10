using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_TIPO")]
public class ProcessoTipo
{
    [Key]
    public int Id { get; set; }
    public string Descricao { get; set; }
    public int ReferenciaHubCount { get; set; }
    public string ReferenciaHubCountName { get; set; }
    public int Ordem { get; set; }
    public int IdAuxFluxo { get; set; }
}
