using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Service.BuyCryptoProcessor.Domain.Models;

namespace Service.BuyCryptoProcessor.Grpc.Models
{
    [DataContract]
    public class IntentionsResponse
    {
        [DataMember(Order = 1)] public bool Success { get; set; }
        [DataMember(Order = 2)] public string ErrorMessage { get; set; }
        [DataMember(Order = 3)] public DateTime TsForNextQuery { get; set; }
        [DataMember(Order = 4)] public List<CryptoBuyIntention> Intentions { get; set; }
    }
    
}