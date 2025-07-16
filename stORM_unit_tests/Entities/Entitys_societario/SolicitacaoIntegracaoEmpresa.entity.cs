using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stORM_unit_tests.Entities.Entitys_societario
{
    [Table("TB_SOLICITACAO_INTEGRACAO_EMPRESA")]
    public class SolicitacaoIntegracaoEmpresa
    {
        [Key]
        public int Id { get; set; } = 0;
        public int IdUsuarioSolicitacao { get; set; } = 1;
        public DateTime SolicitacaoDataInicio { get; set; } = DateTime.Now;
        public DateTime SolicitacaoDataFim { get; set; } = DateTime.Now;
        public bool Ativo { get; set; } = true;
        public CompanyIntegrationTypeEnum SolicitacaoTipo { get; set; } = CompanyIntegrationTypeEnum.INSERT;
        public string Empresas { get; set; }
    }

    public enum CompanyIntegrationTypeEnum
    {
        INSERT = 1,
        DELETE = 2,
    }
}
