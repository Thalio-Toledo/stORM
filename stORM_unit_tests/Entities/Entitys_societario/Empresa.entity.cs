using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using BonesORMUnitTests.Entities.Entitys_societario.Aux_Entitys;
using stORM.DataAnotattions;

namespace BonesORMUnitTests.Entities.Entitys_societario;

[Table("ControlePermissao..TB_EMPRESA")]
public class Empresa
{
    [Key]
    [ForeignkeyFrom("EmpresaIntegracao")]
    public int Id { get; set; } = 0;
    public Guid Guid { get; set; }
    public int CodigoManager { get; set; } = 0;
    public string Cnpj { get; set; }
    public string DenominacaoSocial { get; set; } = "";
    public bool Ativo { get; set; }
    public DateTime DataInclusao { get; set; }
    public int IdUsuarioInclusao { get; set; }
    public DateTime? DataAlteracao { get; set; }
    public int IdUsuarioAtualizacao { get; set; }
    public int IdEmpresaPrincipal { get; set; }
    public int CodigoManagerEmpresaPrincipal { get; set; }

    [ForeignkeyFrom("ClassificacaoArea")]
    public int IdAuxClassificacao { get; set; }
    public ClassificacaoArea ClassificacaoArea { get; set; }
    public DateTime DataInativacao { get; set; }
    public int IdMatriz { get; set; }
    public string Observacao { get; set; }

    [ForeignkeyFrom("EmpresaClassificacaoABC")]
    public int IdAuxComplexidade { get; set; }
    public EmpresaClassificacaoABC EmpresaClassificacaoABC { get; set; }

    [ForeignkeyFrom("EmpresaTipo")]
    public int IdAuxTipo { get; set; }
    public EmpresaTipo EmpresaTipo { get; set; }
    public List<EmpresaEndereco> EmpresaEnderecos { get; set; } = new List<EmpresaEndereco>();
    public List<Processo> Processos { get; set; } = new List<Processo>();
    public List<Encarteiramento> Encarteiramentos { get; set; }
    public EmpresaIntegracao? EmpresaIntegracao { get; set; }

    //public AuxiliarGRID status { get; set; }
    //public AuxiliarGRID satisfacao { get; set; }
    //public AuxiliarGRID tributacao { get; set; }

}
