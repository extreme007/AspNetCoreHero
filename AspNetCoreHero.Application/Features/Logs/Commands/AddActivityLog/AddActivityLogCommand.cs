using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.Logs.Commands.AddActivityLog
{
    public partial class AddActivityLogCommand : IRequest<Response<int>>
    {
        public string Action { get; set; }
        public string UserId { get; set; }
    }
    public class AddActivityLogCommandHandler : IRequestHandler<AddActivityLogCommand, Response<int>>
    {
        private readonly ILogRepository _logRepository;

        private IUnitOfWork _unitOfWork { get; set; }

        public AddActivityLogCommandHandler(ILogRepository logRepository, IUnitOfWork unitOfWork)
        {
            _logRepository = logRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Response<int>> Handle(AddActivityLogCommand request, CancellationToken cancellationToken)
        {
            await _logRepository.AddLogAsync(request.Action, request.UserId);
            var result = await _unitOfWork.Commit(cancellationToken);
            if (result > 0)
                return new Response<int>(result);
            return new Response<int>(0);
        }
    }
}
