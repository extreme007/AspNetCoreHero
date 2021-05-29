using AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetAll;
using AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetById;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCoreHero.PublicAPI.Controllers.v1
{
    public class ArticleCategoryController : BaseApiController
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            return Ok(await _mediator.Send(new GetAllArticleCategoriesQuery()));
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            return Ok(await _mediator.Send(new GetArticleCategoryByIdQuery { Id = id }));
        }
    }
}
