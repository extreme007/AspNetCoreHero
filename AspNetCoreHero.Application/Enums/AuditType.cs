using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Enums
{
    public enum AuditType
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
        Login = 4,
        Logout = 5,
    }
}
