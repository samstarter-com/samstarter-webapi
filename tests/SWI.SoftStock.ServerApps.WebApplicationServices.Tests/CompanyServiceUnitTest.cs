using Microsoft.EntityFrameworkCore;
using Moq;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationServices;

namespace SWI.SoftStock.WebApi.Tests
{
    public class CompanyServiceUnitTest
    {
        [Fact]
        public void IsCompanyExists_WhenCompanyExist_ShouldReturnTrue()
        {
            // Arrange
            const string CompanyName = "TestCompany";
            var mockDbFactory = new Mock<IDbContextFactory<MainDbContext>>();
            var context = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase("InMemoryTest").Options);
            context.StructureUnits.Add(new StructureUnit() { Name = CompanyName });
            context.SaveChanges();

            mockDbFactory.Setup(f => f.CreateDbContext())
                .Returns(context);

            // Act
            var companyService = new CompanyService(mockDbFactory.Object);
            var isExist = companyService.IsCompanyExists(CompanyName);
            
            // Assert
            Assert.True(isExist);

        }
    }
}