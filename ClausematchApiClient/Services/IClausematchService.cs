using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public interface IClausematchService
    {
        Task<List<Category>> GetAllCategoriesAsync(string jwtToken);
        Task<List<Document>> GetAllDocumentsByCategoryAsync(string jwtToken, string categoryId);
    }
}
