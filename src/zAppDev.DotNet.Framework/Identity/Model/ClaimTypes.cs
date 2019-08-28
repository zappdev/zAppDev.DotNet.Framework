namespace zAppDev.DotNet.Framework.Identity.Model
{
    public static class ClaimTypes
    {
        internal const string ClaimTypeNamespace = "http://schemas.clmsuk.com/2013/12/identity/claims";

        /// <summary>
        /// The URI for a claim that specifies a permission of an entity, http://schemas.clmsuk.com/2013/12/identity/claims/permission.
        /// </summary>
        public const string Permission = "http://schemas.clmsuk.com/2013/12/identity/claims/permission";

        /// <summary>
        /// Default action claim type.
        /// </summary>
        public const string ActionType = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/action";

        /// <summary>
        /// Default resource claim type
        /// </summary>
        public const string ResourceType = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/resource";

        /// <summary>
        /// Controller Action resource claim type
        /// </summary>
        public const string ControllerAction = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/controlleraction";

        /// <summary>
        /// Dataset resource claim type
        /// </summary>
        public const string Dataset = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/dataset";

        /// <summary>
        /// Generic Action resource claim type
        /// </summary>
        public const string GenericAction = "http://schemas.clmsuk.com/2013/12/identity/claimsauthorization/genericaction";

        /// <summary>
        /// Url resource claim type
        /// </summary>
        public const string Url = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/url";

        /// <summary>
        /// IDEF0 Activity resource claim type
        /// </summary>
        public const string IDEF0Activity = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/idef0activity";

        /// <summary>
        /// ExposedService resource claim type
        /// </summary>
        public const string ExposedService = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/exposedservice";

        /// <summary>
        /// Application Access claim type
        /// </summary>
        public const string ApplicationAccess = "http://schemas.clmsuk.com/2013/12/identity/claims/authorization/application";

        /// <summary>
        /// Persistent Login claim type
        /// </summary>
        public const string PersistentLogin = "http://schemas.clmsuk.com/2013/12/identity/claims/authentication/persistentlogin";

        /// <summary>
        /// Windows Identity Exists claim type
        /// </summary>
        public const string LocalLogin = "http://schemas.clmsuk.com/2013/12/identity/claims/authentication/locallogin";

        #region External Profile Claims
        /// <summary>
        /// The person's gender. Possible values include, but are not limited to, the following values: "male", "female", "other" (Google+)
        /// </summary>
        public const string Gender = "http://schemas.clmsuk.com/2013/12/identity/claims/external/user/gender";
        /// <summary>
        /// The family name (last name) of this person. (Google+)
        /// </summary>
        public const string Surname = "http://schemas.clmsuk.com/2013/12/identity/claims/external/user/surname";
        /// <summary>
        /// The given name (first name) of this person.(Google+)
        /// </summary>
        public const string Name = "http://schemas.clmsuk.com/2013/12/identity/claims/external/user/name";
        /// <summary>
        /// The name of this person, which is suitable for display.
        /// </summary>
        public const string DisplayName = "http://schemas.clmsuk.com/2013/12/identity/claims/external/user/displayName";
        /// <summary>
        /// The e-mail of this person
        /// </summary>
        public const string Email = "http://schemas.clmsuk.com/2013/12/identity/claims/external/user/email";
        #endregion        
    }
}