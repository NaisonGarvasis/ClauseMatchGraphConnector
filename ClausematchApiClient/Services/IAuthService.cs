﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClauseMatchGraphConnector.ClausematchApiClient.Services
{
    public interface IAuthService
    {
        Task<string> GetJwtTokenAsync(Settings settings);
    }
}
