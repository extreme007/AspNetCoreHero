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

namespace AspNetCoreHero.Application.Features.Articles.Queries.GetAll
{
    public class GetAllArticlesQuery : IRequest<PagedResponse<IEnumerable<GetAllArticlesViewModel>>>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
    }

    public class GetAllArticleCategoryQueryHandler : IRequestHandler<GetAllArticlesQuery, PagedResponse<IEnumerable<GetAllArticlesViewModel>>>
    {
        private readonly IArticleRepositoryAsync _articleRepository;
        private readonly IMapper _mapper;
        private readonly PaginationSettings _paginationSettings;

        public GetAllArticleCategoryQueryHandler(IArticleRepositoryAsync articleRepository, IMapper mapper, IOptions<PaginationSettings> paginationSettings)
        {
            _articleRepository = articleRepository;
            _mapper = mapper;
            _paginationSettings = paginationSettings.Value;
        }

        public async Task<PagedResponse<IEnumerable<GetAllArticlesViewModel>>> Handle(GetAllArticlesQuery request, CancellationToken cancellationToken)
        {
            var pageSize = request.PageSize < 1 ? _paginationSettings.PageSize : request.PageSize;
            int totalRecords = await _articleRepository.CountAsync();
            var validRequest = new RequestParameter(request.PageNumber, pageSize);
            if (request.PageNumber == 0) // get All
            {
                validRequest = new RequestParameter(1, totalRecords);
                var data = await _articleRepository.GetAllAsync();
                var allArticlesViewModel = _mapper.Map<IEnumerable<GetAllArticlesViewModel>>(data);
                return new PagedResponse<IEnumerable<GetAllArticlesViewModel>>(allArticlesViewModel, validRequest, totalRecords);
            }           
            var dataPaged = await _articleRepository.GetPagedResponseAsync(validRequest.PageNumber, validRequest.PageSize);
            var articleViewModel = _mapper.Map<IEnumerable<GetAllArticlesViewModel>>(dataPaged);
            return new PagedResponse<IEnumerable<GetAllArticlesViewModel>>(articleViewModel, validRequest, totalRecords);
        }
    }
}
