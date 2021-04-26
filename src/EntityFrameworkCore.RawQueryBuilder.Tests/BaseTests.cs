using EntityFrameworkCore.RawQueryBuilder.Tests.AdventureWorks;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder.Tests
{
    public class BaseTests
    {
        protected DbSet<Product> dbSet;

        #region Setup
        [SetUp]
        public void Setup()
        {
            string connectionString = "Server=WIN10VM2021\\SQLEXPRESS; Database=AdventureWorks2019; Integrated Security=True";
            var opts = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder<AdventureWorksDbContext>(), connectionString).Options;
            var context = new AdventureWorksDbContext(opts);
            dbSet = context.Products;
        }

        #endregion

    }
}
