using AspNetCoreHero.Application.Features.Articles.Queries.GetAll;
using AspNetCoreHero.Application.Features.Articles.Queries.GetById;
using AspNetCoreHero.Application.Parameters;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.PublicAPI.Controllers.v1
{
    public class ArticleController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] RequestParameter filter)
        {
            return Ok(await _mediator.Send(new GetAllArticlesQuery() { PageSize = filter.PageSize, PageNumber = filter.PageNumber }));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _mediator.Send(new GetArticleByIdQuery { Id = id }));
        }
    }
}
