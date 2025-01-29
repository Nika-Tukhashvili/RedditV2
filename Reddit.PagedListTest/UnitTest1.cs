using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using Reddit.Repositories;

namespace Reddit.PagedListTest
{
    public class UnitTest1
    {
        private ApplicationDbContext GetDbContext()
        {
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            var dbContext = new ApplicationDbContext(options);

            dbContext.Posts.Add(new Post { Title = "Test Title 1", Content = "Content 1" });
            dbContext.Posts.Add(new Post { Title = "Test Title 2", Content = "Content 1" });
            dbContext.Posts.Add(new Post { Title = "Test Title 3", Content = "Content 1" });
            dbContext.Posts.Add(new Post { Title = "Test Title 4", Content = "Content 1" });
            dbContext.Posts.Add(new Post { Title = "Test Title 5", Content = "Content 1" });
            dbContext.SaveChanges();
            return dbContext;
        }
        [Fact]
        public async Task CreateAsync_ReturnsCorrectPageSize()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 1;
            var pageSize = 2;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.Equal(pageSize, result.Items.Count);
            Assert.Equal(pageSize, result.PageSize);
        }
        [Fact]
        public async Task CreateAsync_ReturnsCorrectItems()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 1;
            var pageSize = 3;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.Equal(3, result.Items.Count);
            Assert.Equal("Test Title 1", result.Items[0].Title);
            Assert.Equal("Test Title 2", result.Items[1].Title);
            Assert.Equal("Test Title 3", result.Items[2].Title);
        }
        [Fact]
        public async Task CreateAsync_LastPage_ReturnsCorrectItemCount()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 2;
            var pageSize = 4;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.Equal("Test Title 5", result.Items[0].Title);
            Assert.Single(result.Items);

        }
        [Fact]
        public async Task CreateAsync_HasNextPage_ReturnsCorrectValue()
        {
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var firstPage = await PagedList<Post>.CreateAsync(queryable, 1, 2);

            var lastPage = await PagedList<Post>.CreateAsync(queryable, 3, 2);

            Assert.True(firstPage.HasNextPage);
            Assert.False(lastPage.HasNextPage);
        }
        [Fact]
        public async Task CreateAsync_HasPreviousPage_ReturnsCorrectValue()
        {
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var firstPage = await PagedList<Post>.CreateAsync(queryable, 1, 4);

            var middlePage = await PagedList<Post>.CreateAsync(queryable, 2, 2);

            Assert.False(firstPage.HasPreviousPage);
            Assert.True(middlePage.HasPreviousPage);
        }


        [Fact]
        public async Task CreateAsync_PageNumberGreaterThanTotalPages_ReturnsEmptyList()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 5;
            var pageSize = 2;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
            Assert.False(result.HasNextPage);
        }



        [Fact]
        public async Task CreateAsync_PageSizeGreaterThanTotalItems_ReturnsCorrectList()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 1;
            var pageSize = 10;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.True(pageSize > result.TotalCount);
            Assert.Equal(5, result.TotalCount);
            Assert.Equal(pageSize, result.PageSize);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }

        [Fact]
        public async Task CreateAsync_TotalItemsGreaterThanPageSize_ReturnsCorrectList()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 1;
            var pageSize = 3;

            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);

            Assert.True(result.TotalCount > pageSize);
            Assert.Equal(5, result.TotalCount);
            Assert.True(result.HasNextPage);
        }



        [Fact]
        public async Task CreateAsync_TotalCount_ReturnsCorrectValue()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);

            var result = await PagedList<Post>.CreateAsync(queryable, 1, 2);

            Assert.Equal(5, result.TotalCount);
        }
        [Fact]
        public async Task CreateAsync_EmptyDatabase_ReturnsEmptyList()
        {

            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            using var context = new ApplicationDbContext(options);
            var queryable = context.Posts.OrderBy(p => p.Title);

            var result = await PagedList<Post>.CreateAsync(queryable, 1, 5);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }




        [Fact]
        public async Task CreateAsync_SetsCorrectPageNumber()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var expectedPageNumber = 2;
            var pageSize = 2;

            var result = await PagedList<Post>.CreateAsync(queryable, expectedPageNumber, pageSize);

            Assert.Equal(expectedPageNumber, result.PageNumber);
        }


        [Fact]
        public async Task CreateAsync_PageNumberNegative_ThrowsException()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = -2;
            var pageSize = 2;

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize));

            Assert.Equal("pageNumber", exception.ParamName);

        }

        [Fact]
        public async Task CreateAsync_PageSizeNegative_ThrowsException()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 2;
            var pageSize = -2;

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize));

            Assert.Equal("pageSize", exception.ParamName);

        }

        [Fact]
        public async Task CreateAsync_PageSizeMoreThan50_ThrowsException()
        {

            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 2;
            var pageSize = 100;

            var exception = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize));

            Assert.Equal("pageSize", exception.ParamName);

        }

    }
}