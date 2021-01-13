using AspNetCoreHero.Application.DTOs.Logs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Interfaces.Repositories
{
    public interface ILogRepository
    {
        Task<List<AuditLogResponse>> GetAuditLogsAsync(string userId);

        Task AddLogAsync(string action, string userId,string IpAddress = null);
    }
}
