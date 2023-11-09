using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Npgsql;
using RestAPI_OCMS.Controllers;
using RestAPI_OCMS.Models;

namespace RestAPI_OCMS.Tests
{
    [TestClass]
    public class OCMS_ControllerTests
    {
        [TestMethod]
        public void GetInventoryById_ReturnsCorrectResponse()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfigurationWrapper>();
            var mockDBApplication = new Mock<IDBApplication>();
            var expectedResponse = new Response();

            mockConfiguration.Setup(config => config.GetConnectionString(It.IsAny<string>())).Returns("Host=localhost;Port=5432;Database=optic;Username=postgres;Password=133080;");
            mockDBApplication.Setup(db => db.GetInventoryById(It.IsAny<NpgsqlConnection>(), It.IsAny<int>())).Returns(expectedResponse);

            var controller = new OCMS_Controller(mockConfiguration.Object, mockDBApplication.Object);

            // Act
            var result = controller.GetInventoryById(1);

            // Assert
            Assert.AreEqual(expectedResponse, result);
        }
    }
}
