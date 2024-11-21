using Reddit.Models;
using System.Linq.Expressions;
namespace Reddit.Repositories
{
    public class CommunitiesRepository : ICommunitiesRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public CommunitiesRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<PagedList<Community>> GetCommunities(int pageNumber, int pageSize, string? searchKey, string? sortKey = null, bool isAscending = true)
        {
            var communities = _dbContext.Communities.AsQueryable();
            if (searchKey != null)
            {
                communities = communities.Where(c => c.Name.Contains(searchKey) || c.Description.Contains(searchKey));
            }
            if (isAscending)
            {
                communities = communities.OrderBy(GetSortExpression(sortKey));
            }
            else
            {
                communities = communities.OrderByDescending(GetSortExpression(sortKey));
            }
            return await PagedList<Community>.CreateAsync(communities, pageNumber, pageSize);
        }
        private Expression<Func<Community, object>> GetSortExpression(string? sortKey)
        {
            sortKey = sortKey?.ToLower();
            return sortKey switch
            {
                "postsCount" => community => community.Posts.Count,
                "subscribersCount" => community => community.Subscribers.Count,
                "createdAt" => community => community.CreatedAt,
                _ => community => community.Id
            };
        }
    }
}