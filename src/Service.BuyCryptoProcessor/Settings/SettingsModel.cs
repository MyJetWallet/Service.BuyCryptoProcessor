using MyJetWallet.Sdk.Service;
using MyYamlParser;

namespace Service.BuyCryptoProcessor.Settings
{
    public class SettingsModel
    {
        [YamlProperty("BuyCryptoProcessor.SeqServiceUrl")]
        public string SeqServiceUrl { get; set; }

        [YamlProperty("BuyCryptoProcessor.ZipkinUrl")]
        public string ZipkinUrl { get; set; }

        [YamlProperty("BuyCryptoProcessor.ElkLogs")]
        public LogElkSettings ElkLogs { get; set; }
        
        [YamlProperty("BuyCryptoProcessor.MyNoSqlReaderHostPort")]
        public string  MyNoSqlReaderHostPort{ get; set; } 
        
        [YamlProperty("BuyCryptoProcessor.SpotServiceBusHostPort")]
        public string  SpotServiceBusHostPort{ get; set; } 
        
        [YamlProperty("BuyCryptoProcessor.PostgresConnectionString")]
        public string PostgresConnectionString { get; set; }
                
        [YamlProperty("BuyCryptoProcessor.BitgoDepositDetectorGrpcServiceUrl")]
        public string  BitgoDepositDetectorGrpcServiceUrl{ get; set; }
        
        [YamlProperty("BuyCryptoProcessor.LiquidityConverterGrpcServiceUrl")]
        public string  LiquidityConverterGrpcServiceUrl{ get; set; } 
        
        [YamlProperty("BuyCryptoProcessor.ChangeBalanceGatewayGrpcServiceUrl")]
        public string  ChangeBalanceGatewayGrpcServiceUrl{ get; set; } 
        
        [YamlProperty("BuyCryptoProcessor.ServiceClientId")]
        public string ServiceClientId { get; set; }
        [YamlProperty("BuyCryptoProcessor.ServiceWalletId")]
        public string ServiceWalletId { get; set; }
        
        [YamlProperty("BuyCryptoProcessor.TimerPeriodInSec")]
        public int TimerPeriodInSec { get; set; }
        
        [YamlProperty("BuyCryptoProcessor.RetriesLimit")]
        public int RetriesLimit { get; set; }
        
        [YamlProperty("BuyCryptoProcessor.CircleSuccessUrl")]
        public string CircleSuccessUrl { get; set; }
        [YamlProperty("BuyCryptoProcessor.CircleFailureUrl")]
        public string CircleFailureUrl { get; set; }


        

    }
}
