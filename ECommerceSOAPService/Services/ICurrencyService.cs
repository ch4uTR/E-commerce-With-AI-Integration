using ECommerceSOAPService.Models;
using SoapCore;
using System.ServiceModel;

namespace ECommerceSOAPService.Services
{
    [ServiceContract]
    public interface ICurrencyService
    {

        [OperationContract]
        Task<List<CurrencyDTO>> GetCurrencyRates();

    }



}
