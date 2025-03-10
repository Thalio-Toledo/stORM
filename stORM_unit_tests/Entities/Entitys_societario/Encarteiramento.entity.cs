using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("SocietarioDigital..TB_ENCARTEIRAMENTO")]
public class Encarteiramento
{
    [Key]
    public int Id { get; set; }
    public bool Ativo { get; set; }
    public DateTime DataInclusao { get; set; }
    public int idAuxFluxo { get; set; }
    public int IdUsuarioInclusao { get; set; }

    [ForeignkeyFrom("UsuarioBackoffice")]
    public int IdUsuario { get; set; }
    public UsuarioBackoffice UsuarioBackoffice { get; set; }

    [ForeignkeyFrom("Empresa")]
    public int IdEmpresa { get; set; }
    public Empresa Empresa { get; set; }
}
