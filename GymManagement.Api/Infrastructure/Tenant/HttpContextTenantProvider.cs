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

                // Try claim
                var claim = ctx.User?.FindFirst(CustomClaims.GymId)?.Value;


                if (!Guid.TryParse(claim, out var gymIdFromClaim))
                    throw new UnauthorizedAccessException("Gym ID claim is missing or invalid.");

                return GymId;
            }
        }
    }
}
