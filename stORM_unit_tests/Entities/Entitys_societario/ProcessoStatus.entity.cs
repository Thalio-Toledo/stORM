using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_STATUS")]
public class ProcessoStatus
{
    [Key]
    public int Id { get; set; }
    public string label { get; set; }
    public int Ordem { get; set; }
    public string CssClass { get; set; }
    public string IconName { get; set; }
    public string IconStyle { get; set; }
    public bool PermitirAlteracaoManual { get; set; }
    public int IdAuxFluxo { get; set; }
    public List<ProcessoStatusPendencia> ProcessoStatusPendencias { get; set; }
    public List<Processo> Processos { get; set; }
}
