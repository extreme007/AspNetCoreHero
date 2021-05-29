using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Features.Articles.Queries.GetAll;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Wrappers;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.Articles.Queries.GetById
{
    public class GetArticleByIdQuery : IRequest<Response<GetByIdArticleViewModel>>
    {
        public int Id { get; set; }
        public class GetArticleByIdQueryHandler : IRequestHandler<GetArticleByIdQuery, Response<GetByIdArticleViewModel>>
        {
            private readonly IArticleRepositoryAsync _articleRepository;
            private readonly IMapper _mapper;
            public GetArticleByIdQueryHandler(IArticleRepositoryAsync articleRepository, IMapper mapper)
            {
                _articleRepository = articleRepository;
                _mapper = mapper;
            }
            public async Task<Response<GetByIdArticleViewModel>> Handle(GetArticleByIdQuery query, CancellationToken cancellationToken)
            {
                var article = await _articleRepository.GetByIdAsync(query.Id);
                if (article == null) throw new NotFoundException<Article>(query.Id);
                var articlesViewModel = _mapper.Map<GetByIdArticleViewModel>(article);
                return new Response<GetByIdArticleViewModel>(articlesViewModel);
            }
        }
    }
}
