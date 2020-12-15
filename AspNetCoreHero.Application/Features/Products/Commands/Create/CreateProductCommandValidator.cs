using AspNetCoreHero.Application.Interfaces.Repositories;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Features.Products.Commands.Create
{
    public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        private readonly IProductRepositoryAsync _productRepository;

        public CreateProductCommandValidator(IProductRepositoryAsync productRepository)
        {
            this._productRepository = productRepository;
            RuleFor(p => p.Barcode)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.")
                .MustAsync(IsUniqueBarcode).WithMessage("{PropertyName} already exists.");

            RuleFor(p => p.Name)
                .NotEmpty().WithMessage("{PropertyName} is required.")
                .NotNull()
                .MinimumLength(10)
                .MaximumLength(50).WithMessage("{PropertyName} must not exceed 50 characters.");

        }

        private async Task<bool> IsUniqueBarcode(string barcode, CancellationToken cancellationToken)
        {
            return await _productRepository.IsUniqueBarcodeAsync(barcode);
        }
    }
}
