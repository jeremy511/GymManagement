using AutoMapper;
using GymManagement.Api.Features.Auth.Commands;
using GymManagement.Api.Features.Auth.Domain;
using GymManagement.Api.Features.Gyms.Domain;
using GymManagement.Api.Features.Gyms.Controllers;
using GymManagement.Api.Features.Classes.Controllers;
using GymManagement.Api.Features.Classes.Domain;
using GymManagement.Api.Features.Members.Controllers;
using GymManagement.Api.Features.Members.Domain;
using GymManagement.Api.Features.Memberships.Controllers;
using GymManagement.Api.Features.Memberships.Domain;
using GymManagement.Api.Features.Payments.Controllers;
using GymManagement.Api.Features.Payments.Domain;

namespace GymManagement.Api.Infrastructure.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Auth
            CreateMap<User, AuthResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.Id));

            // Member -> MemberResponse
            CreateMap<Member, MemberResponse>()
                .ForMember(dest => dest.HasActiveMembership, opt => opt.Ignore());

            // Payment -> PaymentResponse
            CreateMap<Payment, PaymentResponse>();
            CreateMap<Gym, GymResponse>();

            // Classes
            CreateMap<Class, ClassResponse>()
                .ForMember(dest => dest.RegisteredCount, opt => opt.MapFrom(src => src.Reservations.Count));
            CreateMap<Reservation, ReservationResponse>();

            // Memberships
            CreateMap<MembershipType, MembershipTypeResponse>();
            CreateMap<Membership, MembershipResponse>()
                .ForMember(dest => dest.PricePaid, opt => opt.MapFrom(src => src.PricePaid))
                .ForMember(dest => dest.EndAt, opt => opt.MapFrom(src => src.EndDate));
        }
    }
}
