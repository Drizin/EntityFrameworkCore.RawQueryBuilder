using EntityFrameworkCore.RawQueryBuilder.Tests.AdventureWorks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace EntityFrameworkCore.RawQueryBuilder.Tests
{
    public class QueryBuilderTests : BaseTests
    {
        string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= {0} AND [Weight] <= {1} AND [Name] LIKE {2}
ORDER BY ProductId
";

        int maxPrice = 1000;
        int maxWeight = 15;
        string search = "%Mountain%";


        [Test]
        public void TestTemplateAPI()
        {

            var q = dbSet.RawQueryBuilder(
$@"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
/**where**/
ORDER BY ProductId
");
            q.Where($"[ListPrice] <= {maxPrice}");
            q.Where($"[Weight] <= {maxWeight}");
            q.Where($"[Name] LIKE {search}");

            Assert.AreEqual(expected, q.Sql);
            Assert.AreEqual(maxPrice, q.Parameters.Items[0]);
            Assert.AreEqual(maxWeight, q.Parameters.Items[1]);
            Assert.AreEqual(search, q.Parameters.Items[2]);

            //Assert.That(q.Parameters.ParameterNames.Contains("p0"));
            //Assert.That(q.Parameters.ParameterNames.Contains("p1"));
            //Assert.That(q.Parameters.ParameterNames.Contains("p2"));
            //Assert.AreEqual(q.Parameters.Get<int>("p0"), maxPrice);
            //Assert.AreEqual(q.Parameters.Get<int>("p1"), maxWeight);
            //Assert.AreEqual(q.Parameters.Get<string>("p2"), search);

            //var products = q.AsQueryable().ToList();
            //Assert.That(products.Any());
        }
    }
}
