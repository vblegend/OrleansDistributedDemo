using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Orleans.Concurrency;
using Orleans.Providers;
using Orleans.Runtime;
using OrleansDistributedDemo.Interfaces;

namespace OrleansDistributedDemo.Silo
{
    public class PlayerState
    {
        public long Level { get; set; }
    }


    /// <summary>
    /// Implementation of the counter grain that maintains its state across different silos
    /// </summary>
    //[PlacementStrategy(typeof(HashBasedPlacement))] // ���� Grain ID �Ĺ�ϣ�ֲ���Orleans �Զ����⡣
    //[PlacementStrategy(typeof(RandomPlacement))] // RandomPlacement������ֲ���
    //[PlacementStrategy(typeof(PreferLocalPlacement))] // PreferLocalPlacement�����ȱ��� Silo��
    // [StatelessWorker] // �� Orleans �� ��� Silo ��ͬʱ������ Grains�������� �߲����������� API Gateway����

    [StorageProvider(ProviderName = "Redis")]
    public class CounterGrain : Grain<PlayerState>, ICounterGrain
    {
        private readonly ILogger<CounterGrain> _logger;
        private readonly IGrainContext _grainContext;

        public CounterGrain(ILogger<CounterGrain> logger, IGrainContext grainContext)
        {
            _logger = logger;
            _grainContext = grainContext;
        }

        public override Task OnActivateAsync(CancellationToken cancellationToken)
        {
            return base.OnActivateAsync(cancellationToken);
        }



        public async Task<long> IncrementAsync(long increment)
        {
            var grainKey = this.IdentityString;
            State.Level += increment;
            _logger.LogInformation("Grain {GrainKey}: Counter incremented by {Increment}. New value: {Value}",  grainKey, increment, State.Level);
            await WriteStateAsync();
            return State.Level;
        }

        public Task<long> GetCountAsync()
        {
            return Task.FromResult(State.Level);
        }

        public Task ResetAsync()
        {
            var oldValue = State.Level;
            State.Level = 0;
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