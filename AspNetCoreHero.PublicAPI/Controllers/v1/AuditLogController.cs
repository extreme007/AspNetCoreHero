using AspNetCoreHero.Application.Features.Logs.Commands.AddActivityLog;
using AspNetCoreHero.Application.Features.Logs.Queries.GetCurrentUserLogs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.PublicAPI.Controllers.v1
{
    [Authorize]
    public class AuditLogController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] GetAuditLogsQuery filter)
        {
            return Ok(await _mediator.Send(new GetAuditLogsQuery() { userId = filter.userId}));
        }

        [HttpPost]
        public async Task<IActionResult> Post(AddActivityLogCommand command)
        {
            return Ok(await _mediator.Send(command));
        }
    }
}
