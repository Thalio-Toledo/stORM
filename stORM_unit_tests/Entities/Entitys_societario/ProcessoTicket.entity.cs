using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_TICKET")]
public class ProcessoTicket
{
    [Key]
    public int Id { get; set; }
    public int IdTicket { get; set; }
    public int IdAuxTipoTicket { get; set; }

    [ForeignkeyFrom("Processo")]
    public int IdProcesso { get; set; }
    public bool Ativo { get; set; }

}
