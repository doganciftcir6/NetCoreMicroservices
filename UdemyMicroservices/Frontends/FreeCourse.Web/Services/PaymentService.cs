using FreeCourse.Web.Models.FakePayment;
using FreeCourse.Web.Services.Interface;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<bool> ReceivePaymentAsync(PaymentInfoInput paymentInfoInput)
        {
            //gönderdiğim datayı direkt olarak serlize yapsın jsona ve apiye json bilgi göndersin.
            //PostAsJsonAsync ile
            var response = await _httpClient.PostAsJsonAsync<PaymentInfoInput>("fakepayments", paymentInfoInput);
            return response.IsSuccessStatusCode;

        }
    }
}
