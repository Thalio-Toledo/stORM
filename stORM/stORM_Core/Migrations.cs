using BonesCore.BonesCoreOrm;
using BonesCoreOrm.Generators;
using stORM.DbRepository;
using stORM.stORM_Core.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace stORM.stORM_Core
{
    public static class Migrations
    {
        public static void StartMigration()
        {
            stORMCore orm = new stORMCore();
            var entities = GetDbRepositories();

            foreach (var entity in entities)
            {
                var gen = new CreateGen();
                var script = gen.Generate(entity);
            }
            
        }
        private static List<Type> GetDbRepositories()
        {
            var types = Assembly.GetEntryAssembly().GetTypes();


            return (from type in types
                    where type.IsClass && !type.IsAbstract // Apenas classes concretas
                    let baseType = type.BaseType
                    where baseType != null && baseType.IsGenericType
                    where baseType.GetGenericTypeDefinition() == typeof(DbRepository<>)
                    select  baseType.GetGenericArguments()[0])
                   .ToList();
        }
    }
}
