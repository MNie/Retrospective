namespace WebApp.Configuration
{
    public class AblyConfig
    {
        public string ApiKey { get; set; }
        public AblyChannel Push { get; set; }
        public AblyChannel Topic { get; set; }
    }
}