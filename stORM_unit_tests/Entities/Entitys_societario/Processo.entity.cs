using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BonesORMUnitTests.Entities.Entitys_societario.Aux_Entitys;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("TB_PROCESSO")]
public class Processo
{
    [Key]
    public int Id { get; set; }

    [ForeignkeyFrom("Empresa")]
    public int IdEmpresa { get; set; }
    public Empresa? Empresa { get; set; }

    [ForeignkeyFrom("ProcessoTipo")]
    public int IdProcessoTipo { get; set; }
    public ProcessoTipo? ProcessoTipo { get; set; }
    public int IdUsuarioResponsavel { get; set; }

    [ForeignkeyFrom("ProcessoStatus")]
    public int IdStatus { get; set; }
    public ProcessoStatus? ProcessoStatus { get; set; }

    [ForeignkeyFrom("ProcessoStatusPendencia")]
    public int IdStatusPendencia { get; set; }
    public ProcessoStatusPendencia ProcessoStatusPendencia { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime DataModificacao { get; set; }
    public int IdUsuarioModificacao { get; set; }
    public string DocumentoUrlDownload { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataPrevista { get; set; }
    public DateTime DataInicio { get; set; }
    public DateTime DataFim { get; set; }
    public int NumeroProcesso { get; set; }
    public int IdAuxTipoMovimentacaoCertidao { get; set; }
    public string? PB { get; set; }

    [ForeignkeyFrom("TipoFluxo")]
    public int IdAuxFluxo { get; set; }
    public TipoFluxo TipoFluxo { get; set; }
    public string? numeroTicket { get; set; }
    public List<ProcessoTicket>? ProcessoTickets { get; set; } = new List<ProcessoTicket>();
    public List<ProcessoHistorico>? ProcessoHistoricos { get; set; } = new List<ProcessoHistorico>();


}
