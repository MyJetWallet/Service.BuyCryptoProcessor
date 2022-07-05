using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.Service;
using MyJetWallet.Sdk.ServiceBus;
using Service.BuyCryptoProcessor.Jobs;

namespace Service.BuyCryptoProcessor
{
    public class ApplicationLifetimeManager : ApplicationLifetimeManagerBase
    {
        private readonly ILogger<ApplicationLifetimeManager> _logger;
        private readonly ServiceBusLifeTime _serviceBusLifeTime;
        private readonly MyNoSqlClientLifeTime _noSqlClientLifeTime;
        private readonly PaymentProcessingJob _paymentProcessingJob;
        
        public ApplicationLifetimeManager(IHostApplicationLifetime appLifetime, ILogger<ApplicationLifetimeManager> logger, ServiceBusLifeTime serviceBusLifeTime, MyNoSqlClientLifeTime noSqlClientLifeTime, PaymentProcessingJob paymentProcessingJob)
            : base(appLifetime)
        {
            _logger = logger;
            _serviceBusLifeTime = serviceBusLifeTime;
            _noSqlClientLifeTime = noSqlClientLifeTime;
            _paymentProcessingJob = paymentProcessingJob;
        }

        protected override void OnStarted()
        {
            _logger.LogInformation("OnStarted has been called.");
            _serviceBusLifeTime.Start();
            _noSqlClientLifeTime.Start();
            _paymentProcessingJob.Start();

        }

        protected override void OnStopping()
        {
            _logger.LogInformation("OnStopping has been called.");
            _paymentProcessingJob.Dispose();
            _serviceBusLifeTime.Stop();
            _noSqlClientLifeTime.Stop();
        }

        protected override void OnStopped()
        {
            _logger.LogInformation("OnStopped has been called.");
        }
    }
}
