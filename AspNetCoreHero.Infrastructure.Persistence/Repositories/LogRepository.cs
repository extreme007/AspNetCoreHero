using AspNetCoreHero.Application.DTOs.Logs;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Domain.Common;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Infrastructure.Persistence.Repositories
{
    public class LogRepository : ILogRepository
    {
        private readonly IMapper _mapper;
        private readonly IGenericRepositoryAsync<Audit> _repository;
        private readonly IDateTimeService _dateTimeService;

        public LogRepository(IGenericRepositoryAsync<Audit> repository, IMapper mapper, IDateTimeService dateTimeService)
        {
            _repository = repository;
            _mapper = mapper;
            _dateTimeService = dateTimeService;
        }

        public async Task AddLogAsync(string action, string userId,string IpAddress)
        {
            var audit = new Audit()
            {
                Type = action,
                UserId = userId,
                DateTime = _dateTimeService.NowUtc,
                IpAddress = IpAddress
            };
            await _repository.AddAsync(audit);

        }

        public async Task<List<AuditLogResponse>> GetAuditLogsAsync(string userId)
        {
            var logs = await _repository.FindAllAsync(a => a.UserId == userId);
            var mappedLogs = _mapper.Map<List<AuditLogResponse>>(logs);
            return mappedLogs;
        }
    }

    public class LogProfile : Profile
    {
        public LogProfile()
        {
            CreateMap<AuditLogResponse, Audit>().ReverseMap();
        }
    }
}
