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

### Another Properties
``` charp
[Table("custumer")] 
public class Custumer{
	[Key]
	public int CustumerId {get;set;}
	public string CustumerName { get; set; }
	public bool Active { get; set; }
	public DateTime CreatedDate { get; set; }
}

```

### Navigation Properties

```csharp

// One to One
[Table("custumer")] 
public class Custumer{
	[Key]
	public int CustumerId {get;set;}
	public string CustumerName { get; set; }
	public bool Active { get; set; }
	public DateTime CreatedDate { get; set; }

	//Decorate your ForeignKey with the annotation ForeignkeyOf and the entity name
	[ForeignkeyOf("Adress")]
	public int AddressID {get;set;}
	public Adress? Adress {get;set;}
	
}

// One to Many
[Table("custumer")] 
public class Custumer{
	[Key]
	public int CustumerId {get;set;}
	public string CustumerName { get; set; }
	public bool Active { get; set; }
	public DateTime CreatedDate { get; set; }
	
	[ForeignkeyOf("Adress")]
	public int AddressID {get;set;}
	public Adress? Adress {get;set;}

	//Create a List of you entity 
	public List<Order>? Orders{ get; set; }
}
 

```

### Repository
stORM abastract the necessity to implement the repository pattern all you need to do is inherit from our repository.
``` csharp
// Create your repository using your entity
public class CustumerRepository : DbRepository<Custumer>
{
    public CustumerRepository(stORMCore orm) : base(orm)
    {
    }
}
```



