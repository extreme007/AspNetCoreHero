using AspNetCoreHero.Application.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace AspNetCoreHero.Application.Wrappers
{
    public class PagedResponse<T> : Response<T>
    {
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }

        public PagedResponse(T pagedData, RequestParameter request, int totalRecords = 0)
        {
            this.PageNumber = request.PageNumber;
            this.PageSize = request.PageSize;
            this.TotalPages = Convert.ToInt32(Math.Ceiling((double)totalRecords / (double)request.PageSize));
            this.TotalRecords = totalRecords;
            this.Data = pagedData;
            this.Message = null;
            this.Succeeded = true;
            this.Errors = null;
        }     
    }
}
