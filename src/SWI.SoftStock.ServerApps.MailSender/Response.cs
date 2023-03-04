namespace SWI.SoftStock.ServerApps.MailSender
{
    /// <summary>
    /// Response from service
    /// </summary>
    public class Response
    {
        /// <summary>
        /// Code=0 - OK, else Error
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// If Code!=0 then Message contains error details
        /// </summary>
        public string Message { get; set; }
    }
}