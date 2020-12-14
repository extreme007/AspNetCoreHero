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
            int totalRecords = await _productRepository.CountAsync();
            if (request.PageNumber == 0) // get All
            {
                var data = await _productRepository.GetAllAsync();
                var allProductViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(data);
                var filterAll = new PaginationFilter(1, totalRecords);
                return PaginationHelper.CreatePagedReponse<GetAllProductsViewModel>(allProductViewModel, filterAll, totalRecords);
            }
            var pageSize = request.PageSize < 1 ? _paginationConfiguration.PageSize : request.PageSize;
            var validFilter = new PaginationFilter(request.PageNumber, pageSize);
            //var products =  _productRepository.GetAllIncluding(x=>x.ProductCategory);
            //var dataPaged = products.Skip((validFilter.PageNumber - 1) * pageSize)
            //    .Take(pageSize);            
            var dataPaged = await _productRepository.GetPagedResponseAsync(validFilter.PageNumber, validFilter.PageSize, "ProductCategory");
            var productsViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(dataPaged);
            var result = PaginationHelper.CreatePagedReponse<GetAllProductsViewModel>(productsViewModel, validFilter, totalRecords);
            return result;
        }
    }
}
