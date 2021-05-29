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

namespace AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetById
{
    public class GetArticleCategoryByIdQuery : IRequest<Response<ArticleCategory>>
    {
        public int Id { get; set; }
        public class GetArticleCategoryByIdQueryHandler : IRequestHandler<GetArticleCategoryByIdQuery, Response<ArticleCategory>>
        {
            private readonly IArticleCategoryRepositoryAsync _articleCategoryRepository;
            public GetArticleCategoryByIdQueryHandler(IArticleCategoryRepositoryAsync articleCategoryRepository)
            {
                _articleCategoryRepository = articleCategoryRepository;
            }
            public async Task<Response<ArticleCategory>> Handle(GetArticleCategoryByIdQuery query, CancellationToken cancellationToken)
            {
                var category = await _articleCategoryRepository.GetByIdAsync(query.Id);
                if (category == null) throw new NotFoundException<ArticleCategory>(query.Id);
                return new Response<ArticleCategory>(category);
            }
        }
    }
}
