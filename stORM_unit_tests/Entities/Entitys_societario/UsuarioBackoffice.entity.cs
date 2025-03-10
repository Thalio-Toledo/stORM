using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("ControlePermissao..TB_USUARIO_BACKOFFICE")]
public class UsuarioBackoffice
{
    [Key]
    public int Id { get; set; }
    public string Nome { get; set; }
    public string Email { get; set; }
    public string Cpf { get; set; }
    public bool? Ativo { get; set; }
    public int? IdAreaPrincipal { get; set; }
    public List<ProcessoHistorico> ProcessoHistoricos { get; set; }
}
