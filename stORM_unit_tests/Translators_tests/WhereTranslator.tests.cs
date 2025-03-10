using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using BonesCore.BonesCoreOrm.ExpressionsTranslators;
using BonesCoreOrm.Generators;
using BonesORMUnitTests.Entities.Entitys_societario;
using FluentAssertions;
using stORM.utils;
using stORM_unit_tests.Entities.Entities_custumer;
using static stORM.Models.GroupByModel;

namespace BonesORMUnitTests.Translators_tests;

public class WhereTranslatorTests
{
    [Fact]
    public async Task Translate_Where_one_prop()
    {
        //Arrange
        Expression<Func<ProcessoHistorico, bool>> expr = (ph => ph.IdProcesso == 1);

        var translatorJoin = new WhereTranslator(typeof(Custumer));

        //Assert
        var whereInfo = translatorJoin.TranslateWhereExpression(expr.Body);

        var whereScript = "([TB_ProHis].IdProcesso = 1)";

        // Act & Assert
        whereInfo.WhereScript.NormalizeQuery().Should().Be(whereScript.NormalizeQuery());
    }

    [Fact]
    public async Task Translate_Where_and_operator()
    {
        //Arrange
        Expression<Func<ProcessoHistorico, bool>> expr = (ph => ph.IdProcesso == 1 && ph.IdStatus == 1);
        var translatorJoin = new WhereTranslator(typeof(Custumer));
        var whereScript = "(([TB_ProHis].IdProcesso = 1 ) AND ([TB_ProHis].IdStatus = 1 ))";

        //Assert
        var whereInfo = translatorJoin.TranslateWhereExpression(expr.Body);
        
        // Act & Assert
        whereInfo.WhereScript.NormalizeQuery().Should().Be(whereScript.NormalizeQuery());
    }

    [Fact]
    public async Task Translate_Where_OR_operator()
    {
        //Arrange
        Expression<Func<ProcessoHistorico, bool>> expr = (ph => ph.IdProcesso == 1 || ph.Processo.Id == 1);

        var translatorJoin = new WhereTranslator(typeof(Custumer));
        var whereScript = "(([TB_ProHis].IdProcesso = 1 ) OR ([TB_Pro].Id = 1 ))";

        //Assert
        var whereInfo = translatorJoin.TranslateWhereExpression(expr.Body);

        // Act & Assert
        whereInfo.WhereScript.NormalizeQuery().Should().Be(whereScript.NormalizeQuery());
    }
}
