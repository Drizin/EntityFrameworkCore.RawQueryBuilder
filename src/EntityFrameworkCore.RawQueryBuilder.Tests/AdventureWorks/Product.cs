using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace EntityFrameworkCore.RawQueryBuilder.Tests.AdventureWorks
{
    [Table("Product", Schema = "production")]
    public partial class Product
    {
        public Product()
        {

        }

        [Key] public int Productid { get; set; }

        public string Name { get; set; }
        public decimal Listprice { get; set; }
        public decimal? Weight { get; set; }
    }
}
