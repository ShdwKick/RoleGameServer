using HotChocolate.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Data.Helpers
{
    public class Helpers
    {

        public static string GetTokenFromHeader(IHttpContextAccessor httpContextAccessor)
        {
            if (httpContextAccessor == null)
                throw new ArgumentException("ERROR_OCCURRED");

            var httpContext = httpContextAccessor.HttpContext;
            string authorizationHeader = httpContext.Request.Headers["Authorization"];

            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                throw new ArgumentException("INVALID_AUTHORIZATION_HEADER_PROBLEM");
            }

            return authorizationHeader.Substring("Bearer ".Length).Trim();
        }
    }
}
