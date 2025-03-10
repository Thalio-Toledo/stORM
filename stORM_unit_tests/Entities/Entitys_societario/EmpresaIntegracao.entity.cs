using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario
{
    [Table("TB_EMPRESA_INTEGRACAO")]
    public class EmpresaIntegracao
    {
        [Key]
        public int Id { get; set; }

        [ForeignkeyFrom("Empresa")]
        public int IdEmpresa { get; set; }
        public Empresa Empresa { get; set; }
        public int IdSituacao { get; set; }
        public bool PossuiCertificado { get; set; }
        public DateTime? DataCriacao { get; set; }
        public DateTime? DataUltimaImportacao { get; set; }
        public DateTime? DataExpiracao { get; set; }
        public int? IdHubCount { get; set; }
        public bool? IsAtivo { get; set; }
        public DateTime? DataInclusaoHubcount { get; set; }
        public DateTime? DataExclusaoHubcount { get; set; }

    }
}
