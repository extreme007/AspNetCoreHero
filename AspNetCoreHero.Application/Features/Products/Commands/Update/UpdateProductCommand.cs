using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Features.ProductCategories.Queries.GetAll;
using AspNetCoreHero.Application.Features.Products.Queries.GetAll;
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

namespace AspNetCoreHero.Application.Features.Products.Commands.Update
{
    public class UpdateProductCommand : IRequest<Response<GetAllProductsViewModel>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string Barcode { get; set; }
        public decimal Price { get; set; }
        public int ProductCategoryId { get; set; }
        public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, Response<GetAllProductsViewModel>>
        {
            private readonly IProductRepositoryAsync _productRepository;
            private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            public UpdateProductCommandHandler(IProductRepositoryAsync productRepository, IProductCategoryRepositoryAsync productCategoryRepository, IUnitOfWork unitOfWork, IMapper mapper)
            {
                _productRepository = productRepository;
                _productCategoryRepository = productCategoryRepository;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
            }
            public async Task<Response<GetAllProductsViewModel>> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
            {
                var product = await _productRepository.GetByIdAsync(command.Id);

                if (product == null)
                {
                    throw new ApiException($"Product Not Found.");
                }
                else
                {
                    product.Name = command.Name;
                    product.Price = command.Price;
                    product.Description = command.Description;
                    product.Image = command.Image;
                    product.ProductCategoryId = command.ProductCategoryId;
                    product.Barcode = command.Barcode;
                    await _productRepository.UpdateAsync(product,command.Id);
                    var result = await _unitOfWork.Commit(cancellationToken);
                    if (result > 0)
                    {
                        var productsViewModel = _mapper.Map<GetAllProductsViewModel>(product);
                        var productCategory = await _productCategoryRepository.GetByIdAsync(productsViewModel.ProductCategoryId);
                        var productCategoryViewModel = _mapper.Map<GetAllProductCategoryViewModel>(productCategory);
                        productsViewModel.ProductCategory = productCategoryViewModel;
                        return new Response<GetAllProductsViewModel>(productsViewModel);
                    }
                      
                    return new Response<GetAllProductsViewModel>(null);
                }
            }
        }
    }
}
