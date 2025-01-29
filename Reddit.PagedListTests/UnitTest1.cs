using Microsoft.EntityFrameworkCore;
using Reddit.Models;
using Reddit.Repositories;

namespace Reddit.PagedListTests
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
            // Seed the database with test data
            dbContext.Posts.Add(new Post { Title = "Title 1", Content = "Content 1", Upvote = 5, Downvote = 1 });
            dbContext.Posts.Add(new Post { Title = "Title 2", Content = "Content 2", Upvote = 12, Downvote = 1 });
            dbContext.Posts.Add(new Post { Title = "Title 3", Content = "Content 3", Upvote = 3, Downvote = 1 });
            dbContext.Posts.Add(new Post { Title = "Title 4", Content = "Content 4", Upvote = 221, Downvote = 1 });
            dbContext.Posts.Add(new Post { Title = "Title 5", Content = "Content 5", Upvote = 5, Downvote = 2123 });
            dbContext.SaveChanges();
            return dbContext;
        }
        [Fact]
        public async Task CreateAsync_ReturnsCorrectPageSize()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 1;
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);
            // Assert
            Assert.Equal(pageSize, result.Items.Count);
            Assert.Equal(pageSize, result.PageSize);
        }
        [Fact]
        public async Task CreateAsync_ReturnsCorrectItems()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 2;
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);
            // Assert
            Assert.Equal(2, result.Items.Count);
            Assert.Equal("Title 3", result.Items[0].Title);
            Assert.Equal("Title 4", result.Items[1].Title);
        }
        [Fact]
        public async Task CreateAsync_LastPage_ReturnsCorrectItemCount()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 3;
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);
            // Assert
            Assert.Single(result.Items);
            Assert.Equal("Title 5", result.Items[0].Title);
        }
        [Fact]
        public async Task CreateAsync_HasNextPage_ReturnsCorrectValue()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            // Act - First page
            var firstPage = await PagedList<Post>.CreateAsync(queryable, 1, 2);
            // Act - Last page
            var lastPage = await PagedList<Post>.CreateAsync(queryable, 3, 2);
            // Assert
            Assert.True(firstPage.HasNextPage);
            Assert.False(lastPage.HasNextPage);
        }
        [Fact]
        public async Task CreateAsync_HasPreviousPage_ReturnsCorrectValue()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            // Act - First page
            var firstPage = await PagedList<Post>.CreateAsync(queryable, 1, 2);
            // Act - Middle page
            var middlePage = await PagedList<Post>.CreateAsync(queryable, 2, 2);
            // Assert
            Assert.False(firstPage.HasPreviousPage);
            Assert.True(middlePage.HasPreviousPage);
        }
        [Fact]
        public async Task CreateAsync_TotalCount_ReturnsCorrectValue()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, 1, 2);
            // Assert
            Assert.Equal(5, result.TotalCount); // Total number of seeded posts
        }
        [Fact]
        public async Task CreateAsync_EmptyDatabase_ReturnsEmptyList()
        {
            // Arrange
            var dbName = Guid.NewGuid().ToString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            using var context = new ApplicationDbContext(options);
            var queryable = context.Posts.OrderBy(p => p.Title);
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, 1, 5);
            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
            Assert.False(result.HasNextPage);
            Assert.False(result.HasPreviousPage);
        }
        [Fact]
        public async Task CreateAsync_PageNumberGreaterThanTotalPages_ReturnsEmptyList()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageNumber = 5;
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);
            // Assert
            Assert.Empty(result.Items);
            Assert.Equal(5, result.TotalCount);
            Assert.False(result.HasNextPage);
            Assert.True(result.HasPreviousPage);
        }
        [Fact]
        public async Task CreateAsync_SetsCorrectPageNumber()
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var expectedPageNumber = 2;
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, expectedPageNumber, pageSize);
            // Assert
            Assert.Equal(expectedPageNumber, result.PageNumber);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public async Task CreateAsync_DifferentPages_SetsCorrectPageNumber(int pageNumber)
        {
            // Arrange
            using var context = GetDbContext();
            var queryable = context.Posts.OrderBy(p => p.Title);
            var pageSize = 2;
            // Act
            var result = await PagedList<Post>.CreateAsync(queryable, pageNumber, pageSize);
            // Assert
            Assert.Equal(pageNumber, result.PageNumber);
        }
    }
}