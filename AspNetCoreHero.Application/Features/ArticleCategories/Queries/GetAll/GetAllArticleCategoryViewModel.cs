using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetAll
{
    public class GetAllArticleCategoryViewModel
    {
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string RewriteURL { get; set; }
        public int? ParentId { get; set; }
    }
}
