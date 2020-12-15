using AspNetCoreHero.Application.Configurations;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Parameters;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll
{
    public class GetAllProductCategoriesQuery : IRequest<PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }
    public class GetAllProductsQueryHandler : IRequestHandler<GetAllProductCategoriesQuery, PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>>
    {
        private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
        private readonly IMapper _mapper;
        private readonly PaginationConfiguration _paginationConfiguration;

        public GetAllProductsQueryHandler(IProductCategoryRepositoryAsync productCategoryRepository, IMapper mapper,IOptions<PaginationConfiguration> paginationConfiguration)
        {
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
            _paginationConfiguration = paginationConfiguration.Value;
        }

        public async Task<PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize < 1 ? _paginationConfiguration.PageSize : request.PageSize;
            int totalRecords = await _productCategoryRepository.CountAsync();
            var validRequest = new RequestParameter(request.PageNumber, pageSize);
            if (request.PageNumber == 0) // get All
            {
                validRequest = new RequestParameter(1, totalRecords);
                var data = await _productCategoryRepository.GetAllAsync();
                var allCategoriesViewModel = _mapper.Map<IEnumerable<GetAllProductCategoryViewModel>>(data);
                return new PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>(allCategoriesViewModel, validRequest, totalRecords);
            }         

            var categories = await _productCategoryRepository.GetPagedResponseAsync(validRequest.PageNumber, validRequest.PageSize);
            var categoriesViewModel = _mapper.Map<IEnumerable<GetAllProductCategoryViewModel>>(categories);
            return new PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>(categoriesViewModel, validRequest, totalRecords);
        }
    }
}
