using ExampleTwilioMessageSenderProject.Domain.Models;

namespace ExampleTwilioMessageSenderProject.Domain.Contract
{
    public interface IAlertSenderService
    {
        Task<ResponseResult> SendAlertAsync(Alert alett);
    }
}
