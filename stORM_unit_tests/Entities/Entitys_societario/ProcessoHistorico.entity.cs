using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_HISTORICO")]
public class ProcessoHistorico
{
    [Key]
    public int Id { get; set; }

    [ForeignkeyFrom("UsuarioResponsavel")]
    public int IdUsuarioResponsavel { get; set; }
    public UsuarioBackoffice UsuarioResponsavel { get; set; }
    public string Descricao { get; set; }
    public DateTime Data { get; set; }
    public int IdStatus { get; set; }
    public int IdStatusPendencia { get; set; }
    public DateTime DataPrevista { get; set; }

    [ForeignkeyFrom("UsuarioModificacao")]
    public int IdUsuarioModificacao { get; set; }
    public UsuarioBackoffice UsuarioModificacao { get; set; }
    public string PB { get; set; }
    public string NumeroTicket { get; set; }

    [ForeignkeyFrom("Processo")]
    public int IdProcesso { get; set; }
    public Processo Processo { get; set; }
}
