using System.Threading.Tasks;
using AspNetCoreHero.Application.Features.ProductCategories.Commands.Create;
using AspNetCoreHero.Application.Features.ProductCategories.Commands.Delete;
using AspNetCoreHero.Application.Features.ProductCategories.Commands.Update;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetById;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AspNetCoreHero.PublicAPI.Controllers.v1
{
    [Authorize]
    public class ProductCategoryController : BaseApiController
    {
        [HttpGet]
        [Authorize(Roles ="Basic")]
        public async Task<IActionResult> Get([FromQuery] GetAllProductCategoriesQuery filter)
        {           
            return Ok(await _mediator.Send(new GetAllProductCategoriesQuery() { PageSize = filter.PageSize, PageNumber = filter.PageNumber }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {         
            return Ok(await _mediator.Send(new GetProductCategoryByIdQuery { Id = id }));
        }

        // POST api/<controller>
        [HttpPost]
        public async Task<IActionResult> Post(CreateProductCategoryCommand command)
        {
            return Ok(await _mediator.Send(command));
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, UpdateProductCategoryCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest();
            }
            return Ok(await _mediator.Send(command));
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id, bool Physical)
        {
            return Ok(await _mediator.Send(new DeleteProductCategoryByIdCommand { Id = id, Physical = Physical }));
        }
    }
}
