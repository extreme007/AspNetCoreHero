using AspNetCoreHero.Application.Features.ArticleCategories.Queries.GetAll;
using AspNetCoreHero.Domain.Entities;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Mappings
{
    public class ArticleCategoryProfile : Profile
    {
        public ArticleCategoryProfile()
        {
            CreateMap<ArticleCategory, GetAllArticleCategoryViewModel>().ReverseMap();
        }
    }
}
