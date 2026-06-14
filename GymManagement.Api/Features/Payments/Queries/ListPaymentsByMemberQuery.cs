using MediatR;
using GymManagement.Api.Features.Payments.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Payments.Queries
{
    public record ListPaymentsByMemberQuery(Guid MemberId) : IRequest<List<PaymentResponse>>;

    public class ListPaymentsByMemberQueryHandler : IRequestHandler<ListPaymentsByMemberQuery, List<PaymentResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListPaymentsByMemberQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<PaymentResponse>> Handle(ListPaymentsByMemberQuery request, CancellationToken cancellationToken)
        {
            var payments = await _db.Payments
                .Where(p => p.MemberId == request.MemberId)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<PaymentResponse>>(payments);
        }
    }
}
