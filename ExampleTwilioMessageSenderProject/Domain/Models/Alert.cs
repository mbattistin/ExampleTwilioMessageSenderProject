namespace ExampleTwilioMessageSenderProject.Domain.Models
{
    public class Alert
    {
        public string Sender { get; set; }
        public List<string> Recipients { get; set; }
        public string MessageTitle { get; set; }
        public string MessageBody { get; set; }
        public string MessageSendingType { get; set; }
        public string AttachmentFileNameWithExtension { get; set; }
        public string Base64FileContentAttachment { get; set; }
        public string MimeTypeFileAttachment { get; set; }
    }
}
