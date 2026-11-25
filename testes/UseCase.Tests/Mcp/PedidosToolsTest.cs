using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetMonthlyRevenue;
using ChatIA.Communication.Responses;
using ChatIA.McpServer.Tools;
using Moq;

namespace UseCase.Tests.Mcp
{
    /// <summary>
    /// Testa as tools MCP isoladas (sem protocolo stdio), com use cases mockados.
    /// </summary>
    public class PedidosToolsTest
    {
        [Fact]
        public async Task GetPedidosCount_Should_Return_Formatted_Text()
        {
            var tools = CreateTools(count: 2);

            var result = await tools.GetPedidosCount();

            Assert.Contains("2", result);
        }

        [Fact]
        public async Task GetLatestPedidos_Should_Return_Formatted_List()
        {
            var tools = CreateTools(
                latest: new ResponseLatestPedidosJson
                {
                    Pedidos =
                    [
                        new ResponsePedidoShortJson
                        {
                            ShortReference = "48",
                            CustomerName = "mateus lima",
                            Total = 7.5m,
                            SalesChannel = "anotaai",
                            Type = "TAKE",
                            CreatedAt = new DateTime(2025, 9, 25, 22, 48, 54, DateTimeKind.Utc)
                        }
                    ]
                });

            var result = await tools.GetLatestPedidos(10);

            Assert.Contains("mateus lima", result);
            Assert.Contains("#48", result);
        }

        [Fact]
        public async Task GetMonthlyRevenue_Should_Return_Formatted_Revenue()
        {
            var tools = CreateTools(revenue: new ResponseMonthlyRevenueJson
            {
                Year = 2025,
                Month = 9,
                Revenue = 15m
            });

            var result = await tools.GetMonthlyRevenue(2025, 9);

            Assert.Contains("09/2025", result);
            Assert.Contains("15.00", result);
        }

        private static PedidosTools CreateTools(
            long count = 0,
            ResponseLatestPedidosJson? latest = null,
            ResponseMonthlyRevenueJson? revenue = null)
        {
            var countUseCase = new Mock<IGetPedidosCountUseCase>();
            countUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ResponsePedidosCountJson { Count = count });

            var latestUseCase = new Mock<IGetLatestPedidosUseCase>();
            latestUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(latest ?? new ResponseLatestPedidosJson());

            var revenueUseCase = new Mock<IGetMonthlyRevenueUseCase>();
            revenueUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(revenue ?? new ResponseMonthlyRevenueJson());

            return new PedidosTools(countUseCase.Object, latestUseCase.Object, revenueUseCase.Object);
        }
    }
}
