using Microsoft.Extensions.DependencyInjection;
using Moq;
using TaskAspNet.Business.Dtos;
using TaskAspNet.Business.Interfaces;
using TaskAspNet.Business.Services;
using TaskAspNet.Data.Interfaces;

namespace TaskAspNet.Tests
{
    public class MemberServiceTests : BaseTest
    {
        private readonly MemberService _service;
        private readonly Mock<INotificationService> _notificationsMock;

        public MemberServiceTests()
        {
            var memberRepo = ServiceProvider.GetRequiredService<IMemberRepository>();
            _notificationsMock = new Mock<INotificationService>();
            var unitOfWork = ServiceProvider.GetRequiredService<IUnitOfWork>();
            _service = new MemberService(memberRepo, _notificationsMock.Object, unitOfWork);
        }

        [Fact]
        public async Task GetAllMembersAsync_ShouldReturnEmpty_WhenNone()
        {
            var members = await _service.GetAllMembersAsync();
            Assert.Empty(members);
        }

        [Fact]
        public async Task AddMemberAsync_ShouldAddMember_AndNotify()
        {
            var dto = new MemberDto
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                UserId = "user1",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            };

            var result = await _service.AddMemberAsync(dto);

            Assert.Equal("John", result.FirstName);
            Assert.Equal("Doe", result.LastName);
            Assert.Equal("john@example.com", result.Email);
            _notificationsMock.Verify(n => n.NotifyMemberCreatedAsync(
                result.Id, "John Doe", "user1"), Times.Once);
        }

        [Fact]
        public async Task GetMembersByIdAsync_ShouldReturnMember_WhenExists()
        {
            var added = await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@smith.com",
                UserId = "user2",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            var list = await _service.GetMembersByIdAsync(added.Id);
            Assert.Single(list);
            Assert.Equal(added.Id, list.First().Id);
        }

        [Fact]
        public async Task GetMembersByIdAsync_ShouldReturnEmpty_WhenNotExists()
        {
            var list = await _service.GetMembersByIdAsync(999);
            Assert.Empty(list);
        }

        [Fact]
        public async Task UpdateMemberAsync_ShouldModifyMember_AndNotify()
        {
            var added = await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Joe",
                LastName = "Bloggs",
                Email = "joe@bloggs.com",
                UserId = "user3",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            var toUpdate = new MemberDto
            {
                Id = added.Id,
                FirstName = "Joseph",
                LastName = "Bloggs",
                Email = "joseph@bloggs.com",
                UserId = "user3",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            };

            var updated = await _service.UpdateMemberAsync(added.Id, toUpdate);

            Assert.Equal("Joseph", updated.FirstName);
            Assert.Equal("joseph@bloggs.com", updated.Email);
            _notificationsMock.Verify(n => n.NotifyMemberUpdatedAsync(
                updated.Id, "Joseph Bloggs", "user3"), Times.Once);
        }

        [Fact]
        public async Task DeleteMemberAsync_ShouldRemoveMember()
        {
            var added = await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Mark",
                LastName = "Twain",
                Email = "mark@twain.com",
                UserId = "user4",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            var deleted = await _service.DeleteMemberAsync(added.Id);

            Assert.Equal(added.Id, deleted.Id);
            var after = await _service.GetAllMembersAsync();
            Assert.DoesNotContain(after, m => m.Id == added.Id);
        }

        [Fact]
        public async Task SearchMembersAsync_ShouldReturnMatchingMembers()
        {
            await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Alice",
                LastName = "Wonder",
                Email = "alice@wonder.com",
                UserId = "user5",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });
            await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Bob",
                LastName = "Builder",
                Email = "bob@builder.com",
                UserId = "user6",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            // search "Ali"
            var results = await _service.SearchMembersAsync("Ali");
            Assert.Single(results);
            Assert.Contains(results, m => m.FirstName == "Alice");
        }

        [Fact]
        public async Task GetAllJobTitlesAsync_ShouldReturnEmpty_WhenNone()
        {
            var list = await _service.GetAllJobTitlesAsync();
            Assert.Empty(list);
        }

        [Fact]
        public async Task GetMemberByUserIdAsync_ShouldReturnMember_WhenExists()
        {
            var added = await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Sam",
                LastName = "Spade",
                Email = "sam@spade.com",
                UserId = "user7",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            var result = await _service.GetMemberByUserIdAsync("user7");
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
        }

        [Fact]
        public async Task GetMemberByUserIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _service.GetMemberByUserIdAsync("noSuchUser");
            Assert.Null(result);
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnMember_WhenExists()
        {
            var added = await _service.AddMemberAsync(new MemberDto
            {
                FirstName = "Nancy",
                LastName = "Drew",
                Email = "nancy@drew.com",
                UserId = "user8",
                Year = 1990,
                Month = 1,
                Day = 1,
                ImageData = new UploadSelectImgDto { CurrentImage = null }
            });

            var result = await _service.GetMemberByIdAsync(added.Id);
            Assert.NotNull(result);
            Assert.Equal(added.Id, result.Id);
        }

        [Fact]
        public async Task GetMemberByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            var result = await _service.GetMemberByIdAsync(404);
            Assert.Null(result);
        }
    }
}
