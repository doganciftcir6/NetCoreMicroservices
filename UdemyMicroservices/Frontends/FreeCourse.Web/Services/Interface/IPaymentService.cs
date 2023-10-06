using FreeCourse.Web.Models.FakePayment;
using System.Threading.Tasks;

namespace FreeCourse.Web.Services.Interface
{
    public interface IPaymentService
    {
        Task<bool> ReceivePaymentAsync(PaymentInfoInput paymentInfoInput);
    }
}
