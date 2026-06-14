namespace GymManagement.Api.Infrastructure.Persistence;

public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}
