using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Domain.Common
{
    public abstract class BaseEntity : IBaseEntity
    {
        public int Id { get; set; }
    }
}
