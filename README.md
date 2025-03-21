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

## Add stORM 
Using an extension method addstORM to configure stORM in you application.

At program.cs file 
``` csharp
 builder.Services.AddstORM(builder.Configuration);
```

## Entities Mapping
Create you entity that represent you table in you database

### Table Annotation

``` csharp
//Decorate your entity with the table name on database
[Table("custumer")] 
public class Custumer{

}
```

## Properties
### PrimaryKey

``` csharp
[Table("custumer")] 
public class Custumer{
    //Decorate your primary key with the annotation key
	[Key]
	public int CustumerId {get;set;} 
}


