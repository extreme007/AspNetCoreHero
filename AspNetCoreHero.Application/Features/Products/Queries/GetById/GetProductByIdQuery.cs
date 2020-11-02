using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Domain.Entities;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.Products.Queries.GetById
{
    public class GetProductByIdQuery : IRequest<Response<Product>>
    {
        public int Id { get; set; }
        public class GetProductByIdQueryHandler : IRequestHandler<GetProductByIdQuery, Response<Product>>
        {
            private readonly IProductRepositoryAsync _productRepository;
            private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
            public GetProductByIdQueryHandler(IProductRepositoryAsync productRepository, IProductCategoryRepositoryAsync productCategoryRepository)
            {
                _productRepository = productRepository;
                _productCategoryRepository = productCategoryRepository;
            }
            public async Task<Response<Product>> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(query.Id);
                
                if (product == null) throw new NotFoundException<Product>(query.Id);
                product.ProductCategory = await _productCategoryRepository.GetByIdAsync(product.ProductCategoryId);
                return new Response<Product>(product);
            }
        }
    }
}
