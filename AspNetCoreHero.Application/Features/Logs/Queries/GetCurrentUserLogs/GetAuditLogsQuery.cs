using AspNetCoreHero.Application.DTOs.Logs;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.Logs.Queries.GetCurrentUserLogs
{
    public class GetAuditLogsQuery : IRequest<Response<List<AuditLogResponse>>>
    {
        public string userId { get; set; }

        public GetAuditLogsQuery()
        {
        }
    }

    public class GetAuditLogsQueryHandler : IRequestHandler<GetAuditLogsQuery, Response<List<AuditLogResponse>>>
    {
        private readonly ILogRepository _logRepository;
        private readonly IMapper _mapper;

        public GetAuditLogsQueryHandler(ILogRepository logRepository, IMapper mapper)
        {
            _logRepository = logRepository;
            _mapper = mapper;
        }

        public async Task<Response<List<AuditLogResponse>>> Handle(GetAuditLogsQuery request, CancellationToken cancellationToken)
        {
            var logsData = await _logRepository.GetAuditLogsAsync(request.userId);
            var mappedLogs = _mapper.Map<List<AuditLogResponse>>(logsData);
            return new Response<List<AuditLogResponse>>(mappedLogs);
        }
    }
}
