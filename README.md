# stORM 
stORM is a .NET Library highly inspired on Entity framework, used to mapper pre-existing databases, Soon the feature code first will be implemented.
Storm allows you to create queries fast and simple. Using the concepts from functional programing you can chaining methods to create complex queries and map you entities.

## Configuring connection string

At appsettings.json file configure the connection string 
``` json
"ConnectionStrings": {
    "DefaultConnection": "Password=YOUR_PASSWORD;Persist Security Info=True;User Id=YOUR_USER;Initial Catalog=YOUR_DATABASE;Data Source=YOUR_SERVER;TrustServerCertificate=true;MultipleActiveResultSets=True",
},
```

## Extension methods
Simplify using an extension method to configure you conection.

``` csharp
 public static class DbConnectionExtension
 {
     public static IServiceCollection AddDataBaseConnection(this IServiceCollection services, IConfiguration configuration)
     {
         var configOptions = configuration.GetSection("ConnectionStrings");
         services.Configure<DBConnectionOptions>(configOptions);
         services.AddstORM(configuration);
         return services;
     }
 }

