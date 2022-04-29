using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Helpers;

namespace API.Extensions
{
    // Allows adding Pagination headers to Http Response.
    public static class HttpExtensions
    {
        public static void AddPaginationHeader(this HttpResponse response, int currentPage, 
            int itemsPerPage, int totalItems, int totalPages)
            {
               var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);
               response.Headers.Add("Pagination") 
            }
    }
}