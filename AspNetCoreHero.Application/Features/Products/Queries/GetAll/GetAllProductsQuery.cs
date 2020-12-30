using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Parameters;
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
        private readonly PaginationSettings _paginationSettings;
        public GetAllProductsQueryHandler(IProductRepositoryAsync productRepository, IMapper mapper, IOptions<PaginationSettings> paginationSettings)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _paginationSettings = paginationSettings.Value;
        }

        public async Task<PagedResponse<IEnumerable<GetAllProductsViewModel>>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize < 1 ? _paginationSettings.PageSize : request.PageSize;
            int totalRecords = await _productRepository.CountAsync();
            var validRequest = new RequestParameter(request.PageNumber, pageSize);
            if (request.PageNumber == 0) // get All
            {
                validRequest = new RequestParameter(1, totalRecords);
                //var data = await _productRepository.GetAllAsync();
                var data = await  _productRepository.GetAllIncludingAsync(x=>x.ProductCategory);                
                var allProductViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(data);
                return new PagedResponse<IEnumerable<GetAllProductsViewModel>>(allProductViewModel, validRequest, totalRecords);
            }

            //var products =  _productRepository.GetAllIncluding(x=>x.ProductCategory);
            //var dataPaged = products.Skip((validRequest.PageNumber - 1) * pageSize)
            //    .Take(pageSize);            
            var dataPaged = await _productRepository.GetPagedResponseAsync(validRequest.PageNumber, validRequest.PageSize, "ProductCategory");
            var productsViewModel = _mapper.Map<IEnumerable<GetAllProductsViewModel>>(dataPaged);         
            return new PagedResponse<IEnumerable<GetAllProductsViewModel>>(productsViewModel, validRequest, totalRecords);
        }
    }
}
