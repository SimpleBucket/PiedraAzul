namespace PiedraAzul.GraphQL.Types;

public class UserListResultType
{
    public List<UserType> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}
