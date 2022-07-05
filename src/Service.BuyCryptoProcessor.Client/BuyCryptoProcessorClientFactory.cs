using JetBrains.Annotations;
using MyJetWallet.Sdk.Grpc;
using Service.BuyCryptoProcessor.Grpc;

namespace Service.BuyCryptoProcessor.Client
{
    [UsedImplicitly]
    public class BuyCryptoProcessorClientFactory: MyGrpcClientFactory
    {
        public BuyCryptoProcessorClientFactory(string grpcServiceUrl) : base(grpcServiceUrl)
        {
        }

        public ICryptoBuyService GetHelloService() => CreateGrpcService<ICryptoBuyService>();
    }
}
