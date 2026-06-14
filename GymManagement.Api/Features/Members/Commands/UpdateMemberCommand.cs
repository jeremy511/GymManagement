using MediatR;
using GymManagement.Api.Infrastructure.Common;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymManagement.Api.Features.Members.Commands
{
    public record UpdateMemberCommand(
        Guid MemberId,
        [Required][StringLength(100)] string FirstName,
        [Required][StringLength(100)] string LastName,
        [Required][EmailAddress] string Email) : IRequest<Result<Member>>;

    public class UpdateMemberCommandHandler : IRequestHandler<UpdateMemberCommand, Result<Member>>
    {
        private readonly GymManagementDbContext _db;

        public UpdateMemberCommandHandler(GymManagementDbContext db)
        {
            _db = db;
        }

        public async Task<Result<Member>> Handle(UpdateMemberCommand req, CancellationToken cancellationToken)
        {
            var member = await _db.Members
                .FirstOrDefaultAsync(m => m.Id == req.MemberId, cancellationToken);

            if (member == null)
                return Result<Member>.Failure("Member not found.", "NOT_FOUND");

            member.Update(req.FirstName, req.LastName, req.Email);
            await _db.SaveChangesAsync(cancellationToken);

            return Result<Member>.Success(member);
        }
    }
}
