// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using log4net;

using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Data.Domain;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using NHibernate;

using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public static class IdentityHelper
    {
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public static bool AdminCanResetPassword = true;

        public static CustomUserManager GetUserManager()
        {
            return ServiceLocator.Current.GetInstance<CustomUserManager>();
        }

        public static SignInManager<Model.IdentityUser> GetSignInManager()
        {
            return ServiceLocator.Current.GetInstance<SignInManager<Model.IdentityUser>>();
        }

        public static bool SignIn(string username, string password, bool isPersistent)
        {
            if (string.IsNullOrWhiteSpace(username)
                || string.IsNullOrWhiteSpace(username)) return false;

            var signInManager = GetSignInManager();
            var result = signInManager.PasswordSignInAsync(username, password, isPersistent, true).Result;
            return HandleLoginResult(result, username);
        }

        private static void SignIn(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            var signInManager = GetSignInManager();
            var result = signInManager.ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent).Result;
            var loggedIn = HandleLoginResult(result, loginInfo);
            if (!loggedIn)
            {
                var email = loginInfo.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);

                throw new ApplicationException($"Could not login with external login: {email}, provider: {loginInfo.LoginProvider}");
            }
        }

        public static async Task SignOut()
        {
            var context = Web.GetContext();
            var id = (context.User.Identity as ClaimsIdentity)?.Name;

            if (!string.IsNullOrWhiteSpace(id))
            {
                var clientKey = Web.GetBrowserType();
                var manager = GetUserManager();
                var user = manager.FindById(id);
                manager.SignOutClient(user, clientKey);
            }

            await GetSignInManager().SignOutAsync();
        }

        public static string GetUserProfile()
        {
            var user = GetCurrentIdentityUser()?.User;
            return JsonConvert.SerializeObject(new UserProfileDTO(user), new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                StringEscapeHandling = StringEscapeHandling.EscapeHtml
            });
        }

        public static Model.IdentityUser GetIdentityUserByName(string userName)
        {
            var manager = GetUserManager();
            return manager.FindByName(userName);
        }

        public static Model.IdentityUser GetCurrentIdentityUser()
        {
            var identity = Web.GetContext().User.Identity;

            if (!identity.IsAuthenticated) return null;
            var manager = GetUserManager();

            return manager.FindById(identity.Name);
        }

        public static ApplicationUser GetCurrentApplicationUser()
        {
            return GetCurrentIdentityUser()?.User;
        }

        public static string GetCurrentUserName()
        {
            return Web.GetContext().User.Identity.Name;
        }

        public static List<string> GetCurrentApplicationUserRoles()
        {
            return GetApplicationUserRoles(GetCurrentUserName());
        }

        public static List<string> GetCurrentApplicationUserPermissions()
        {
            return GetUserActivePermissions(GetCurrentUserName());
        }

        public static bool CurrentUserHasPermission(string permission)
        {
            var permissions = GetCurrentApplicationUserPermissions();
            return permissions.Contains(permission);
        }

        public static List<string> GetUserActivePermissions(string username)
        {
            var user = GetIdentityUserByName(username);
            var activePermissions = new List<string>();
            if (user == null) return activePermissions;
            activePermissions = user.User.Permissions.Select(a => a.Name).ToList();
            activePermissions.AddRange(user.User.Roles.SelectMany(a => a.Permissions).Select(a => a.Name));
            return activePermissions;
        }

        public static bool IsAuthenticatedInMode(Type authType)
        {
            var identity = Web.GetContext().User.Identity;
            return
            (
                (
                    (identity != null)
                    &&
                    (identity.IsAuthenticated)
                )
                &&
                (
                    (authType == null)
                    ||
                    (identity.GetType() == authType)
                )
            );
        }

        public static async Task<bool> PasswordIsValid(string password)
        {
            var manager = GetUserManager();

            var result = await manager.CheckPasswordAsync(GetCurrentIdentityUser(), password);
            return result;
        }

        public static List<string> GetApplicationUserRoles(string username)
        {
            var user = GetIdentityUserByName(username);
            var list = new List<string>();
            if (user == null) return list;
            list.AddRange(user.User.Roles.Select(role => role.Name));
            return list;
        }

        public static ApplicationUser GetApplicationUserByName(string username)
        {
            return GetIdentityUserByName(username).User;
        }

        public static bool ValidateUser(string username, string password)
        {
            var manager = GetUserManager();
            return manager.Find(username, password) != null;
        }

        public static string CreateUser(ApplicationUser appUser, string password)
        {
            var manager = GetUserManager();
            return CreateUser(manager, appUser, password);
        }

        public static string CreateUser(CustomUserManager manager, ApplicationUser appUser, string password)
        {
            var user = new Model.IdentityUser(appUser);
            var result = manager.CreateAsync(user, password).Result;
            return result.Succeeded ? null : result.Errors.First().Description;
        }

        private static Model.IdentityUser GetIdentityUser(string username, string email, string name, string userClass = null)
        {
            var defaultUser = new Model.IdentityUser(new ApplicationUser { UserName = username, Email = email, Name = name });

            try
            {
                if (!string.IsNullOrWhiteSpace(userClass) && userClass != "ApplicationUser")
                {
                    var classType = Type.GetType($@"zAppDevRegistry.BO.{userClass}");

                    var unwrappedUser = Activator.CreateInstance(classType) as ApplicationUser;
                    if (unwrappedUser != null)
                    {
                        unwrappedUser.UserName = username;
                        unwrappedUser.Email = email;
                        unwrappedUser.Name = name;
                        return new Model.IdentityUser(unwrappedUser);
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(IdentityHelper)).Error($"Exception caught while creating the Identity User: {e.Message}\r\n(StackTrace: {e.StackTrace})");
            }
            return defaultUser;
        }

        public static IEnumerable<string> CreateAndLoginUser(string username, string email, string name, string userClass = null)
        {
            var info = GetSignInManager().GetExternalLoginInfoAsync().Result;

            if (info == null)
            {
                return new[] { "No External Login information found." };
            }

            return CreateAndLoginUser(username, email, name, info.Principal.Claims, info, userClass);
        }

        public static string[] CreateAndLoginUser(string username, string email, string name, IEnumerable<Claim> claims, ExternalLoginInfo externalLoginInfo, string userClass = null)
        {
            var manager = GetUserManager();
            var user = GetIdentityUser(username, email, name, userClass);
            AddClaimsToUser(user, claims);

            var result = manager.CreateAsync(user).Result;

            if (result.Succeeded)
            {
                result = manager.AddLoginAsync(user, externalLoginInfo).Result;
                if (result.Succeeded)
                {
                    SignIn(externalLoginInfo, isPersistent: false);
                    // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                    // var code = manager.GenerateEmailConfirmationToken(user.Id);
                    // Send this link via email: IdentityHelper.GetUserConfirmationRedirectUrl(code, user.Id)
                    //IdentityHelper.RedirectToReturnUrl(returnUrl, HttpContext.Current.Response);
                    return new string[0];
                }
            }
            return ReportErrors(result.Errors);
        }

        public static bool ValidateExternalLoginAndLogin(out ApplicationUser appUser, out IEnumerable<string> errors)
        {
            errors = new[] { "" };
            appUser = null;

            var manager = GetUserManager();
            var loginInfo = GetSignInManager().GetExternalLoginInfoAsync().Result;

            if (loginInfo == null)
            {
                //RedirectOnFail(HttpContext.Current.Response);
                errors = new[] { "No External Login information found." };
                return false;
            }

            // Find local user associated with the external login
            var user = manager.Find(loginInfo);

            if (user != null)
            {
                // If local user exists -> Login
                SignIn(loginInfo, isPersistent: false);
                appUser = user.User;
                return true;
                //IdentityHelper.RedirectToReturnUrl(HttpContext.Current.Request.QueryString["ReturnUrl"], HttpContext.Current.Response);
            }

            if (Web.GetContext().User.Identity.IsAuthenticated)
            {
                // If the external login is not associated with the currently logged in User -> associate
                // Apply Xsrf check when linking

                loginInfo = GetSignInManager().GetExternalLoginInfoAsync(XsrfKey).Result;
                if (loginInfo == null)
                {
                    errors = new[] { "No External Login information found." };
                    return false;
                }

                var identityUser = manager.Find(loginInfo);
                var addLoginResult = manager.AddLoginAsync(identityUser, loginInfo).Result;
                if (addLoginResult.Succeeded)
                {
                    // If successfully associated the user with the external login -> redirect to ReturnUrl
                    //IdentityHelper.RedirectToReturnUrl(HttpContext.Current.Request.QueryString["ReturnUrl"], HttpContext.Current.Response);
                    appUser = manager.Find(loginInfo).User;
                    return true;
                }
                // Failure to associate -> Return any Errors (result is already populated)
                errors = ReportErrors(addLoginResult.Errors);
                return false;
            }

            // If not local user exists and no user is already logged in in this session -> return to UI to get more info
            errors = new[] { "No local user exists. Please create your local user by providing the following details." };
            var emailValue = loginInfo.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
            appUser = new ApplicationUser
            {
                Email = emailValue,
                UserName = emailValue,
                Name = loginInfo.Principal.Identity.Name,
                Claims = new List<ApplicationUserClaim>()
            };

            AddClaimsToUser(appUser, loginInfo.Principal.Claims);

            return false;
        }

        public static ClaimsPrincipal CreateLocalUserForWindowsIdentity(ClaimsPrincipal incomingPrincipal)
        {
            var log = LogManager.GetLogger(typeof(IdentityHelper));
            var name = incomingPrincipal.Identity.Name;
            var localLoginClaim = incomingPrincipal.FindFirst(Model.ClaimTypes.LocalLogin);

            if (localLoginClaim != null && localLoginClaim.Value == bool.TrueString)
            {
                log.DebugFormat("Identity {0}, has local login and has been processed.", name);
                return incomingPrincipal;
            }

            log.DebugFormat("Identity {0}, has not been processed.", name);
            // Find local user associated with the windows login
            var manager = GetUserManager();
            var user = manager.FindByName(name);

            if (user == null)
            {
                user = CreateLocalUserFromNltm(incomingPrincipal, log, name, manager);
            }
            else
            {
                log.Debug($"User {name} already has local login.");
            }
            PrintUser(user, log);

            var clientKey = Web.GetBrowserType();

            if (user.User.Clients.Count(c => c.ClientKey == clientKey) == 0)
            {
                manager.SignInClient(user,
                                     clientKey,
                                     Web.GetClientIp(),
                                     Web.GetContext().Session != null ? Web.GetContext().Session.Id : string.Empty);
            }

            log.DebugFormat("Identity {0}, has local login. Adding Permission and Role Claims...", name);

            return AddClaimsForNltmAuthUser(incomingPrincipal, user);
        }

        public const string ProviderNameKey = "providerName";

        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request.Query[ProviderNameKey];
        }

        public static string GetExternalLoginRedirectUrl(string accountProvider)
        {
            return "/Account/RegisterExternalLogin?" + ProviderNameKey + "=" + accountProvider;
        }

        private static bool IsLocalUrl(string url)
        {
            return !string.IsNullOrEmpty(url) && ((url[0] == '/' && (url.Length == 1 || (url[1] != '/' && url[1] != '\\'))) || (url.Length > 1 && url[0] == '~' && url[1] == '/'));
        }

        public static void RedirectOnFail(HttpResponse response)
        {
            var principal = Web.GetContext().User;
            response.RedirectToAction(principal.Identity.IsAuthenticated ? "Home" : "SignIn");
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!string.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
            {
                response.Redirect(returnUrl);
            }
            else
            {
                response.Redirect("~/");
            }
        }

        public static void CreateOrUpdateUser(ApplicationUser applicationUser)
        {
            if (applicationUser.IsTransient())
            {
                var manager = GetUserManager();
                var result = manager.CreateAsync(new Model.IdentityUser(applicationUser)).Result; //, ApplicationUser.PasswordHash);
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Could not create User. {string.Join("\r\n", ReportErrors(result.Errors))}");
                }
            }
            else
            {
                var manager = GetUserManager();
                var result = manager.UpdateAsync(new Model.IdentityUser(applicationUser)).Result;
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Could not update User. {string.Join("\r\n", ReportErrors(result.Errors))}");
                }
            }
        }

        public static string ChangePassword(ApplicationUser user, string currentPassword, string newPassword)
        {
            var manager = GetUserManager();
            var result = manager.ChangePasswordAsync(GetIdentityUserByName(user.UserName), currentPassword, newPassword).Result;
            return result.Succeeded ? null : result.Errors.First().Description;
        }

        public static string ResetPassword(ApplicationUser user, string newPassword)
        {
            var manager = GetUserManager();
            var identityUser = GetIdentityUserByName(user.UserName);

            var result = manager.RemovePasswordAsync(identityUser).Result;
            if (result.Succeeded)
            {
                result = manager.AddPasswordAsync(identityUser, newPassword).Result;
            }
            return result.Succeeded ? null : result.Errors.First().Description;
        }

        public static string ResetPasswordByAdmin(ApplicationUser user, string newPassword)
        {
            if (AdminCanResetPassword)
            {
                return ResetPassword(user, newPassword);
            }
            else
            {
                throw new ApplicationException("Not allowed to reset password of other user!");
            }
        }

        public static void LogAction(string controller, string action, bool success, string errorMessage)
        {
            var factory = ServiceLocator.Current.GetInstance<ISessionFactory>();

            using (var manager = new MiniSessionService(factory))
            {
                manager.OpenSessionWithTransaction();

                var repo = ServiceLocator.Current
                    .GetInstance<Data.DAL.IRepositoryBuilder>()
                    .CreateCreateRepository(manager);
                // If Logging is disabled, return
                var logSetting = repo.Get<ApplicationSetting>(s => s.Key == "OperationAccessLog").FirstOrDefault();
                if (
                    logSetting == null ||
                    string.IsNullOrWhiteSpace(logSetting.Value) ||
                    string.Compare(logSetting.Value, "false", StringComparison.OrdinalIgnoreCase) == 0 ||
                    string.Compare(logSetting.Value, "0", StringComparison.OrdinalIgnoreCase) == 0
                )
                    return;
                ApplicationUser user = null;
                var activePermissions = new List<string>();
                try
                {
                    user = GetCurrentApplicationUser();
                    activePermissions = GetCurrentApplicationUserPermissions();
                }
                catch (Exception e)
                {
                    var log = LogManager.GetLogger(typeof(IdentityHelper));
                    log.Error($"Failed to GetCurrentApplicationUser while Logging action: {controller}.{action}", e);
                }
                var entry = new ApplicationUserAction
                {
                    UserName = user == null ? "Anonymous" : user.UserName,
                    Action = action,
                    Controller = controller,
                    Date = DateTime.UtcNow,
                    ActivePermissions = user == null ? "" : string.Join(";", activePermissions),
                    ActiveRoles = user == null ? "" : string.Join(";", user.Roles.Select(x => x.Name)),
                    Success = success,
                    ErrorMessage = errorMessage
                };
                repo.Save(entry);

                manager.CommitChanges();
            }
        }

        public static void AddClaimsToUser(ApplicationUser user, IEnumerable<Claim> claims)
        {
            foreach (var claim in claims)
            {
                user.AddClaims(new ApplicationUserClaim
                {
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    ClaimValueType = claim.ValueType
                });
            }
        }

        public static void AddClaimsToUser(Model.IdentityUser user, IEnumerable<Claim> claims)
        {
            AddClaimsToUser(user.User, claims);
        }

        private static string[] ReportErrors(IEnumerable<IdentityError> resultErrors)
        {
            return resultErrors.Select(ex => $"[{ex.Code}] - {ex.Description}").ToArray();
        }

        #region NLTM Authentication (Windows)
        private static void PrintUser(Model.IdentityUser user, ILog log)
        {
            try
            {
                if (user == null)
                {
                    log.Debug("PrintUser: user is null");
                    return;
                }
                if (user.User == null)
                {
                    log.Debug($"PrintUser: user {user.Id} has no ApplicationUser member");
                    return;
                }
                var userRoles = string.Join(",", user.User.Roles.Select(r => r.Name));
                var userPermissions = string.Join(",", user.User.Permissions.Select(r => r.Name));
                log.Debug($"User {user.Id} Roles: {userRoles}, Permissions: {userPermissions}");
                foreach (var role in user.User.Roles)
                {
                    var rolePermissions = role.Permissions.Select(a => a.Name);
                    log.Debug($"User {user.Id} Role {role.Name} Permissions: {rolePermissions}");
                }
            }
            catch (Exception e)
            {
                log.Error($"PrintUser for user: '{user?.Id}' failed", e);
            }
        }

        private static Model.IdentityUser CreateLocalUserFromNltm(ClaimsPrincipal incomingPrincipal, ILog log, string name, CustomUserManager manager)
        {
            log.DebugFormat("Identity {0}, does not have local login. Creating...", name);
            var id = incomingPrincipal.FindFirstValue(System.Security.Claims.ClaimTypes.PrimarySid);
            var externalLoginInfo = new ExternalLoginInfo(
                incomingPrincipal,
                "NTLM",
                id,
                name);

            var email = incomingPrincipal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? string.Empty;
            var givenName = incomingPrincipal.FindFirst(System.Security.Claims.ClaimTypes.GivenName)?.Value ?? string.Empty;

            var errors = CreateAndLoginUser(name, email, givenName, incomingPrincipal.Claims, externalLoginInfo, null);

            if (errors.Any())
                throw new ApplicationException("Could not create local user!\r\n" + string.Join("\r\n", errors));

            var user = manager.FindByName(name);
            return user;
        }

        private static ClaimsPrincipal AddClaimsForNltmAuthUser(ClaimsPrincipal incomingPrincipal, Model.IdentityUser user)
        {
            var newIdentity = new ClaimsIdentity("Negotiate");
            newIdentity.AddClaims(incomingPrincipal.Claims);
            newIdentity.AddClaims(GetCurrentApplicationUserPermissions().Select(p => new Claim(Model.ClaimTypes.Permission, p)));
            newIdentity.AddClaims(user.User.Roles.Select(r => new Claim(System.Security.Claims.ClaimTypes.Role, r.Name)));
            newIdentity.AddClaim(new Claim(Model.ClaimTypes.LocalLogin, bool.TrueString));
            return new ClaimsPrincipal(newIdentity);
        }
        #endregion

        #region External Account Functions

        public static bool ExternalAccountIsLinked(string loginProvider, ApplicationUser user = null)
        {
            if (string.IsNullOrWhiteSpace(loginProvider))
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if (user == null)
            {
                user = GetCurrentApplicationUser();
            }
            return user?.Logins?.FirstOrDefault(x => x.LoginProvider == loginProvider) != null;
        } // end ExternalAccountIsLinked()

        public static bool LinkExternalAccount(ExternalLoginInfo loginInfo)
        {
            if (loginInfo == null) return false;
            var result = GetUserManager().LinkExternalAccount(loginInfo);
            return true;
        }

        public static bool LinkExternalAccount()
        {
            var loginInfo = GetSignInManager().GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                throw new ApplicationException("External Login Failure: External Login Info was null!");
            }
            // Sign in the user with this external login provider if the user already has a login
            var success = LinkExternalAccount(loginInfo.Result);
            return success;
        }

        public static bool ExternalSignIn(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            var result = GetSignInManager()
                .ExternalLoginSignInAsync(loginInfo.LoginProvider, loginInfo.ProviderKey, isPersistent);
            return HandleLoginResult(result.Result, loginInfo);
        }

        private static bool HandleLoginResult(SignInResult result, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username), "Username is empty!");

            var log = LogManager.GetLogger(typeof(IdentityHelper));

            if (result.Succeeded)
            {
                var context = Web.GetContext();

                GetUserManager().SignInClient(username,
                    context.Request.Headers["User-Agent"].ToString(),
                    context.Connection.RemoteIpAddress.ToString(),
                    context.Session != null ? context.Session.Id : string.Empty);
                // Hubs.EventsHub.RaiseSignIn(loginInfo.DefaultUserName, DateTime.UtcNow);
                return true;
            }

            if (result.IsLockedOut)
            {
                log.Error("User is locked out!");
                return false;
            }

            if (result.IsNotAllowed)
            {
                log.Error("Is Not Allowed!");
                return false;
            }

            if (result.RequiresTwoFactor)
            {
                log.Error("Requires Verification!");
                throw new NotImplementedException();
            }

            if (!result.Succeeded)
            {
                log.Error("Failure to Login!");
                return false;
            }

            throw new ArgumentOutOfRangeException(nameof(result));
        }

        private static bool HandleLoginResult(SignInResult result, ExternalLoginInfo loginInfo)
        {
            var log = LogManager.GetLogger(typeof(IdentityHelper));

            if (result.Succeeded)
            {
                var context = Web.GetContext();

                //GetUserManager().SignInClient(loginInfo,
                //    context.Request.Headers["User-Agent"].ToString(),
                //    context.Connection.RemoteIpAddress.ToString(),
                //    context.Session != null ? context.Session.Id : string.Empty);

                //GetUserManager().SignInClient(loginInfo,
                //    HttpContext.Current.Request.Browser.Type,
                //    HttpContext.Current.Request.UserHostAddress,
                //    HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : string.Empty);
                //Hubs.EventsHub.RaiseSignIn(loginInfo.DefaultUserName, DateTime.UtcNow);
                return true;
            }

            if (result.IsLockedOut)
            {
                log.Error("User is locked out!");
                return false;
            }

            if (result.IsNotAllowed)
            {
                log.Error("Is Not Allowed!");
                return false;
            }

            if (result.RequiresTwoFactor)
            {
                log.Error("Requires Verification!");
                throw new NotImplementedException();
            }

            if (!result.Succeeded)
            {
                log.Error("Failure to Login!");
                return false;
            }

            throw new ArgumentOutOfRangeException(nameof(result));
        }

        public static ApplicationUserExternalProfile GetExternalProfile()
        {
            return null;
        }

        #endregion
    }

    public class UserProfileDTO
    {
        [JsonConstructor]
        public UserProfileDTO() { }

        public UserProfileDTO(ApplicationUser original)
        {
            UserName = original?.UserName;
            Name = original?.Name;
            Email = original?.Email;
            PhoneNumber = original?.PhoneNumber;

            SetRoles(original?.Roles);
            SetPermissions(original?.Permissions);
        }

        private void SetRoles(IEnumerable<ApplicationRole> originalRoles)
        {
            roles = originalRoles?.Select(role => new IdentityApplicationRoleDTO(role)).ToList();
        }

        private void SetPermissions(IEnumerable<ApplicationPermission> originalPermissions)
        {
            permissions = originalPermissions?.Select(permission => new IdentityApplicationPermissionDTO(permission)).ToList();
        }

        [DataMember(Name = "UserName")]
        protected string userName = string.Empty;
        [DataMember(Name = "Name")]
        protected string name;
        [DataMember(Name = "Email")]
        protected string email;
        [DataMember(Name = "PhoneNumber")]
        protected string phoneNumber;

        [DataMember(Name = "Roles")]
        protected List<IdentityApplicationRoleDTO> roles;

        public List<IdentityApplicationRoleDTO> Roles
        {
            get
            {
                return roles;
            }
            set
            {
                roles = value;
            }
        }

        [DataMember(Name = "Permissions")]
        protected List<IdentityApplicationPermissionDTO> permissions;

        public List<IdentityApplicationPermissionDTO> Permissions
        {
            get
            {
                return permissions;
            }
            set
            {
                permissions = value;
            }
        }

        public string Name
        {
            get;
            set;
        }
        public string Email
        {
            get;
            set;
        }
        public string UserName
        {
            get;
            set;
        }
        public string PhoneNumber
        {
            get;
            set;
        }
    }

    public class IdentityApplicationRoleDTO
    {
        [JsonConstructor]
        public IdentityApplicationRoleDTO() { }

        public IdentityApplicationRoleDTO(ApplicationRole original)
        {
            Name = original?.Name;
            Description = original?.Description;
            if (original?.IsCustom != null) IsCustom = (original?.IsCustom).Value;
        }

        public string Name
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public bool IsCustom
        {
            get;
            set;
        }
    }

    public class IdentityApplicationPermissionDTO
    {
        [JsonConstructor]
        public IdentityApplicationPermissionDTO() { }

        public IdentityApplicationPermissionDTO(ApplicationPermission original)
        {
            Name = original?.Name;
            Description = original?.Description;
            if (original?.IsCustom != null) IsCustom = (original?.IsCustom).Value;
        }

        public string Name
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        public bool IsCustom
        {
            get;
            set;
        }
    }
}
#endif