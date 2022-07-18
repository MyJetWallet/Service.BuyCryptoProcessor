using Autofac;
using MyJetWallet.Sdk.NoSql;
using MyJetWallet.Sdk.ServiceBus;
using MyServiceBus.Abstractions;
using Service.Bitgo.DepositDetector.Client;
using Service.Bitgo.DepositDetector.Domain.Models;
using Service.BuyCryptoProcessor.Domain.Models;
using Service.BuyCryptoProcessor.Jobs;
using Service.ChangeBalanceGateway.Client;
using Service.Fees.Client;
using Service.IndexPrices.Client;
using Service.Liquidity.Converter.Client;

namespace Service.BuyCryptoProcessor.Modules
{
    public class ServiceModule: Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var myNoSqlClient = builder.CreateNoSqlClient(Program.Settings.MyNoSqlReaderHostPort, Program.LogFactory);
            var serviceBusClient = builder.RegisterMyServiceBusTcpClient(()=>Program.Settings.SpotServiceBusHostPort, Program.LogFactory);
            var queueName = "Service.BuyCryptoProcessor";
            
            builder.RegisterMyServiceBusSubscriberSingle<Deposit>(serviceBusClient, Deposit.TopicName, queueName,
                TopicQueueType.PermanentWithSingleConnection);
            builder.RegisterMyServiceBusPublisher<CryptoBuyIntention>(serviceBusClient, CryptoBuyIntention.TopicName, true);
            
            builder.RegisterCircleDepositServiceClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl);
            builder.RegisterUnlimintDepositServiceClient(Program.Settings.BitgoDepositDetectorGrpcServiceUrl);
            builder.RegisterLiquidityConverterClient(Program.Settings.LiquidityConverterGrpcServiceUrl);
            builder.RegisterSpotChangeBalanceGatewayClient(Program.Settings.ChangeBalanceGatewayGrpcServiceUrl);
            builder.RegisterFeesClients(myNoSqlClient);
            builder.RegisterIndexPricesClient(myNoSqlClient);
            builder.RegisterType<PaymentProcessingJob>().AsSelf().SingleInstance().AutoActivate();
        }
    }
}