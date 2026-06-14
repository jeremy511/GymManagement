using MediatR;
using GymManagement.Api.Features.Payments.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace GymManagement.Api.Features.Payments.Queries
{
    public record ListPaymentsQuery() : IRequest<List<PaymentResponse>>;

    public class ListPaymentsQueryHandler : IRequestHandler<ListPaymentsQuery, List<PaymentResponse>>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public ListPaymentsQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<List<PaymentResponse>> Handle(ListPaymentsQuery request, CancellationToken cancellationToken)
        {
            var payments = await _db.Payments.ToListAsync(cancellationToken);
            return _mapper.Map<List<PaymentResponse>>(payments);
        }
    }
}
