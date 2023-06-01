namespace ExampleTwilioMessageSenderProject.Domain.Models
{
    public class ResponseResult
    {
        public ResponseResult(bool success, string xIdMessage, string errorMessage = "")
        {
            Success = success;
            XIdMessage = xIdMessage;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; set; }
        public string XIdMessage { get; set; }
        public string ErrorMessage { get; set; }
    }
}
