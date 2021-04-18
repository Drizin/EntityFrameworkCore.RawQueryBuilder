using EntityFrameworkCore.RawQueryBuilder.Tests.AdventureWorks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace EntityFrameworkCore.RawQueryBuilder.Tests
{
    public class FluentQueryBuilderTests : BaseTests
    {

        string expected = @"SELECT ProductId, Name, ListPrice, Weight
FROM [Production].[Product]
WHERE [ListPrice] <= {0} AND [Weight] <= {1} AND [Name] LIKE {2}
ORDER BY ProductId
";

        [Test]
        public void TestFluentAPI()
        {
            int maxPrice = 1000;
            int maxWeight = 15;
            string search = "%Mountain%";

            var q = dbSet.FluentQueryBuilder()
                .Select($"ProductId")
                .Select($"Name")
                .Select($"ListPrice")
                .Select($"Weight")
                .From($"[Production].[Product]")
                .Where($"[ListPrice] <= {maxPrice}")
                .Where($"[Weight] <= {maxWeight}")
                .Where($"[Name] LIKE {search}")
                .OrderBy($"ProductId")
                ;

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
