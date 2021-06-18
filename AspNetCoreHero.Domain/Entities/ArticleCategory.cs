using AspNetCoreHero.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Domain.Entities
{
    public class ArticleCategory : AuditableBaseEntity
    {
        public string Title { get; set; }
        public string MetaTitle { get; set; }
        public string RewriteURL { get; set; }
        public int? ParentId { get; set; }
    }
}
