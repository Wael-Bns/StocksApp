using StocksApp.Domain.Entities;
using StocksApp.Domain.Events;
using StocksApp.Domain.Enums;
using StocksApp.Domain.RepositoryContracts;
using StocksApp.Domain.Specifications;
using StocksApp.Core.ServiceContracts;

namespace StocksApp.Core.Services
{
    public class OrdersExecutionService : IOrdersExecutionService
    {
        private readonly IGenericRepository<SellOrder> _sellOrderRepository;
        private readonly IUnitOfWork _unitOfWork;

        public OrdersExecutionService(IGenericRepository<SellOrder> sellOrderRepository, IUnitOfWork unitOfWork)
        {
            _sellOrderRepository = sellOrderRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task ExecuteSellOrdersAsync(IReadOnlyCollection<SellOrderCreatedCommand> orders, CancellationToken cancellationToken)
        {
            var orderIds = orders.Select(o => o.SellOrderId).ToList();

            try
            {
                await _unitOfWork.BeginTransactionAsync(cancellationToken);

                var spec = new SellOrdersByIdsSpecification(orderIds);
                var sellOrders = await _sellOrderRepository.ListAsync(spec);

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