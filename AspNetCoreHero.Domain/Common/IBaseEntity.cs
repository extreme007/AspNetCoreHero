using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Domain.Common
{
    public interface IBaseEntity
    {
        public int Id { get; set; }
    }
}
