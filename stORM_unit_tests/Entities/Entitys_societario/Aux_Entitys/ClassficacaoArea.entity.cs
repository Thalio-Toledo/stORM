using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonesORMUnitTests.Entities.Entitys_societario.Aux_Entitys;

[Table("ControlePermissao..TB_ENUM_AUXILIAR")]
public class ClassificacaoArea
{
    public string Configuracao { get; set; }

    [Key]
    public int Chave { get; set; }
    public string Valor { get; set; }
    public string Label { get; set; }
    public int Ordem { get; set; }
}
