namespace zAppDev.DotNet.Framework.Identity
{
    public class PasswordPolicyConfig
    {
        public int RequiredLength { get; set; }

        public bool RequireNonLetterOrDigit { get; set; }

        public bool RequireLowercase { get; set; }

        public bool RequireUppercase { get; set; }

        public bool RequireDigit { get; set; }

        public PasswordPolicyConfig()
        {
            RequiredLength = 6;
            RequireNonLetterOrDigit = true;
            RequireLowercase = true;
            RequireUppercase = true;
            RequireDigit = true;
        }
    }
}
