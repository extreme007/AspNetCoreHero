using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreHero.PublicAPI.Controllers.v1
{
    [Authorize]
    public class TestController : BaseApiController
    {
        [HttpGet]
        [Authorize(Roles = "Basic")]
        public IActionResult Get()
        {
            return Ok(new string[] { "value 1", "value 2"});
        }
    }
}
