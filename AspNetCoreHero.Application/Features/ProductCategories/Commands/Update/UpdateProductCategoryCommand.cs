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

namespace AspNetCoreHero.Application.Features.ProductCategories.Commands.Update
{
    public class UpdateProductCategoryCommand : IRequest<Response<ProductCategory>>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Tax { get; set; }
        public string Description { get; set; }
        public class UpdateProductCategoryCommandHandler : IRequestHandler<UpdateProductCategoryCommand, Response<ProductCategory>>
        {
            private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            public UpdateProductCategoryCommandHandler(IProductCategoryRepositoryAsync productCategoryRepository, IUnitOfWork unitOfWork)
            {
                _productCategoryRepository = productCategoryRepository;
                _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            }
            public async Task<Response<ProductCategory>> Handle(UpdateProductCategoryCommand command, CancellationToken cancellationToken)
            {
                var category = await _productCategoryRepository.GetByIdAsync(command.Id);

                if (category == null)
                {
                    throw new ApiException($"Product Category Not Found.");
                }
                else
                {
                    category.Name = command.Name;
                    category.Tax = command.Tax;
                    category.Description = command.Description;
                    await _productCategoryRepository.UpdateAsync(category);
                    var result = await _unitOfWork.Commit(cancellationToken);
                    if(result > 0)
                        return new Response<ProductCategory>(category);
                    return new Response<ProductCategory>(null);
                }
            }
        }
    }
}
