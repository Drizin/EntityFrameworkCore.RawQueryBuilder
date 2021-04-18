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
            var context = new AdventureWorksDbContext();
            dbSet = context.Products;

            //TODO: integration tests with real database
        }
        #endregion

    }
}
