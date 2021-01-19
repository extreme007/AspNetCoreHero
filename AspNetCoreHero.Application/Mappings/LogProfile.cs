using AspNetCoreHero.Application.DTOs.Logs;
using AspNetCoreHero.Domain.Common;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Mappings
{
    class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<AuditLogResponse, Audit>().ReverseMap();
        }
    }
}
