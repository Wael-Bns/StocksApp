using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StocksApp.Core.Options;
using StocksApp.Core.ServiceContracts;
using StocksApp.Domain.Entities;
using StocksApp.Domain.Events;
using StocksApp.Domain.RepositoryContracts;

namespace StocksApp.Core.Services
{
    public class OrderMatcher : IOrderMatcher
    {
        private readonly ISellOrderMatchRepository _matchRepository;
        private readonly IGenericRepository<Outbox> _outboxRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOptions<OrderMatchingOptions> _options;
        private readonly ILogger<OrderMatcher> _logger;

        public OrderMatcher(
            ISellOrderMatchRepository matchRepository,
            IGenericRepository<Outbox> outboxRepository,
            IUnitOfWork unitOfWork,
            IOptions<OrderMatchingOptions> options,
            ILogger<OrderMatcher> logger)
        {
            _matchRepository = matchRepository;
            _outboxRepository = outboxRepository;
            _unitOfWork = unitOfWork;
            _options = options;
            _logger = logger;
        }

        public async Task RunOnceAsync(CancellationToken ct)
        {
            await _unitOfWork.BeginTransactionAsync(ct);
            try
            {
                var orderIds = await _matchRepository.GetMatchableOrderIdsAsync(_options.Value.MatcherGracePeriod, ct);
                if (orderIds.Count == 0)
                {
                    await _unitOfWork.CommitTransactionAsync(ct);
                    return;
                }

                var orders = await _matchRepository.ListTrackedAsync(orderIds, ct);
                foreach (var order in orders)
                {
                    order.MarkExecuted();
                    await _matchRepository.CreditCashAsync(order.UserId, order.Price * order.Quantity, ct);
                    await _outboxRepository.AddAsync(SellOrderExecuted.From(order).ToOutbox());
                }

                await _unitOfWork.SaveChangesAsync(ct);
                await _unitOfWork.CommitTransactionAsync(ct);

                _logger.LogInformation("Matched and executed {Count} sell orders.", orders.Count);
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                throw;
            }
        }
    }
}