using AspNetCoreHero.Application.DTOs.Settings;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Parameters;
using AspNetCoreHero.Application.Wrappers;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetAll
{
    public class GetAllArticleCategoriesQuery : IRequest<Response<IEnumerable<GetAllArticleCategoryViewModel>>>
    {

    }

    public class GetAllArticleCategoryQueryHandler : IRequestHandler<GetAllArticleCategoriesQuery, Response<IEnumerable<GetAllArticleCategoryViewModel>>>
    {
        private readonly IArticleCategoryRepositoryAsync _articleCategoryRepository;
        private readonly IMapper _mapper;

        public GetAllArticleCategoryQueryHandler(IArticleCategoryRepositoryAsync articleCategoryRepository, IMapper mapper)
        {
            _articleCategoryRepository = articleCategoryRepository;
            _mapper = mapper;
        }

        public async Task<Response<IEnumerable<GetAllArticleCategoryViewModel>>> Handle(GetAllArticleCategoriesQuery request, CancellationToken cancellationToken)
        {
            var categories = await _articleCategoryRepository.GetAllAsync();
            var categoriesViewModel = _mapper.Map<IEnumerable<GetAllArticleCategoryViewModel>>(categories);
            return new Response<IEnumerable<GetAllArticleCategoryViewModel>>(categoriesViewModel);
        }
    }
}
