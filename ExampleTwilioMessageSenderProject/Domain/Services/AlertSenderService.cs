using ExampleTwilioMessageSenderProject.Domain.Contract;
using ExampleTwilioMessageSenderProject.Domain.Models;
using ExampleTwilioMessageSenderProject.Domain.Models.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;
using System.Net;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace ExampleTwilioMessageSenderProject.Domain.Service
{
    public class AlertSenderService : IAlertSenderService
    {
        private readonly SendGridMailConfigurations _sendGridMailConfig;
        private readonly TwilioConfigurations _twilioConfig;

        public AlertSenderService(SendGridMailConfigurations sendGridMailConfig, TwilioConfigurations twilioConfig)
        {
            _sendGridMailConfig = sendGridMailConfig;
            _twilioConfig = twilioConfig;
        }   

        public async Task<ResponseResult> SendAlertAsync(Alert alert)
        {
            ResponseResult response;

            var messageSendingType = alert.MessageSendingType.Replace("-", "").ToLower();

            if (messageSendingType.Equals("email"))
            {
                response = await SendEmailUsingTwiloAsync(alert);
            }
            else if (messageSendingType.Equals("sms"))
            {
                response = await SendSMSMessageFromTwilo(alert);
            }
            else if (messageSendingType.Equals("whatsapp"))
            {
                response = await SendWhatsAppMessageFromTwilo(alert);
            }
            else
            {
                response = new ResponseResult(false, "Message sending type not recognized.");
            }

            return response;
        }

        private async Task<ResponseResult> SendWhatsAppMessageFromTwilo(Alert alert)
        {
            string whatsappErrorNumbers = "";
            string bodyMessage = $"Hello! The application informs: {alert.MessageBody}";
            TwilioClient.Init(_twilioConfig.AccountSid, _twilioConfig.AuthToken);

            foreach (var recipient in alert.Recipients)
            {
                try
                {
                    //number format +(country)(regional code)(number): +12182824886";
                    var message = await MessageResource.CreateAsync(
                        body: bodyMessage,
                        from: new Twilio.Types.PhoneNumber($"whatsapp:{_twilioConfig.SenderWhatsappNumber}"),
                        to: new Twilio.Types.PhoneNumber($"whatsapp:{recipient}"));
                }
                catch (Exception)
                {
                    whatsappErrorNumbers += $"{recipient} ";
                }
            }

            if (whatsappErrorNumbers != "")
                return new ResponseResult(false, null, $"Failed to send whatsapp to recipient(s): {whatsappErrorNumbers}");


            return new ResponseResult(true, null);
        }

        private async Task<ResponseResult> SendSMSMessageFromTwilo(Alert alert)
        {
            string smsErrorNumbers = "";

            //number format +(country)(regional code)(number): +12182824886";
            TwilioClient.Init(_twilioConfig.AccountSid, _twilioConfig.AuthToken);
            foreach (var recipient in alert.Recipients)
            {
                try
                {

                    var message = await MessageResource.CreateAsync(
                        body: alert.MessageBody,
                        from: new Twilio.Types.PhoneNumber(_twilioConfig.SenderPhoneNumber),
                            to: new Twilio.Types.PhoneNumber(recipient)
                        );

                }
                catch (Exception)
                {
                    smsErrorNumbers += $"{recipient} ";
                }
            }

            if (smsErrorNumbers != "")
                return new ResponseResult(false, null, $"Failed to send sms to recipient(s): {smsErrorNumbers}");

            return new ResponseResult(true, null);
        }

        private async Task<ResponseResult> SendEmailUsingTwiloAsync(Alert alert)
        {
            var client = new SendGridClient(_sendGridMailConfig.ApiKey);
            var tos = alert.Recipients.Select(d => new EmailAddress(d, "")).ToList();
            var from = new EmailAddress(alert.Sender, "");

            var message = MailHelper.CreateSingleEmailToMultipleRecipients(
                from, tos, alert.MessageTitle, "", alert.MessageBody);

            if (!string.IsNullOrEmpty(alert.Base64FileContentAttachment))
                message.AddAttachment(alert.AttachmentFileNameWithExtension, alert.Base64FileContentAttachment, alert.MimeTypeFileAttachment);
            var messagemId = "";

            try
            {
                var response = await client.SendEmailAsync(message);

                messagemId = response.Headers.Single(x => x.Key == "X-Message-Id").Value.FirstOrDefault()?.ToString();

                if (HttpStatusCode.OK == response.StatusCode || HttpStatusCode.Accepted == response.StatusCode)
                {
                    return new ResponseResult(true, messagemId);
                }
                else
                {
                    return new ResponseResult(false, messagemId, String.Format(GetSendGridErroMessage(response.StatusCode), string.Join(",", alert.Recipients)));
                }
            }
            catch (Exception ex)
            {
                return new ResponseResult(false, messagemId, ex.Message);
            }
        }


        private string GetSendGridErroMessage(HttpStatusCode statusCode)
        {
            switch (statusCode)
            {
                case HttpStatusCode.BadRequest:
                    return "Failed to send email to recipient(s): {0}. Incorrect parameters.";
                case HttpStatusCode.Unauthorized:
                    return "Failed to send email to recipient(s): {0}. Problem authenticating in application.";
                case HttpStatusCode.Forbidden:
                    return "Failed to send email to recipient(s): {0}. Access denied in application.";
                case HttpStatusCode.NotFound:
                    return "Failed to send email to recipient(s): {0}. Method not found in application.";
                case HttpStatusCode.MethodNotAllowed:
                    return "Failed to send email to recipient(s): {0}. Method not found in application.";
                case HttpStatusCode.RequestEntityTooLarge:
                    return "Failed to send email to recipient(s): {0}. The data entered exceeded the size limit allowed by the application.";
                case HttpStatusCode.UnsupportedMediaType:
                    return "Failed to send email to recipient(s): {0}. Sent media is not supported.";
                case HttpStatusCode.TooManyRequests:
                    return "Failed to send email to recipient(s): {0}. The number of requests in the application exceeded the allowed limit.";
                case HttpStatusCode.InternalServerError:
                    return "Failed to send email to recipient(s): {0}. Application is unavailable.";
                case HttpStatusCode.ServiceUnavailable:
                    return "Failed to send email to recipient(s): {0}. Application is unavailable.";
                default:
                    return "Failed to send email to recipient(s): {0}.";
            }
        }
    }
}
