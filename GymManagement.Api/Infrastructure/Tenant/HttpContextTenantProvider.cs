using GymManagement.Api.Features.Gyms.Domain;
using Microsoft.AspNetCore.Http;
using System;
using GymManagement.Api.Shared.Security;
using System.Security.Claims;

namespace GymManagement.Api.Infrastructure.Tenant
{
    // Simple tenant provider that reads a 'gym_id' claim first, then a header 'X-Gym-Id'.
    // Returns Guid.Empty when none found (DbContext may treat that as admin/no-filter).
    public class HttpContextTenantProvider : ITenantProvider
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public HttpContextTenantProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid GymId
        {
            get
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx == null) return Guid.Empty;

                // Try JWT claim
                var claimValue = ctx.User?.FindFirst(CustomClaims.GymId)?.Value;

                if (!string.IsNullOrWhiteSpace(claimValue) && Guid.TryParse(claimValue, out var gymIdFromClaim))
                {

                    return gymIdFromClaim;
                }

                //fallback to header
                if (ctx.Request?.Headers != null && ctx.Request.Headers.TryGetValue("X-Gym-Id", out var headerValue))
                {
                    var header = headerValue.FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var gymIdFromHeader))
                        return gymIdFromHeader;

                }

                // No gym id -> return empty (DbContext treats as admin/no-filter)
                return Guid.Empty;
            }
        }
    }
}
