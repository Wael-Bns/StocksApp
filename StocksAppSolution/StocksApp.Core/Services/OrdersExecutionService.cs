using StocksApp.Domain.Events;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Core.Services
{
    public class OrdersExecutionService : IOrdersExecutionService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IUnitOfWork _unitOfWork;
        public OrdersExecutionService(IOrderRepository orderRepository, IUnitOfWork unitOfWork)
        {
            _orderRepository = orderRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task ExecuteSellOrdersAsync(IReadOnlyCollection<SellOrderCreatedCommand> orders, CancellationToken cancellationToken)
        {
            List<Guid> orderIds = orders
                            .Select(o => o.SellOrderId)
                            .ToList();

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);
                var sellOrders = await _orderRepository.GetSellOrdersByIds(orderIds);

                foreach (var sellOrder in sellOrders)
                {
                    sellOrder.Status = SellOrderStatus.Executed;
                    sellOrder.User.CashBalance += sellOrder.Price * sellOrder.Quantity;
                }
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw new Exception("An error occurred while executing sell orders.", ex);
            }
        }
    }
}
