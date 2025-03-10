using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BonesCore.BonesCoreOrm;
using BonesCoreOrm.Generators;
using FluentAssertions;
using stORM.utils;
using stORM_unit_tests.Entities.Entities_custumer;
using static stORM.Models.GroupByModel;

namespace BonesORMUnitTests.Update_orm_tests;

public  class UpdateTests
{
    [Fact]
    public async Task Generate_Update()
    {
        //Arrange  
        var options = new Config();

        var table = new Custumer() { Id = 1, Name = "Custumer", AddressId = 1, Active = true};
        options.SetEntity(typeof(Custumer));

        var update = new UpdateGen(options);
        string query = @""" UPDATE DatabaseName..Custumer SET 
                               Name = 'Custumer',
                               Active = 1,
                               AddressId = 1
                            WHERE   
                             Id = 1 """;

        //Assert
        var result = update.Generate(table);

        // Act & Assert
        result.NormalizeQuery().Should().Be(query.NormalizeQuery());
    }

    [Fact]
    public async Task Should_return_exception_Entity_without_primaryKey()
    {
        //Arrange  
        var options = new Config();

        var table = new Product() { Id = 1, Name = "Product", Price = 10 };
        options.SetEntity(typeof(Product));

        var update = new UpdateGen(options);

        //Assert
        Action act = () => update.Generate(table);

        // Act & Assert
        act.Invoking(a => a()).Should().Throw<Exception>().WithMessage($"Primary Key from {typeof(Product).Name} was not found!"); ;
    }

    [Fact]
    public async Task Should_return_exception_Entity_without_primaryKeyValue()
    {
        //Arrange  
        var options = new Config();

        var table = new Address() { Street = "Street", Number = 10 };
        options.SetEntity(typeof(Address));

        var update = new UpdateGen(options);

        //Assert
        Action act = () => update.Generate(table);

        // Act & Assert
        act.Invoking(a => a()).Should().Throw<Exception>().WithMessage($"Primary Key value from {typeof(Address).Name} was not found!"); ;
    }
}
