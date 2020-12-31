using AspNetCoreHero.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Domain.Entities
{
    public class ProductCategory : AuditableBaseEntity
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Tax { get; set; }
    }
}
