using BonesCoreOrm.Generators;
using BonesORMUnitTests.Entities.Entitys_societario;
using BonesORMUnitTests.Entities.Entitys_societario.Aux_Entitys;
using FluentAssertions;
using stORM.Models;
using stORM.utils;
using stORM_unit_tests.Entities.Entities_custumer;
using static stORM.Models.GroupByModel;

namespace BonesORMUnitTests.Select_orm_testes
{
    public class SelectTests
    {

        [Fact]
        public async Task Given_An_Entity_Should_Return_An_Entity_Query()
        {
            //Arrange  
            var options = new Config();

            var table = new Custumer();
            options.SetEntity(typeof(Custumer));

            var select = new SelectGen(options);
            string query = @""" DECLARE @JSON nvarchar(max) 
                                SET @JSON = (
                                    SELECT
                                      [TB_Cus].Id
                                      ,[TB_Cus].Name
                                      ,[TB_Cus].Active
                                      ,[TB_Cus].Date
                                      ,[TB_Cus].AddressId 
                                    FROM DatabaseName..Custumer (NOLOCK) [TB_Cus] 
                                FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(table);

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }

        [Fact]
        public async Task Given_An_Entity_With_A_SubEntity_One_To_one_Should_Return_An_Entity_with_Join_Query()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(Custumer));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Address).Name,
                MainEntity = typeof(Custumer).Name,
                JoinFull = true,
                Name = typeof(Address).Name
            });

            var select = new SelectGen(options);

            string query = @""" DECLARE @JSON nvarchar(max) 
                                SET @JSON = (
                                    SELECT
                                        [TB_Cus].Id
                                        ,[TB_Cus].Name
                                        ,[TB_Cus].Active
                                        ,[TB_Cus].Date
                                        ,[TB_Cus].AddressId
                                        ,[TB_Add].Id AS [Address.Id]
                                        ,[TB_Add].Street AS [Address.Street]
                                        ,[TB_Add].Number AS [Address.Number]
                                        ,[TB_Add].Active AS [Address.Active]
                                        ,[TB_Add].DateCreation AS [Address.DateCreation]
                                        ,[TB_Add].CustumerId AS [Address.CustumerId] 
                                    FROM DatabaseName..Custumer (NOLOCK) [TB_Cus] 
                                    LEFT JOIN DatabaseName..Address (NOLOCK) [TB_Add] ON [TB_Add].Id = [TB_Cus].AddressId 
                                FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(new Custumer());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }


        [Fact]
        public async Task Given_An_Entity_With_A_SubEntity_One_To_many_Should_Return_An_Entity_with_Count_of_SubEntity()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(ProcessoStatus));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Processo).Name,
                MainEntity = typeof(ProcessoStatus).Name,
                JoinFull = true,
                Name = typeof(Processo).Name
            });

            options.CountList.Add(new CountModel { Entity = typeof(Processo).Name });

            var select = new SelectGen(options);

            string query = @""" DECLARE @JSON nvarchar(max) 
                                SET @JSON = (


                                SELECT
                                  [TB_ProSta].Id
                                  ,[TB_ProSta].label
                                  ,[TB_ProSta].Ordem
                                  ,[TB_ProSta].CssClass
                                  ,[TB_ProSta].IconName
                                  ,[TB_ProSta].IconStyle
                                  ,[TB_ProSta].PermitirAlteracaoManual
                                  ,[TB_ProSta].IdAuxFluxo
                                  ,


                                (SELECT
                                   COUNT(*)  
                                FROM TB_PROCESSO (NOLOCK) [TB_Pro] 
                                WHERE [TB_ProSta].Id = [TB_Pro].IdStatus) AS  Processos 
                                FROM SocietarioDigital..TB_PROCESSO_STATUS (NOLOCK) [TB_ProSta] 
                                 FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(new ProcessoStatus());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }

        [Fact]
        public async Task SHOULD_RETURN_AN_ENTITY_WITH_NESTEDS_SUBENTITIES_JOINS_AND_A_SUBSELECT_QUERY()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(Custumer));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Address).Name,
                MainEntity = typeof(Custumer).Name,
                JoinFull = true,
                Name = typeof(Address).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Order).Name,
                MainEntity = typeof(Custumer).Name,
                JoinFull = true,
                Name = typeof(Order).Name
            });

            var select = new SelectGen(options);

            string query = @""" DECLARE @JSON nvarchar(max) 
                                SET @JSON = (
                                    SELECT
                                      [TB_Cus].Id
                                      ,[TB_Cus].Name
                                      ,[TB_Cus].Active
                                      ,[TB_Cus].Date
                                      ,[TB_Cus].AddressId
                                      ,
                                    (SELECT
                                      [TB_Ord].Id
                                      ,[TB_Ord].CreationDate
                                      ,[TB_Ord].Status
                                      ,[TB_Ord].CustumerId 
                                    FROM DatabaseName..Order (NOLOCK) [TB_Ord] 
                                    WHERE [TB_Cus].Id = [TB_Ord].CustumerId FOR JSON PATH) AS  Orders
                                      ,[TB_Add].Id AS [Address.Id]
                                      ,[TB_Add].Street AS [Address.Street]
                                      ,[TB_Add].Number AS [Address.Number]
                                      ,[TB_Add].Active AS [Address.Active]
                                      ,[TB_Add].DateCreation AS [Address.DateCreation]
                                      ,[TB_Add].CustumerId AS [Address.CustumerId] 
                                    FROM DatabaseName..Custumer (NOLOCK) [TB_Cus] 
                                    LEFT JOIN DatabaseName..Address (NOLOCK) [TB_Add] ON [TB_Add].Id = [TB_Cus].AddressId 
                                FOR JSON PATH) SELECT @JSON AS 'result' """;



            //Assert
            var result = select.Generate(new Custumer());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }

        [Fact]
        public async Task SHOULD_RETURN_QUERY_WITH_MANY_NESTED_ENTITIES()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(ProcessoHistorico));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Processo).Name,
                MainEntity = typeof(ProcessoHistorico).Name,
                JoinFull = true,
                Name = typeof(Processo).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(ProcessoStatus).Name,
                MainEntity = typeof(Processo).Name,
                JoinFull = true,
                Name = typeof(ProcessoStatus).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(ProcessoStatusPendencia).Name,
                MainEntity = typeof(ProcessoStatus).Name,
                JoinFull = true,
                Name = typeof(ProcessoStatusPendencia).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Empresa).Name,
                MainEntity = typeof(Processo).Name,
                JoinFull = true,
                Name = typeof(Empresa).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(EmpresaEndereco).Name,
                MainEntity = typeof(Empresa).Name,
                JoinFull = true,
                Name = typeof(EmpresaEndereco).Name
            });

            var select = new SelectGen(options);

            string query = @""" 
                                DECLARE @JSON nvarchar(max) 
                                SET @JSON = (


                                SELECT
                                  [TB_ProHis].Id
                                  ,[TB_ProHis].IdUsuarioResponsavel
                                  ,[TB_ProHis].Descricao
                                  ,[TB_ProHis].Data
                                  ,[TB_ProHis].IdStatus
                                  ,[TB_ProHis].IdStatusPendencia
                                  ,[TB_ProHis].DataPrevista
                                  ,[TB_ProHis].IdUsuarioModificacao
                                  ,[TB_ProHis].PB
                                  ,[TB_ProHis].NumeroTicket
                                  ,[TB_ProHis].IdProcesso
                                  ,[TB_Pro].Id AS [Processo.Id]
                                  ,[TB_Pro].IdEmpresa AS [Processo.IdEmpresa]
                                  ,[TB_Pro].IdProcessoTipo AS [Processo.IdProcessoTipo]
                                  ,[TB_Pro].IdUsuarioResponsavel AS [Processo.IdUsuarioResponsavel]
                                  ,[TB_Pro].IdStatus AS [Processo.IdStatus]
                                  ,[TB_Pro].IdStatusPendencia AS [Processo.IdStatusPendencia]
                                  ,[TB_Pro].DataCriacao AS [Processo.DataCriacao]
                                  ,[TB_Pro].DataModificacao AS [Processo.DataModificacao]
                                  ,[TB_Pro].IdUsuarioModificacao AS [Processo.IdUsuarioModificacao]
                                  ,[TB_Pro].DocumentoUrlDownload AS [Processo.DocumentoUrlDownload]
                                  ,[TB_Pro].Ativo AS [Processo.Ativo]
                                  ,[TB_Pro].DataPrevista AS [Processo.DataPrevista]
                                  ,[TB_Pro].DataInicio AS [Processo.DataInicio]
                                  ,[TB_Pro].DataFim AS [Processo.DataFim]
                                  ,[TB_Pro].NumeroProcesso AS [Processo.NumeroProcesso]
                                  ,[TB_Pro].IdAuxTipoMovimentacaoCertidao AS [Processo.IdAuxTipoMovimentacaoCertidao]
                                  ,[TB_Pro].PB AS [Processo.PB]
                                  ,[TB_Pro].IdAuxFluxo AS [Processo.IdAuxFluxo]
                                  ,[TB_Pro].numeroTicket AS [Processo.numeroTicket]
                                  ,[TB_Emp].Id AS [Processo.Empresa.Id]
                                  ,[TB_Emp].Guid AS [Processo.Empresa.Guid]
                                  ,[TB_Emp].CodigoManager AS [Processo.Empresa.CodigoManager]
                                  ,[TB_Emp].Cnpj AS [Processo.Empresa.Cnpj]
                                  ,[TB_Emp].DenominacaoSocial AS [Processo.Empresa.DenominacaoSocial]
                                  ,[TB_Emp].Ativo AS [Processo.Empresa.Ativo]
                                  ,[TB_Emp].DataInclusao AS [Processo.Empresa.DataInclusao]
                                  ,[TB_Emp].IdUsuarioInclusao AS [Processo.Empresa.IdUsuarioInclusao]
                                  ,[TB_Emp].DataAlteracao AS [Processo.Empresa.DataAlteracao]
                                  ,[TB_Emp].IdUsuarioAtualizacao AS [Processo.Empresa.IdUsuarioAtualizacao]
                                  ,[TB_Emp].IdEmpresaPrincipal AS [Processo.Empresa.IdEmpresaPrincipal]
                                  ,[TB_Emp].CodigoManagerEmpresaPrincipal AS [Processo.Empresa.CodigoManagerEmpresaPrincipal]
                                  ,[TB_Emp].IdAuxClassificacao AS [Processo.Empresa.IdAuxClassificacao]
                                  ,[TB_Emp].DataInativacao AS [Processo.Empresa.DataInativacao]
                                  ,[TB_Emp].IdMatriz AS [Processo.Empresa.IdMatriz]
                                  ,[TB_Emp].Observacao AS [Processo.Empresa.Observacao]
                                  ,[TB_Emp].IdAuxComplexidade AS [Processo.Empresa.IdAuxComplexidade]
                                  ,[TB_Emp].IdAuxTipo AS [Processo.Empresa.IdAuxTipo]
                                  ,

                                    (SELECT
                                      [TB_EmpEnd].Id
                                      ,[TB_EmpEnd].IdEmpresa
                                      ,[TB_EmpEnd].Rua
                                      ,[TB_EmpEnd].Numero
                                      ,[TB_EmpEnd].complemento
                                      ,[TB_EmpEnd].Bairro
                                      ,[TB_EmpEnd].CEP
                                      ,[TB_EmpEnd].IdMunicipio
                                      ,[TB_EmpEnd].ativo
                                      ,[TB_EmpEnd].idAuxEmpresaEnderecoTipo 
                                    FROM ControlePermissao..TB_EMPRESA_ENDERECO (NOLOCK) [TB_EmpEnd] 
                                    WHERE [TB_Emp].Id = [TB_EmpEnd].IdEmpresa FOR JSON PATH) [Processo.Empresa.EmpresaEnderecos]

                                  ,[TB_ProSta].Id AS [Processo.ProcessoStatus.Id]
                                  ,[TB_ProSta].label AS [Processo.ProcessoStatus.label]
                                  ,[TB_ProSta].Ordem AS [Processo.ProcessoStatus.Ordem]
                                  ,[TB_ProSta].CssClass AS [Processo.ProcessoStatus.CssClass]
                                  ,[TB_ProSta].IconName AS [Processo.ProcessoStatus.IconName]
                                  ,[TB_ProSta].IconStyle AS [Processo.ProcessoStatus.IconStyle]
                                  ,[TB_ProSta].PermitirAlteracaoManual AS [Processo.ProcessoStatus.PermitirAlteracaoManual]
                                  ,[TB_ProSta].IdAuxFluxo AS [Processo.ProcessoStatus.IdAuxFluxo]
                                  ,

                                (SELECT
                                  [TB_ProStaPen].Id
                                  ,[TB_ProStaPen].descricao
                                  ,[TB_ProStaPen].IdStatus
                                  ,[TB_ProStaPen].IdAuxFluxo 
                                FROM SocietarioDigital..TB_PROCESSO_STATUS_PENDENCIA (NOLOCK) [TB_ProStaPen] 
                                WHERE [TB_ProSta].Id = [TB_ProStaPen].IdStatus FOR JSON PATH) [Processo.ProcessoStatus.ProcessoStatusPendencias] 

                                FROM SocietarioDigital..TB_PROCESSO_HISTORICO (NOLOCK) [TB_ProHis] 
                                LEFT JOIN TB_PROCESSO (NOLOCK) [TB_Pro] ON [TB_Pro].Id = [TB_ProHis].IdProcesso 
                                LEFT JOIN ControlePermissao..TB_EMPRESA (NOLOCK) [TB_Emp] ON [TB_Emp].Id = [TB_Pro].IdEmpresa 
                                LEFT JOIN SocietarioDigital..TB_PROCESSO_STATUS (NOLOCK) [TB_ProSta] ON [TB_ProSta].Id = [TB_Pro].IdStatus 

                                 FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(new Custumer());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }

        [Fact]
        public async Task SHOULD_RETURN_QUERY_ONE_TO_MANY_TO_MANY_()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(Empresa));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Processo).Name,
                MainEntity = typeof(Empresa).Name,
                JoinFull = true,
                Name = typeof(Processo).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(ProcessoHistorico).Name,
                MainEntity = typeof(Processo).Name,
                JoinFull = true,
                Name = typeof(ProcessoHistorico).Name
            });

            var select = new SelectGen(options);

            string query = @""" 
                        DECLARE @JSON nvarchar(max) 
                        SET @JSON = (


                        SELECT
                          [TB_Emp].Id
                          ,[TB_Emp].Guid
                          ,[TB_Emp].CodigoManager
                          ,[TB_Emp].Cnpj
                          ,[TB_Emp].DenominacaoSocial
                          ,[TB_Emp].Ativo
                          ,[TB_Emp].DataInclusao
                          ,[TB_Emp].IdUsuarioInclusao
                          ,[TB_Emp].DataAlteracao
                          ,[TB_Emp].IdUsuarioAtualizacao
                          ,[TB_Emp].IdEmpresaPrincipal
                          ,[TB_Emp].CodigoManagerEmpresaPrincipal
                          ,[TB_Emp].IdAuxClassificacao
                          ,[TB_Emp].DataInativacao
                          ,[TB_Emp].IdMatriz
                          ,[TB_Emp].Observacao
                          ,[TB_Emp].IdAuxComplexidade
                          ,[TB_Emp].IdAuxTipo
                          ,


                        (SELECT
                          [TB_Pro].Id
                          ,[TB_Pro].IdEmpresa
                          ,[TB_Pro].IdProcessoTipo
                          ,[TB_Pro].IdUsuarioResponsavel
                          ,[TB_Pro].IdStatus
                          ,[TB_Pro].IdStatusPendencia
                          ,[TB_Pro].DataCriacao
                          ,[TB_Pro].DataModificacao
                          ,[TB_Pro].IdUsuarioModificacao
                          ,[TB_Pro].DocumentoUrlDownload
                          ,[TB_Pro].Ativo
                          ,[TB_Pro].DataPrevista
                          ,[TB_Pro].DataInicio
                          ,[TB_Pro].DataFim
                          ,[TB_Pro].NumeroProcesso
                          ,[TB_Pro].IdAuxTipoMovimentacaoCertidao
                          ,[TB_Pro].PB
                          ,[TB_Pro].IdAuxFluxo
                          ,[TB_Pro].numeroTicket
                          ,

                        (SELECT
                          [TB_ProHis].Id
                          ,[TB_ProHis].IdUsuarioResponsavel
                          ,[TB_ProHis].Descricao
                          ,[TB_ProHis].Data
                          ,[TB_ProHis].IdStatus
                          ,[TB_ProHis].IdStatusPendencia
                          ,[TB_ProHis].DataPrevista
                          ,[TB_ProHis].IdUsuarioModificacao
                          ,[TB_ProHis].PB
                          ,[TB_ProHis].NumeroTicket
                          ,[TB_ProHis].IdProcesso 
                        FROM SocietarioDigital..TB_PROCESSO_HISTORICO (NOLOCK) [TB_ProHis] 
                        WHERE [TB_Pro].Id = [TB_ProHis].IdProcesso FOR JSON PATH) [ProcessoHistoricos] 

                        FROM TB_PROCESSO (NOLOCK) [TB_Pro] 
                        WHERE [TB_Emp].Id = [TB_Pro].IdEmpresa FOR JSON PATH) AS  Processos 

                        FROM ControlePermissao..TB_EMPRESA (NOLOCK) [TB_Emp] 
                         FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(new Custumer());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }

        [Fact]
        public async Task SHOULD_RETURN_QUERY_ProcessoHistorico_Processo_Empresa_Enderecos()
        {
            //Arrange  
            var options = new Config();

            options.SetEntity(typeof(ProcessoHistorico));

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Processo).Name,
                MainEntity = typeof(ProcessoHistorico).Name,
                JoinFull = true,
                Name = typeof(Processo).Name
            });
            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(UsuarioBackoffice).Name,
                MainEntity = typeof(ProcessoHistorico).Name,
                JoinFull = true,
                Name = "UsuarioResponsavel"
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(UsuarioBackoffice).Name,
                MainEntity = typeof(ProcessoHistorico).Name,
                JoinFull = true,
                Name = "UsuarioModificacao"
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(Empresa).Name,
                MainEntity = typeof(Processo).Name,
                JoinFull = true,
                Name = typeof(Empresa).Name
            });
            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(EmpresaEndereco).Name,
                MainEntity = typeof(Empresa).Name,
                JoinFull = true,
                Name = typeof(EmpresaEndereco).Name
            });
            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(EmpresaTipo).Name,
                MainEntity = typeof(Empresa).Name,
                JoinFull = true,
                Name = typeof(EmpresaTipo).Name
            });

            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(ProcessoStatus).Name,
                MainEntity = typeof(Processo).Name,
                JoinFull = true,
                Name = typeof(ProcessoStatus).Name
            });
            options.JoinsList.Add(new JoinEntity
            {
                Entity = typeof(ProcessoStatusPendencia).Name,
                MainEntity = typeof(ProcessoStatus).Name,
                JoinFull = true,
                Name = typeof(ProcessoStatusPendencia).Name
            });

            var select = new SelectGen(options);

            string query = @""" 
                            DECLARE @JSON nvarchar(max) 
                            SET @JSON = (

                            SELECT
                              [TB_ProHis].Id
                              ,[TB_ProHis].IdUsuarioResponsavel
                              ,[TB_ProHis].Descricao
                              ,[TB_ProHis].Data
                              ,[TB_ProHis].IdStatus
                              ,[TB_ProHis].IdStatusPendencia
                              ,[TB_ProHis].DataPrevista
                              ,[TB_ProHis].IdUsuarioModificacao
                              ,[TB_ProHis].PB
                              ,[TB_ProHis].NumeroTicket
                              ,[TB_ProHis].IdProcesso
                              ,[TB_UsuRes].Id AS [UsuarioResponsavel.Id]
                              ,[TB_UsuRes].Nome AS [UsuarioResponsavel.Nome]
                              ,[TB_UsuRes].Email AS [UsuarioResponsavel.Email]
                              ,[TB_UsuRes].Cpf AS [UsuarioResponsavel.Cpf]
                              ,[TB_UsuRes].Ativo AS [UsuarioResponsavel.Ativo]
                              ,[TB_UsuRes].IdAreaPrincipal AS [UsuarioResponsavel.IdAreaPrincipal]
                              ,[TB_UsuMod].Id AS [UsuarioModificacao.Id]
                              ,[TB_UsuMod].Nome AS [UsuarioModificacao.Nome]
                              ,[TB_UsuMod].Email AS [UsuarioModificacao.Email]
                              ,[TB_UsuMod].Cpf AS [UsuarioModificacao.Cpf]
                              ,[TB_UsuMod].Ativo AS [UsuarioModificacao.Ativo]
                              ,[TB_UsuMod].IdAreaPrincipal AS [UsuarioModificacao.IdAreaPrincipal]
                              ,[TB_Pro].Id AS [Processo.Id]
                              ,[TB_Pro].IdEmpresa AS [Processo.IdEmpresa]
                              ,[TB_Pro].IdProcessoTipo AS [Processo.IdProcessoTipo]
                              ,[TB_Pro].IdUsuarioResponsavel AS [Processo.IdUsuarioResponsavel]
                              ,[TB_Pro].IdStatus AS [Processo.IdStatus]
                              ,[TB_Pro].IdStatusPendencia AS [Processo.IdStatusPendencia]
                              ,[TB_Pro].DataCriacao AS [Processo.DataCriacao]
                              ,[TB_Pro].DataModificacao AS [Processo.DataModificacao]
                              ,[TB_Pro].IdUsuarioModificacao AS [Processo.IdUsuarioModificacao]
                              ,[TB_Pro].DocumentoUrlDownload AS [Processo.DocumentoUrlDownload]
                              ,[TB_Pro].Ativo AS [Processo.Ativo]
                              ,[TB_Pro].DataPrevista AS [Processo.DataPrevista]
                              ,[TB_Pro].DataInicio AS [Processo.DataInicio]
                              ,[TB_Pro].DataFim AS [Processo.DataFim]
                              ,[TB_Pro].NumeroProcesso AS [Processo.NumeroProcesso]
                              ,[TB_Pro].IdAuxTipoMovimentacaoCertidao AS [Processo.IdAuxTipoMovimentacaoCertidao]
                              ,[TB_Pro].PB AS [Processo.PB]
                              ,[TB_Pro].IdAuxFluxo AS [Processo.IdAuxFluxo]
                              ,[TB_Pro].numeroTicket AS [Processo.numeroTicket]
                              ,[TB_Emp].Id AS [Processo.Empresa.Id]
                              ,[TB_Emp].Guid AS [Processo.Empresa.Guid]
                              ,[TB_Emp].CodigoManager AS [Processo.Empresa.CodigoManager]
                              ,[TB_Emp].Cnpj AS [Processo.Empresa.Cnpj]
                              ,[TB_Emp].DenominacaoSocial AS [Processo.Empresa.DenominacaoSocial]
                              ,[TB_Emp].Ativo AS [Processo.Empresa.Ativo]
                              ,[TB_Emp].DataInclusao AS [Processo.Empresa.DataInclusao]
                              ,[TB_Emp].IdUsuarioInclusao AS [Processo.Empresa.IdUsuarioInclusao]
                              ,[TB_Emp].DataAlteracao AS [Processo.Empresa.DataAlteracao]
                              ,[TB_Emp].IdUsuarioAtualizacao AS [Processo.Empresa.IdUsuarioAtualizacao]
                              ,[TB_Emp].IdEmpresaPrincipal AS [Processo.Empresa.IdEmpresaPrincipal]
                              ,[TB_Emp].CodigoManagerEmpresaPrincipal AS [Processo.Empresa.CodigoManagerEmpresaPrincipal]
                              ,[TB_Emp].IdAuxClassificacao AS [Processo.Empresa.IdAuxClassificacao]
                              ,[TB_Emp].DataInativacao AS [Processo.Empresa.DataInativacao]
                              ,[TB_Emp].IdMatriz AS [Processo.Empresa.IdMatriz]
                              ,[TB_Emp].Observacao AS [Processo.Empresa.Observacao]
                              ,[TB_Emp].IdAuxComplexidade AS [Processo.Empresa.IdAuxComplexidade]
                              ,[TB_Emp].IdAuxTipo AS [Processo.Empresa.IdAuxTipo]
                              ,


                            (SELECT
                              [TB_EmpEnd].Id
                              ,[TB_EmpEnd].IdEmpresa
                              ,[TB_EmpEnd].Rua
                              ,[TB_EmpEnd].Numero
                              ,[TB_EmpEnd].complemento
                              ,[TB_EmpEnd].Bairro
                              ,[TB_EmpEnd].CEP
                              ,[TB_EmpEnd].IdMunicipio
                              ,[TB_EmpEnd].ativo
                              ,[TB_EmpEnd].idAuxEmpresaEnderecoTipo 
                            FROM ControlePermissao..TB_EMPRESA_ENDERECO (NOLOCK) [TB_EmpEnd] 
                            WHERE [TB_Emp].Id = [TB_EmpEnd].IdEmpresa FOR JSON PATH) [Processo.Empresa.EmpresaEnderecos]
                              ,[TB_EmpTip].Configuracao AS [Processo.Empresa.EmpresaTipo.Configuracao]
                              ,[TB_EmpTip].Chave AS [Processo.Empresa.EmpresaTipo.Chave]
                              ,[TB_EmpTip].Valor AS [Processo.Empresa.EmpresaTipo.Valor]
                              ,[TB_EmpTip].Label AS [Processo.Empresa.EmpresaTipo.Label]
                              ,[TB_EmpTip].IconName AS [Processo.Empresa.EmpresaTipo.IconName]
                              ,[TB_ProSta].Id AS [Processo.ProcessoStatus.Id]
                              ,[TB_ProSta].label AS [Processo.ProcessoStatus.label]
                              ,[TB_ProSta].Ordem AS [Processo.ProcessoStatus.Ordem]
                              ,[TB_ProSta].CssClass AS [Processo.ProcessoStatus.CssClass]
                              ,[TB_ProSta].IconName AS [Processo.ProcessoStatus.IconName]
                              ,[TB_ProSta].IconStyle AS [Processo.ProcessoStatus.IconStyle]
                              ,[TB_ProSta].PermitirAlteracaoManual AS [Processo.ProcessoStatus.PermitirAlteracaoManual]
                              ,[TB_ProSta].IdAuxFluxo AS [Processo.ProcessoStatus.IdAuxFluxo]
                              ,


                            (SELECT
                              [TB_ProStaPen].Id
                              ,[TB_ProStaPen].descricao
                              ,[TB_ProStaPen].IdStatus
                              ,[TB_ProStaPen].IdAuxFluxo 
                            FROM SocietarioDigital..TB_PROCESSO_STATUS_PENDENCIA (NOLOCK) [TB_ProStaPen] 
                            WHERE [TB_ProSta].Id = [TB_ProStaPen].IdStatus FOR JSON PATH) [Processo.ProcessoStatus.ProcessoStatusPendencias] 
                            FROM SocietarioDigital..TB_PROCESSO_HISTORICO (NOLOCK) [TB_ProHis] 
                            LEFT JOIN ControlePermissao..TB_USUARIO_BACKOFFICE (NOLOCK) [TB_UsuRes] ON [TB_UsuRes].Id = [TB_ProHis].IdUsuarioResponsavel 
                            LEFT JOIN ControlePermissao..TB_USUARIO_BACKOFFICE (NOLOCK) [TB_UsuMod] ON [TB_UsuMod].Id = [TB_ProHis].IdUsuarioModificacao 
                            LEFT JOIN TB_PROCESSO (NOLOCK) [TB_Pro] ON [TB_Pro].Id = [TB_ProHis].IdProcesso 
                            LEFT JOIN ControlePermissao..TB_EMPRESA (NOLOCK) [TB_Emp] ON [TB_Emp].Id = [TB_Pro].IdEmpresa 
                            LEFT JOIN ControlePermissao..TB_ENUM_AUXILIAR (NOLOCK) [TB_EmpTip] ON [TB_EmpTip].Chave = [TB_Emp].IdAuxTipo 
                            LEFT JOIN SocietarioDigital..TB_PROCESSO_STATUS (NOLOCK) [TB_ProSta] ON [TB_ProSta].Id = [TB_Pro].IdStatus 
                             FOR JSON PATH) SELECT @JSON AS 'result' """;

            //Assert
            var result = select.Generate(new Custumer());

            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }
    }
}
