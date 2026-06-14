using MediatR;
using GymManagement.Api.Features.Payments.Controllers;
using GymManagement.Api.Infrastructure.Persistence;
using AutoMapper;

namespace GymManagement.Api.Features.Payments.Queries
{
    public record GetPaymentQuery(Guid Id) : IRequest<PaymentResponse?>;

    public class GetPaymentQueryHandler : IRequestHandler<GetPaymentQuery, PaymentResponse?>
    {
        private readonly GymManagementDbContext _db;
        private readonly IMapper _mapper;

        public GetPaymentQueryHandler(GymManagementDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PaymentResponse?> Handle(GetPaymentQuery request, CancellationToken cancellationToken)
        {
            var payment = await _db.Payments.FindAsync(new object[] { request.Id }, cancellationToken);
            if (payment == null) return null;

            return _mapper.Map<PaymentResponse>(payment);
        }
    }
}
