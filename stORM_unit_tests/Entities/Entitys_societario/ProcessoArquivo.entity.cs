using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_PROCESSO_ARQUIVO")]
public class ProcessoArquivo
{
    [Key]
    public int Id { get; set; }
    public string NomeArquivo { get; set; }
    public string FileBase64 { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }

    [ForeignkeyFrom("Processo")]
    public int IdProcesso { get; set; }
    public Processo Processo { get; set; }

}
