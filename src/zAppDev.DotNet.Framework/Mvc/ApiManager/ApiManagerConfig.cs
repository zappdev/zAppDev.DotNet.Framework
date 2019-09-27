namespace zAppDev.DotNet.Framework.Mvc.API
{
    public class ApiManagerConfig
    {
        public bool LogEnabled { get; set; }
        public bool AllowPartialResponse { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
    }
}
