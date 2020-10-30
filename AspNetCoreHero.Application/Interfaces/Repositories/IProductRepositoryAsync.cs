using AspNetCoreHero.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AspNetCoreHero.Application.Interfaces.Repositories
{
    public interface IProductRepositoryAsync : IGenericRepositoryAsync<Product>
    {
        Task<IReadOnlyList<Product>> GetAllWithCategoriesAsync(int pageNumber, int pageSize,bool isCached = false);
        Task<IReadOnlyList<Product>> GetAllWithCategoriesWithoutImagesAsync(int pageNumber,int pageSize, bool isCached = false);
        Task<bool> IsUniqueBarcodeAsync(string barcode);
    }
}
