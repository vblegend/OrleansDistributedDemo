using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Runtime;
using OrleansDistributedDemo.Interfaces;

namespace OrleansDistributedDemo.Silo
{
    /// <summary>
    /// Implementation of the counter grain that maintains its state across different silos
    /// </summary>
    //[PlacementStrategy(typeof(HashBasedPlacement))] // 基于 Grain ID 的哈希分布，Orleans 自动均衡。
    //[PlacementStrategy(typeof(RandomPlacement))] // RandomPlacement（随机分布）
    //[PlacementStrategy(typeof(PreferLocalPlacement))] // PreferLocalPlacement（优先本地 Silo）
    // [StatelessWorker] // 让 Orleans 在 多个 Silo 中同时激活多个 Grains，适用于 高并发任务处理（如 API Gateway）。
    public class CounterGrain : Grain, ICounterGrain
    {
        private readonly ILogger<CounterGrain> _logger;
        private long _currentValue;
        private readonly IGrainContext _grainContext;

        public CounterGrain(ILogger<CounterGrain> logger, IGrainContext grainContext)
        {
            _logger = logger;
            _grainContext = grainContext;
            _currentValue = 0;
        }

        public Task<long> IncrementAsync(long increment)
        {
            var grainKey = this.IdentityString;
            _currentValue += increment;
            _logger.LogInformation("Grain {GrainKey}: Counter incremented by {Increment}. New value: {Value}", 
                grainKey, increment, _currentValue);
            
            return Task.FromResult(_currentValue);
        }

        public Task<long> GetCountAsync()
        {
            return Task.FromResult(_currentValue);
        }

        public Task ResetAsync()
        {
            var oldValue = _currentValue;
            _currentValue = 0;
            _logger.LogInformation("Grain {GrainKey}: Counter reset from {OldValue} to 0",
                this.IdentityString, oldValue);
            
            return Task.CompletedTask;
        }

        public Task<string> GetHostInfoAsync()
        {
            var siloAddress = _grainContext.Address.SiloAddress;
            var siloName = _grainContext.Address.GrainId;
            
            return Task.FromResult($"Silo: {siloName}, Address: {siloAddress}, SiloGeneration: {siloAddress.Generation}");
        }
    }
}