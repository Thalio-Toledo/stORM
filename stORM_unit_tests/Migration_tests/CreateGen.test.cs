using BonesCoreOrm.Generators;
using FluentAssertions;
using stORM.stORM_Core.Generators;
using stORM.utils;
using stORM_unit_tests.Entities.Entities_custumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Dapper.SqlMapper;
using static stORM.Models.GroupByModel;

namespace stORM_unit_tests.Migration_tests
{
    public class CreateGenTests
    {
        [Fact]
        public async Task Generate_Create()
        {
            //Arrange  

            var Create = new CreateGen();

            string query = @""" CREATE TABLE Custumer (
                                Id INT IDENTITY(1,1) PRIMARY KEY,
                                Name NVARCHAR(MAX) NOT NULL,
                                Active BIT NOT NULL DEFAULT 1,
                                Date DATETIME NOT NULL,
                                AddressId INT NOT NULL,) """;

            //Assert
            var result = Create.Generate(typeof(Custumer));


            // Act & Assert
            result.NormalizeQuery().Should().Be(query.NormalizeQuery());
        }
    }
}
