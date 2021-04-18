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
        public string Productnumber { get; set; }
        public bool Makeflag { get; set; }
        public bool Finishedgoodsflag { get; set; }
        public string Color { get; set; }
        public short Safetystocklevel { get; set; }
        public short Reorderpoint { get; set; }
        public decimal Standardcost { get; set; }
        public decimal Listprice { get; set; }
        public string Size { get; set; }
        public string Sizeunitmeasurecode { get; set; }
        public string Weightunitmeasurecode { get; set; }
        public decimal? Weight { get; set; }
        public int Daystomanufacture { get; set; }
        public string Productline { get; set; }
        public string Class { get; set; }
        public string Style { get; set; }
        public int? Productsubcategoryid { get; set; }
        public int? Productmodelid { get; set; }
        public DateTime Sellstartdate { get; set; }
        public DateTime? Sellenddate { get; set; }
        public DateTime? Discontinueddate { get; set; }
        public Guid Rowguid { get; set; }
        public DateTime Modifieddate { get; set; }

    }
}
