# EntityFrameworkCore Raw Query Builder

**Query Builder using String Interpolation and Fluent API**

This library is a wrapper around Entity Framework Core mostly for helping building **Raw dynamic SQL queries and commands**, but more flexible than `.FromSqlInterpolated`.  


# Quickstart / NuGet Package

1. Install the [NuGet package EntityFrameworkCore.RawQueryBuilder](https://www.nuget.org/packages/EntityFrameworkCore.RawQueryBuilder)
1. Start using like this:
```cs
using EntityFrameworkCore.RawQueryBuilder;
// ...


var context = new AdventureWorksDbContext(); // your DbContext

// Your "template" query
// Similar to FromSqlInterpolated, but later you can modify 
// the query and add more conditions
var q = context.Products.RawQueryBuilder(
	$@"SELECT ProductId, Name, ListPrice, Weight
	FROM [Production].[Product]
	/**where**/
	ORDER BY ProductId
	");

// Dynamically append conditions
q.Where($"[ListPrice] <= {maxPrice}");
q.Where($"[Weight] <= {maxWeight}");
q.Where($"[Name] LIKE {search}");

// After AsQueryable() you can use any EF extension
// like ToList, ToListAsync, First, Count, Max, etc.. it's pure EF !
var products = q.AsQueryable().ToList();
```


# Full Documentation and Features

Pending...

## Manual command building

```cs
// start your basic query
var q = context.Products.RawQueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight FROM Product WHERE 1=1");

// Dynamically append whatever statements you need
// and RawQueryBuilder will automatically convert interpolated parameters to parameters (injection-safe)
q.Append($"AND ListPrice <= {maxPrice}");
q.Append($"AND Weight <= {maxWeight}");
q.Append($"AND Name LIKE {search}");
q.Append($"ORDER BY ProductId");

// AsQueryable() will automatically pass your query and injection-safe parameters to EF
var products = q.AsQueryable().ToList();
```

So, basically you dynamically build your query, pass parameters as interpolated strings, and they are converted to safe SqlParameters.

This is our mojo :-) 


## \*\*where\*\* filters 

The **\*\*where\*\*** is a special keyword which acts as a placeholder to render dynamically-defined filters:

- You can append filters to RawQueryBuilder object using .Where() method, and those filters are saved internally.
- When you send your query to EF, RawQueryBuilder will search for a `/**where**/` statement in your query and will replace with the filters you defined.

To sum, RawQueryBuilder keeps track of filters in this special structure and during query execution the \*\*where\*\* keyword is replaced with those filters.

```cs
int maxPrice = 1000;
int maxWeight = 15;
string search = "%Mountain%";

// You can build the query manually and just use RawQueryBuilder to replace "where" filters (if any)
var q = context.Products.RawQueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");
    
// You just pass the parameters as if it was an interpolated string, 
// and RawQueryBuilder will automatically convert them to EF parameters (injection-safe)
q.Where($"ListPrice <= {maxPrice}");
q.Where($"Weight <= {maxWeight}");
q.Where($"Name LIKE {search}");

// Query() will automatically build your query and replace your /**where**/ (if any filter was added)
var products = q.AsQueryable().ToList();
```

EF would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM Product
WHERE ListPrice <= {0} AND Weight <= {1} AND Name LIKE {2}
ORDER BY ProductId
```
If you don't need the `WHERE` keyword (if you already have other fixed conditions before), you can use `/**filters**/` instead:
```cs
var q = context.Products.RawQueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    WHERE Price>{minPrice} /**filters**/
    ORDER BY ProductId
    ");
```

## Combining AND/OR Filters

QueryBuilder contains an internal property called "Filters" which just keeps track of all conditions you've added using `.Where()` method.  
By default those conditions are combined using the `AND` operator.  

If you want to write more complex filters (combining multiple AND/OR filters) we have a typed structure for that, like other query builders do.
But differently from other builders, we don't try to reinvent SQL syntax or create a limited abstraction over SQL language, which is powerful, comprehensive, and vendor-specific, so you should still write your raw filters as if they were regular strings, and we do the rest (structuring AND/OR filters, and extracting parameters from interpolated strings):

```cs
var q = context.Products.RawQueryBuilder(@"SELECT ProductId, Name, ListPrice, Weight
    FROM Product
    /**where**/
    ORDER BY ProductId
    ");

q.Where(new Filters()
{
    new Filter($"ListPrice >= {minPrice}"),
    new Filter($"ListPrice <= {maxPrice}")
});
q.Where(new Filters(Filters.FiltersType.OR)
{
    new Filter($"Weight <= {maxWeight}"),
    new Filter($"Name LIKE {search}")
});

var products = q.AsQueryable().ToList();

// AsQueryable() will automatically build your SQL query, 
// and will replace your /**where**/ (if any filter was added)
// "WHERE (ListPrice >= {0} AND ListPrice <= {1}) AND (Weight <= {2} OR Name LIKE {3})"
// it will also pass the parameters objects to EF - it's all injection safe! 
```


## raw strings

If you want to embed raw strings in your queries (don't want them to be parametrized), you can use the **raw modifier**:

```cs
string tempTableName = "##Products" + Guid.NewGuid().ToString().Substring(0, 8);
string name = "%Tesla%";
context.Products.RawQueryBuilder($@"SELECT * FROM {tempTableName:raw} WHERE Name LIKE {name});
").Execute();
```

One good reason to use the **raw** modifier is when using **nameof expression**, which allows us to "find references" for a column, "rename", etc:

```cs
var q = context.Products.RawQueryBuilder($@"
    SELECT
        c.{nameof(Category.Name):raw} as Category, 
        sc.{nameof(Subcategory.Name):raw} as Subcategory, 
        p.{nameof(Product.Name):raw}, p.ProductNumber"
    FROM Product p
    INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID
    INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID");
```

## Explicitly passing SqlParameters / DbParameters

(Pending)


## Fluent API (Chained-methods)

For those who like method-chaining guidance (or for those who allow end-users to build their own queries), there's a Fluent API which allows you to build queries step-by-step mimicking dynamic SQL concatenation.  

So, basically, instead of starting with a full query and just appending new filters (`.Where()`), the RawQueryBuilder will build the whole query for you:

```cs
var q = context.Products.FluentQueryBuilder()
    .Select($"ProductId")
    .Select($"Name")
    .Select($"ListPrice")
    .Select($"Weight")
    .From($"Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy($"ProductId");
    
var products = q.AsQueryable().ToList();
```

You would get this query:

```sql
SELECT ProductId, Name, ListPrice, Weight
FROM Product
WHERE ListPrice <= {0} AND Weight <= {1} AND Name LIKE {2}
ORDER BY ProductId
```
Or more elaborated:

```cs
var q = context.Products.RawQueryBuilder()
    .SelectDistinct($"ProductId, Name, ListPrice, Weight")
    .From("Product")
    .Where($"ListPrice <= {maxPrice}")
    .Where($"Weight <= {maxWeight}")
    .Where($"Name LIKE {search}")
    .OrderBy("ProductId");
```

Building joins dynamically using Fluent API:

```cs
var categories = new string[] { "Components", "Clothing", "Acessories" };

var q = context.Products.RawQueryBuilder()
    .SelectDistinct($"c.Name as Category, sc.Name as Subcategory, p.Name, p.ProductNumber")
    .From($"Product p")
    .From($"INNER JOIN ProductSubcategory sc ON p.ProductSubcategoryID=sc.ProductSubcategoryID")
    .From($"INNER JOIN ProductCategory c ON sc.ProductCategoryID=c.ProductCategoryID")
    .Where($"c.Name IN {categories}");
```

There are also chained-methods for adding GROUP BY, HAVING, ORDER BY, and paging (OFFSET x ROWS / FETCH NEXT x ROWS ONLY).



## Invoking Stored Procedures

Pending

# Database Support

RawQueryBuilder is database agnostic (like EF) - it should work with any EF database providers (including Microsoft SQL Server, PostgreSQL, MySQL, SQLite, Firebird, SQL CE and Oracle), since it's basically a wrapper around the way query is built and parameters are passed to the database provider.  

RawQueryBuilder doesn't generate SQL statements (except for simple clauses which should work in all databases like `WHERE`/`AND`/`OR` - if you're using `/**where**/` keyword).  


# How to build a raw dynamic query without this library?

**Building raw dynamic filters in EF was a little cumbersome / ugly:**

```cs
// https://www.pmichaels.net/2020/10/10/executing-dynamically-generated-sql-in-ef-core/
string sql =
    "select * " +
    "from MyTable ";
 
var parameters = new List<SqlParameter>();
 
int i = 1;
foreach (var filter in filters)
{
    sql += (i == 1 ? "and" : "where") + $" Field{i} = @filter{i} ";                    
    parameters.Add(new SqlParameter($"@filter{i++}", filter));
}
 
var result = _paymentsDbContext.MyTable
    .FromSqlRaw(sql, parameters.ToArray())
    .ToList();
```


# Collaborate

This is a brand new project, and your contribution can help a lot.  

**Would you like to collaborate?**  

Please submit a pull-request or if you prefer you can [create an issue](https://github.com/Drizin/EntityFrameworkCore.RawQueryBuilder/issues) to discuss your idea.


## License
MIT License
