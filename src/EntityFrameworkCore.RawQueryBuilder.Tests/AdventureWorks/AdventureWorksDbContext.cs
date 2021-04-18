using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder.Tests.AdventureWorks
{
    public class AdventureWorksDbContext : DbContext
    {
        public AdventureWorksDbContext()
        {
        }

        public AdventureWorksDbContext(DbContextOptions<AdventureWorksDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Product> Products { get; set; }


    }
}
