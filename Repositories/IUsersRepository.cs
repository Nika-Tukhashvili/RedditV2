using Reddit.Models;

namespace Reddit.Repositories
{
    public interface IUsersRepository
    {
        public Task<PagedList<User>> GetUsers(int pageNumber, int pageSize, string? searchKey, string? sortKey = null, bool isAscending = true);
    }
}
