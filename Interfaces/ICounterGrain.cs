using Orleans;
using System.Threading.Tasks;

namespace OrleansDistributedDemo.Interfaces
{
    /// <summary>
    /// Interface for a distributed counter grain
    /// </summary>
    public interface ICounterGrain : IGrainWithStringKey
    {
        /// <summary>
        /// Increment counter by the specified value
        /// </summary>
        /// <param name="increment">Value to add to the counter</param>
        /// <returns>The new counter value</returns>
        Task<long> IncrementAsync(long increment);
        
        /// <summary>
        /// Get the current counter value
        /// </summary>
        /// <returns>The current value of the counter</returns>
        Task<long> GetCountAsync();
        
        /// <summary>
        /// Reset the counter to zero
        /// </summary>
        /// <returns>Task representing the operation</returns>
        Task ResetAsync();
        
        /// <summary>
        /// Get information about the silo hosting this grain
        /// </summary>
        /// <returns>String containing silo information</returns>
        Task<string> GetHostInfoAsync();
    }
}