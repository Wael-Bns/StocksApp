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
        /// <summary>
        /// Marks a sell order as cancelled in the database
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="gracePeriod"></param>
        /// <param name="ct"></param>
        /// <returns>A boolean value equal to true if the order is marked executed and false if not.</returns>
        Task<bool> TryCancelAsync(Guid orderId, TimeSpan gracePeriod, CancellationToken ct);
    }
}
