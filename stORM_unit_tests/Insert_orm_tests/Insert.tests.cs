using BonesCoreOrm.Generators;
using FluentAssertions;
using stORM.stORM_Core;
using stORM.utils;
using stORM_unit_tests.Entities.Entities_custumer;
using static stORM.Models.GroupByModel;

namespace BonesORMUnitTests.Insert_orm_tests;

public class InsertTests
{
    [Fact]
    public async Task Generate_Insert_Custumer()
    {
        //Arrange  
        var options = new Config();

        var table = new Custumer() { Id = 1, Name = "Custumer", AddressId = 1, Active = true };
        options.SetEntity(typeof(Custumer));

        var insert = new InsertGen(options);
        string query = @""" DECLARE @OUTPUT TABLE(Id bigint)
                            INSERT INTO DatabaseName..Custumer(
                              Name,
                              Active, 
                              AddressId
                            )
                            OUTPUT INSERTED.Id INTO @OUTPUT
                            VALUES('Custumer',1,1)

                            SELECT Id FROM @OUTPUT """;

        //Assert
        var result = insert.Generate(table);

        // Act & Assert
        result.NormalizeQuery().Should().Be(query.NormalizeQuery());
    }

    [Fact]
    public async Task Generate_Insert_Item()
    {
        //Arrange  
        var options = new Config();

        var table = new Item() { OrderId = 1,ProductId = 1, Quantity = 1};
        options.SetEntity(typeof(Item));

        var insert = new InsertGen(options);
        string query = @""" DECLARE @OUTPUT TABLE(Id UNIQUEIDENTIFIER)
                            INSERT INTO DatabaseName..Item(
                              OrderId,ProductId,Quantity
                            )
                            OUTPUT INSERTED.Id INTO @OUTPUT
                            VALUES(
                              1,1,1
                            )

                            SELECT Id FROM @OUTPUT """;

        //Assert
        var result = insert.Generate(table);

        // Act & Assert
        result.NormalizeQuery().Should().Be(query.NormalizeQuery());
    } 
}
