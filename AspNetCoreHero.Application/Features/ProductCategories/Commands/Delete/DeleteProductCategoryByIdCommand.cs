using AspNetCoreHero.Application.Exceptions;
using AspNetCoreHero.Application.Interfaces.Repositories;
using AspNetCoreHero.Application.Interfaces.Shared;
using AspNetCoreHero.Application.Wrappers;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.ProductCategories.Commands.Delete
{
    public class DeleteProductCategoryByIdCommand : IRequest<Response<int>>
    {
        public int Id { get; set; }
        public bool Physical { get; set; } = false;
        public class DeleteProductCategoryByIdCommandHandler : IRequestHandler<DeleteProductCategoryByIdCommand, Response<int>>
        {
            private readonly IProductCategoryRepositoryAsync _productCategoryRepository;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IDateTimeService _dateTime;
            private readonly IAuthenticatedUserService _authenticatedUser;
            public DeleteProductCategoryByIdCommandHandler(IProductCategoryRepositoryAsync productRepository, IUnitOfWork unitOfWork, IDateTimeService dateTime, IAuthenticatedUserService authenticatedUser)
            {
                _productCategoryRepository = productRepository;
                _unitOfWork = unitOfWork;
                _dateTime = dateTime;
                _authenticatedUser = authenticatedUser;
            }
            public async Task<Response<int>> Handle(DeleteProductCategoryByIdCommand command, CancellationToken cancellationToken)
            {
                var category = await _productCategoryRepository.GetByIdAsync(command.Id);
                if (category == null) throw new ApiException($"Product category Not Found.");
                if (command.Physical)
                {
                    await _productCategoryRepository.DeleteAsync(category);
                }
                else //Update status IsDeleted
                {
                    category.IsDeleted = true;
                    await _productCategoryRepository.UpdateAsync(category);
                }

                await _unitOfWork.Commit(cancellationToken);
                return new Response<int>(category.Id);
            }
        }
    }
}
