using AspNetCoreHero.Application.Filters;
using AspNetCoreHero.Application.Helpers;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using MediatR;
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
        public GetAllProductsQueryHandler(IProductCategoryRepositoryAsync productCategoryRepository, IMapper mapper)
        {
            _productCategoryRepository = productCategoryRepository;
            _mapper = mapper;
        }

        public async Task<PagedResponse<IEnumerable<GetAllProductCategoryViewModel>>> Handle(GetAllProductCategoriesQuery request, CancellationToken cancellationToken)
        {
            int totalRecords = await _productCategoryRepository.CountAsync();
            var validFilter = new PaginationFilter(request.PageNumber, request.PageSize, totalRecords);
            var categories = await _productCategoryRepository.GetPagedReponseAsync(validFilter.PageNumber, validFilter.PageSize);
            var categoriesViewModel = _mapper.Map<IEnumerable<GetAllProductCategoryViewModel>>(categories);

            var result = PaginationHelper.CreatePagedReponse<GetAllProductCategoryViewModel>(categoriesViewModel, validFilter, totalRecords);
            return result;
        }
    }
}
