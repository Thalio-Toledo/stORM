using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_STATUS_PENDENCIA")]
public class ProcessoStatusPendencia
{
    [Key]
    public int Id { get; set; }
    public string descricao { get; set; }

    [ForeignkeyFrom("ProcessoStatus")]
    public int IdStatus { get; set; }
    public ProcessoStatus? ProcessoStatus { get; set; }
    public int IdAuxFluxo { get; set; }

}
