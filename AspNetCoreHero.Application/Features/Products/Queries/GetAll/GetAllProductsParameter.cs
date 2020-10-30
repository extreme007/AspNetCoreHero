using AspNetCoreHero.Application.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Features.Products.Queries.GetAll
{
    public class GetAllProductsParameter : PaginationFilter
    {
        public bool ReturnImages { get; set; } = true;
    }
}
