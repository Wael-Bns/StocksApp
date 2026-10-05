using StocksApp.Domain.Entities;

namespace StocksApp.Domain.RepositoryContracts
{
    public interface ISellOrderMatchRepository
    {
        /// <summary>
        /// Returns the Sell order ids that need to get matched .
        /// </summary>
        /// <param name="gracePeriod">The amount of time after which it is assured that a candle is written comfortably</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<Guid>> GetMatchableOrderIdsAsync(TimeSpan gracePeriod, CancellationToken ct);
        /// <summary>
        /// Returns the list of sell orders that match the given ids, including the user of each order
        /// </summary>
        /// <param name="orderIds">The ids of the orders to fetch</param>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<IReadOnlyList<SellOrder>> ListTrackedAsync(IReadOnlyList<Guid> orderIds, CancellationToken ct);
    }
}
