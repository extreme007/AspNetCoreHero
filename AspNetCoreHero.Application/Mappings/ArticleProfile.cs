using AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetAll;
using AspNetCoreHero.Application.Features.Articles.Queries.GetAll;
using AspNetCoreHero.Application.Features.Articles.Queries.GetById;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Mappings
{
    public class ArticleProfile : Profile
    {
        public ArticleProfile()
        {
            CreateMap<Article, GetAllArticlesViewModel>().ReverseMap();
            CreateMap<Article, GetByIdArticleViewModel>().ReverseMap();
        }
    }
}
