using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using SWI.SoftStock.ServerApps.DataModel2;
using SWI.SoftStock.ServerApps.WebApplicationContracts.CompanyService.Add;
using SWI.SoftStock.ServerApps.WebApplicationModel;
using SWI.SoftStock.ServerApps.WebApplicationServices;

namespace SWI.SoftStock.WebApi.Tests
{
    public class CompanyServiceUnitTest
    {
        private readonly MainDbContext context;
        private CompanyService companyService;

        public CompanyServiceUnitTest()
        {
            var mockDbFactory = new Mock<IDbContextFactory<MainDbContext>>();
            context = new MainDbContext(new DbContextOptionsBuilder<MainDbContext>()
                .UseInMemoryDatabase("InMemoryTest").Options);
            mockDbFactory.Setup(f => f.CreateDbContext())
                .Returns(context);
            companyService = new CompanyService(mockDbFactory.Object);
        }

        [Fact]
        public async Task Add_WhenCompanyExist_ShouldReturnNonUnique()
        {
            // Arrange
            const string CompanyName = "TestCompany";
            string AnotherCompanyName = CompanyName.ToLower();
            context.StructureUnits.Add(new StructureUnit() { Name = CompanyName });
            context.SaveChanges();

            var model = new StructureUnitModel()
            {
                IsRootUnit = true,
                Name = AnotherCompanyName,
                ParentUniqueId = null,
                ShortName = "test",
                UniqueId = Guid.NewGuid()
            };

            // Act
            var result = await companyService.Add(model);

            // Assert
            Assert.Equal(CompanyCreationStatus.NonUnique, result.Status);
        }

        [Fact]
        public async Task Add_WhenCompanyNotExist_ShouldReturnSuccess()
        {
            // Arrange
            const string CompanyName = "TestCompany";
            const string AnotherCompanyName = "TestCompany1";
            context.StructureUnits.Add(new StructureUnit() { Name = CompanyName });
            context.SaveChanges();

            var model = new StructureUnitModel()
            {
                IsRootUnit = true,
                Name = AnotherCompanyName,
                ParentUniqueId = null,
                ShortName = "test",
                UniqueId = Guid.NewGuid()
            };

            // Act
            var result = await companyService.Add(model);

            // Assert
            Assert.Equal(CompanyCreationStatus.Success, result.Status);
        }

        [Fact]
        public void IsCompanyExists_WhenCompanyExist_ShouldReturnTrue()
        {
            // Arrange
            const string CompanyName = "TestCompany";
            context.StructureUnits.Add(new StructureUnit() { Name = CompanyName });
            context.SaveChanges();

            // Act
            var isExist = companyService.IsCompanyExists(CompanyName);

            // Assert
            Assert.True(isExist);

        }
    }
}