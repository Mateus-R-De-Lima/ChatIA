using ChatIA.Application.UseCases.Pedidos.GetByShortReference;
using ChatIA.Application.UseCases.Pedidos.GetCount;
using ChatIA.Application.UseCases.Pedidos.GetLatest;
using ChatIA.Application.UseCases.Pedidos.GetLatestDetail;
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
        public async Task GetPedidoByReference_Should_Return_Full_Details()
        {
            var tools = CreateTools(
                detail: new ResponsePedidoDetailJson
                {
                    ShortReference = "12",
                    CustomerName = "Ana",
                    CustomerPhone = "11999999999",
                    Total = 42m,
                    Type = "TAKE",
                    SalesChannel = "anotaai",
                    CreatedAt = new DateTime(2025, 9, 1, 12, 0, 0, DateTimeKind.Utc),
                    Items =
                    [
                        new ResponsePedidoItemJson
                        {
                            Name = "X-Burger Bacon",
                            Quantity = 2,
                            Price = 21m,
                            Total = 42m,
                            Observation = "sem cebola"
                        }
                    ],
                    Payments =
                    [
                        new ResponsePedidoPaymentJson
                        {
                            Name = "PIX",
                            Code = "pix",
                            Value = 42m,
                            Prepaid = true
                        }
                    ]
                });

            var result = await tools.GetPedidoByReference(12);

            Assert.Contains("#12", result);
            Assert.Contains("Ana", result);
            Assert.Contains("X-Burger Bacon", result);
            Assert.Contains("sem cebola", result);
            Assert.Contains("PIX", result);
            Assert.Contains("marca", result, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task GetLatestPedidoDetail_Should_Return_Full_Details()
        {
            var tools = CreateTools(
                detail: new ResponsePedidoDetailJson
                {
                    ShortReference = "99",
                    CustomerName = "João",
                    Total = 10m,
                    Type = "DELIVERY",
                    SalesChannel = "ifood",
                    CreatedAt = DateTime.UtcNow,
                    Items =
                    [
                        new ResponsePedidoItemJson
                        {
                            Name = "Coca 350ml",
                            Quantity = 1,
                            Price = 10m,
                            Total = 10m
                        }
                    ]
                });

            var result = await tools.GetLatestPedidoDetail();

            Assert.Contains("#99", result);
            Assert.Contains("Coca 350ml", result);
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
            ResponsePedidoDetailJson? detail = null,
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

            var latestDetailUseCase = new Mock<IGetLatestPedidoDetailUseCase>();
            latestDetailUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<CancellationToken>()))
                .ReturnsAsync(detail);

            var byRefUseCase = new Mock<IGetPedidoByShortReferenceUseCase>();
            byRefUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(detail);

            var revenueUseCase = new Mock<IGetMonthlyRevenueUseCase>();
            revenueUseCase
                .Setup(useCase => useCase.Execute(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(revenue ?? new ResponseMonthlyRevenueJson());

            return new PedidosTools(
                countUseCase.Object,
                latestUseCase.Object,
                latestDetailUseCase.Object,
                byRefUseCase.Object,
                revenueUseCase.Object);
        }
    }
}
