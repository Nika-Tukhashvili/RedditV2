using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using System.Linq.Expressions;

namespace Reddit.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UsersRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedList<User>> GetUsers(int pageNumber, int pageSize, string? searchKey, string? sortKey = null, bool isAscending = true)
        {
            var users = _dbContext.Users.AsQueryable();

            if (searchKey != null)
            {
                users = users.Where(u => u.Name.Contains(searchKey) || u.Email.Contains(searchKey));
            }

            if (isAscending)
            {
                users = users.OrderBy(GetSortExpression(sortKey));
            }
            else
            {
                users = users.OrderByDescending(GetSortExpression(sortKey));
            }

            return await PagedList<User>.CreateAsync(users, pageNumber, pageSize);
        }

        private Expression<Func<User, object>> GetSortExpression(string? sortKey)
        {
            sortKey = sortKey?.ToLower();
            return sortKey switch
            {
                "numberofposts" => user => user.Posts.Count,
                "id" => user => user.Id,
                _ => user => user.Id
            };
        }

    }
}
