using CoreBanking.API.Models;

namespace CoreBanking.UnitTests
{
    public class ServiceResultTests
    {
        [Fact]
        public void Success_SetsCorrectProperties()
        {
            var r = ServiceResult<string>.Success("hello");
            Assert.True(r.IsSuccess);
            Assert.Equal("hello", r.Value);
            Assert.Null(r.Error);
            Assert.Equal(ErrorType.None, r.ErrorType);
        }

        [Fact]
        public void NotFound_SetsCorrectProperties()
        {
            var r = ServiceResult<string>.NotFound("not here");
            Assert.False(r.IsSuccess);
            Assert.Null(r.Value);
            Assert.Equal("not here", r.Error);
            Assert.Equal(ErrorType.NotFound, r.ErrorType);
        }

        [Fact]
        public void BadRequest_SetsCorrectProperties()
        {
            var r = ServiceResult<string>.BadRequest("invalid");
            Assert.False(r.IsSuccess);
            Assert.Equal("invalid", r.Error);
            Assert.Equal(ErrorType.BadRequest, r.ErrorType);
        }

        [Fact]
        public void Conflict_SetsCorrectProperties()
        {
            var r = ServiceResult<int>.Conflict("concurrent");
            Assert.False(r.IsSuccess);
            Assert.Equal("concurrent", r.Error);
            Assert.Equal(ErrorType.Conflict, r.ErrorType);
        }

        [Fact]
        public void Success_WithComplexType_PreservesValue()
        {
            var obj = new { Id = 42, Name = "test" };
            var r = ServiceResult<object>.Success(obj);
            Assert.True(r.IsSuccess);
            Assert.Same(obj, r.Value);
        }
    }

    public class PaginationResponseTests
    {
        [Fact]
        public void PageCount_ExactDivision_ReturnsCorrect()
        {
            var p = new PaginationResponse<int> { TotalItems = 20, PageSize = 5 };
            Assert.Equal(4, p.PageCount);
        }

        [Fact]
        public void PageCount_WithRemainder_RoundsUp()
        {
            var p = new PaginationResponse<int> { TotalItems = 21, PageSize = 5 };
            Assert.Equal(5, p.PageCount);
        }

        [Fact]
        public void PageCount_SingleItem_ReturnsOne()
        {
            var p = new PaginationResponse<int> { TotalItems = 1, PageSize = 10 };
            Assert.Equal(1, p.PageCount);
        }

        [Fact]
        public void PageCount_ZeroItems_ReturnsZero()
        {
            var p = new PaginationResponse<int> { TotalItems = 0, PageSize = 10 };
            Assert.Equal(0, p.PageCount);
        }

        [Fact]
        public void PageCount_ItemsEqualPageSize_ReturnsOne()
        {
            var p = new PaginationResponse<int> { TotalItems = 10, PageSize = 10 };
            Assert.Equal(1, p.PageCount);
        }

        [Fact]
        public void PageCount_ItemsOneLessThanPageSize_ReturnsOne()
        {
            var p = new PaginationResponse<int> { TotalItems = 9, PageSize = 10 };
            Assert.Equal(1, p.PageCount);
        }

        [Fact]
        public void PageCount_ItemsOneMoreThanPageSize_ReturnsTwo()
        {
            var p = new PaginationResponse<int> { TotalItems = 11, PageSize = 10 };
            Assert.Equal(2, p.PageCount);
        }

        [Fact]
        public void Items_DefaultEmpty()
        {
            var p = new PaginationResponse<string>();
            Assert.Empty(p.Items);
        }
    }
}
