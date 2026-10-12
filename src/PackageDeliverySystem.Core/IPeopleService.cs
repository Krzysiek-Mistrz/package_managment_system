namespace PackageDeliverySystem.Core;

public interface IPeopleService
{
    Task<User?> GetUserAsync(int userId);
    Task<Courier?> GetCourierAsync(int courierId);
    Task<Operator?> GetOperatorAsync(int operatorId);
}
