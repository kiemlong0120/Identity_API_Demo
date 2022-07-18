namespace Identity_API_Demo.Services
{
    public interface ISendMailService
    {
        public string SendMail(string to, string from, string subject, string body);
    }
}
