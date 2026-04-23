using System.Threading.Tasks;
using StocksApp.Core.Domain.Entities;

namespace StocksApp.Core.ServiceContracts
{
    /// <summary>
    /// Contains methods for executing trade operations 
    /// </summary>
    public interface ITradeExecutionService
    {
        /// <summary>
        /// Executes a sell order making necessary changes
        /// </summary>
        /// <param name="incomingOrder"></param>
        /// <returns></returns>
        Task FinalizeSellOrderAsync(SellOrder incomingOrder);
    }
}