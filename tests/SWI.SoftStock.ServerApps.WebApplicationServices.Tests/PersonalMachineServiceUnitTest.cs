using Microsoft.EntityFrameworkCore;
using Moq;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts;
using SWI.SoftStock.ServerApps.WebApplicationContracts.PersonalMachineService.GetByUserId;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationServices;
using System.Net.NetworkInformation;

namespace SWI.SoftStock.WebApi.Tests
{
    public class PersonalMachineServiceUnitTest
    {
        private readonly MainDbContext context;
        private readonly IPersonalMachineService personalMachineService;

        public PersonalMachineServiceUnitTest()
        {
            var mockDbFactory = new Mock<IDbContextFactory<MainDbContext>>();
            context = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase("InMemoryTest").Options);
            mockDbFactory.Setup(f => f.CreateDbContext())
                .Returns(context);
            personalMachineService = new PersonalMachineService(mockDbFactory.Object);
        }

        [Fact]
        public async Task GetByUserIdAsync_WhenUserHasMAchines_ShouldReturn()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var machineName = "TestMachine";
            context.Machines.Add(new Machine() { Id = 123, Name = machineName, CurrentUserId = userId, MachineSoftwaresReadOnly = new MachineSoftwaresReadOnly() });
            context.Machines.Add(new Machine() { Id = 124, Name = machineName, CurrentUserId = Guid.NewGuid(), MachineSoftwaresReadOnly = new MachineSoftwaresReadOnly() });
            context.SaveChanges();          
            var request = new GetByUserIdRequest
            {
                UserId = userId,
                Ordering = MapperFromViewToModel.MapToOrdering(new OrderingModel()),
                Paging = MapperFromViewToModel.MapToPaging(new PagingModel() { PageIndex = 0, PageSize = int.MaxValue })
            };

            // Act
            var result = await personalMachineService.GetByUserIdAsync(request);           
            // Assert
            Assert.Single(result.Model.Items);
            Assert.Equal(result.Model.Items.First().Name, machineName);
        }
    }
}