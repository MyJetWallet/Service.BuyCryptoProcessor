using Autofac;
using Service.BuyCryptoProcessor.Grpc;

// ReSharper disable UnusedMember.Global

namespace Service.BuyCryptoProcessor.Client
{
    public static class AutofacHelper
    {
        public static void RegisterBuyCryptoProcessorClient(this ContainerBuilder builder, string grpcServiceUrl)
        {
            var factory = new BuyCryptoProcessorClientFactory(grpcServiceUrl);

            builder.RegisterInstance(factory.GetBuyService()).As<ICryptoBuyService>().SingleInstance();
            builder.RegisterInstance(factory.GetBuyManager()).As<ICryptoBuyManager>().SingleInstance();

        }
    }
}
