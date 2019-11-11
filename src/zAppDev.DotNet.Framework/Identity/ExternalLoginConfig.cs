namespace zAppDev.DotNet.Framework.Identity
{
    public class ExternalLoginConfig
    {
        public bool IsGoogleEnabled { get; set; }

        public string GoogleClientId { get; set; }

        public string GoogleClientSecret { get; set; }

        public bool IsFacebookEnabled { get; set; }

        public string FacebookClientId { get; set; }

        public string FacebookClientSecret { get; set; }
    }
}
