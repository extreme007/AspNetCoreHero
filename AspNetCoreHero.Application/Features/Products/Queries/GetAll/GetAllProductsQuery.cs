using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreHero.Application.Configurations;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll;
using AspNetCoreHero.Application.Filters;
using AspNetCoreHero.Application.Helpers;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;

namespace AspNetCoreHero.Application.Features.Products.Queries.GetAll
{

    public class GetAllProductsQuery : IRequest<PagedResponse<IEnumerable<GetAllProductsViewModel>>>
    {
        public bool ReturnImages { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductsQuery, PagedResponse<IEnumerable<GetAllProductsViewModel>>>
    {
        private readonly IProductRepositoryAsync _productRepository;
        private readonly IMapper _mapper;
        private readonly PaginationConfiguration _paginationConfiguration;
        public GetAllProductsQueryHandler(IProductRepositoryAsync productRepository, IMapper mapper, IOptions<PaginationConfiguration> paginationConfiguration)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _paginationConfiguration = paginationConfiguration.Value;
        }

        public async Task<PagedResponse<IEnumerable<GetAllProductsViewModel>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            //var product = new List<Product>();
            //var pageSize = request.PageSize < 1 ? _paginationConfiguration.PageSize : request.PageSize;
            //var validFilter = new PaginationFilter(request.PageNumber, pageSize);
            //if (request.ReturnImages)
            //{
            //    var productWithImages = await _productRepository.GetAllWithCategoriesAsync(validFilter.PageNumber, validFilter.PageSize);
            //    product = productWithImages.ToList();
            //}
            //else
            //{
            //    var productWithoutImages = await _productRepository.GetAllWithCategoriesWithoutImagesAsync(validFilter.PageNumber,validFilter.PageSize);
            //    product = productWithoutImages.ToList();
            //}
            //var productViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(product);
            //var result = PaginationHelper.CreatePagedReponse<GetAllProductsViewModel>(productViewModel, validFilter,product.Count());
            //return result;


            int totalRecords = await _productRepository.CountAsync();
            var pageSize = request.PageSize < 1 ? _paginationConfiguration.PageSize : request.PageSize;
            var validFilter = new PaginationFilter(request.PageNumber, pageSize);
            var products = await _productRepository.GetPagedReponseAsync(validFilter.PageNumber, validFilter.PageSize, "ProductCategory");
            var productsViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(products);
            var result = PaginationHelper.CreatePagedReponse<GetAllProductsViewModel>(productsViewModel, validFilter, totalRecords);
            return result;
        }
    }
}
