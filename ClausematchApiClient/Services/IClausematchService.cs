using ClauseMatchGraphConnector.ClausematchApiClient.Models;
using ClauseMatchGraphConnector.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public interface IClausematchService
    {
        Task<List<Category>> GetAllCategoriesAsync(string jwtToken,  Settings settings);
        Task<List<ClausematchDocument>> GetAllDocumentsByCategoryAsync(string jwtToken, string categoryId, Settings settings);
    }
}
