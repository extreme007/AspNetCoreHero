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

namespace AspNetCoreHero.Application.Features.ProductCategories.Commands.Create
{
    public partial class CreateProductCategoryCommand : IRequest<Response<ProductCategory>>
    {
        public string Name { get; set; }
        public decimal Tax { get; set; }
        public string Description { get; set; }
    }
    public class CreateProductCategoryCommandHandler : IRequestHandler<CreateProductCategoryCommand, Response<ProductCategory>>
    {
        private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
        private readonly IMapper _mapper;

        private IUnitOfWork _unitOfWork { get; set; }
        public CreateProductCategoryCommandHandler(IProductCategoryRepositoryAsync productCategoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
        {
            _productCategoryRepository = productCategoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Response<ProductCategory>> Handle(CreateProductCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = _mapper.Map<ProductCategory>(request);
            await _productCategoryRepository.AddAsync(category);
            var result = await _unitOfWork.Commit(cancellationToken);
            if (result > 0)
                return new Response<ProductCategory>(category);
            return new Response<ProductCategory>(null);
        }
    }
}
