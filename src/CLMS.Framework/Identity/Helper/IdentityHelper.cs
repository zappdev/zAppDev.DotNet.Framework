#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Web;
using log4net;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Threading.Tasks;
using CLMS.Framework.Data;
using Newtonsoft.Json;
using System.Security.Authentication;
using System.Collections.Concurrent;
using CLMS.Framework.Identity.Model;
using CLMS.Framework.Utilities;
using CLMS.Framework.Hubs;

namespace CLMS.Framework.Identity
{
    public static class IdentityHelper
    {
        public static bool AllowMultipleSessionsPerUser = true;
        public static bool AdminCanResetPassword = true;

        public static ConcurrentDictionary<string, string> ActiveSessions = new ConcurrentDictionary<string, string>();
        
        // Used for XSRF when linking external logins
        public const string XsrfKey = "XsrfId";

        public static UserManager GetUserManager()
        {
            return GetSignInManager().UserManager as UserManager;
        }

        public static SignInManager<IdentityUser, string> GetSignInManager()
        {
            return HttpContext.Current?.GetOwinContext()?.Get<SignInManager<IdentityUser, string>>();
        }

        public static bool SignIn(string username, string password, bool isPersistent)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(username)) return false;
            if (!AllowMultipleSessionsPerUser && UserHasSession(username))
            {
                OnDuplicateSessionDetected(username, throwException: false);
				return false;
            }
            var signInManager = GetSignInManager();
            var result = signInManager.PasswordSignIn(username, password, isPersistent, true);
            return HandleLoginResult(result, username);
        }

        private static void SignIn(ExternalLoginInfo loginInfo, bool isPersistent)
        {
            if (!AllowMultipleSessionsPerUser && UserHasSession(loginInfo.DefaultUserName))
            {
                OnDuplicateSessionDetected(loginInfo.DefaultUserName, throwException: true);
            }
            var signInManager = GetSignInManager();
            var result = signInManager.ExternalSignIn(loginInfo, isPersistent);
            var loggedIn = HandleLoginResult(result, loginInfo);
            if (!loggedIn)
            {
                throw new ApplicationException($"Could not login with external login: {loginInfo.DefaultUserName}, provider: {loginInfo.Login.LoginProvider}");
            }
        }
        public static void AddUserSession(string username)
        {
            var sessionId = HttpContext.Current?.Session?.SessionID;
			if (string.IsNullOrWhiteSpace(sessionId)) return;
            if (ActiveSessions.ContainsKey(sessionId)) return;
			ActiveSessions.TryAdd(sessionId, username);
        }
        public static void RemoveUserSession(string sessionId)
        {
			if (string.IsNullOrWhiteSpace(sessionId)) return;
            sessionId = sessionId.Replace("SessionStateStoreProvider#", "");
            ActiveSessions.TryRemove(sessionId, out _);
        }
        public static bool UserHasSession(string username)
        {
            var connectedUsernames = ActiveSessions.Values;
            return connectedUsernames.Contains(username);
        }
        public static bool UserHasAnotherSession(string username, string sessionId)
        {
            var activeSessions = ActiveSessions.Where(s => s.Value == username);                                    
            return activeSessions.Where(x => x.Key != sessionId).Any();
        }
        private static void OnDuplicateSessionDetected(string username, bool throwException)
        {                        
			var msg = $"More than one active sessions detected for user: [{username}], session id [{HttpContext.Current?.Session?.SessionID}]";
			if (throwException) 
			{
				throw new AuthenticationException(msg);
			}
            else 
			{
				LogManager.GetLogger(typeof(IdentityHelper)).Error(msg);                        
            }
        }

        public static void SignOut()
        {
            var id = HttpContext.Current.User.Identity.GetUserId();
            if (!string.IsNullOrWhiteSpace(id))
            {
                var clientKey = HttpContext.Current.Request.Browser.Type;
                var manager = GetUserManager();
                var user = manager.FindById(id);
                manager.SignOutClient(user, clientKey);
            }
            var signInManager = GetSignInManager();
            signInManager.AuthenticationManager.SignOut(
                DefaultAuthenticationTypes.ApplicationCookie,
                DefaultAuthenticationTypes.ExternalBearer,
                DefaultAuthenticationTypes.ExternalCookie,
                DefaultAuthenticationTypes.TwoFactorCookie,
                DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            RemoveUserSession(HttpContext.Current?.Session?.SessionID);

            ServiceLocator.Current.GetInstance<IApplicationHub>()?.RaiseSignOutEvent(HttpContext.Current.User.Identity.Name, DateTime.UtcNow);
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

        public static IdentityUser GetIdentityUserByName(string userName)
        {
            var manager = GetUserManager();
            return manager.FindByName(userName);
        }

        public static IdentityUser GetCurrentIdentityUser()
        {
            if (!HttpContext.Current.GetOwinContext().Request.User.Identity.IsAuthenticated) return null;
            var manager = GetUserManager();
            return manager.FindById(HttpContext.Current.GetOwinContext().Request.User.Identity.Name);
        }

        public static ApplicationUser GetCurrentApplicationUser()
        {
            return GetCurrentIdentityUser()?.User;
        }

		public static List<ApplicationUser> GetAllConnectedUsers()
        {
            var connectedUsernames = ServiceLocator.Current.GetInstance<IApplicationHub>()?.GetAllConnectedUsers();
            if (connectedUsernames?.Any() != true) return new List<ApplicationUser>();

            using (var manager = new MiniSessionManager())
            {
                var repo = ServiceLocator.Current
                   .GetInstance<Data.DAL.IRepositoryBuilder>()
                   .CreateCreateRepository(manager);

                return repo.Get<ApplicationUser>(x => connectedUsernames.Contains(x.UserName));
            }
        }
        public static string GetCurrentUserName()
        {
            return HttpContext.Current.GetOwinContext().Request.User.Identity.Name;
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
            var identity = HttpContext.Current.GetOwinContext().Request?.User?.Identity;
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
            var result = await manager.PasswordValidator.ValidateAsync(password);
            return result.Succeeded;
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

        public static string CreateUser(ApplicationUser appUser)
        {
            var manager = GetUserManager();
            return CreateUser(manager, appUser);
        }

        public static string CreateUser(ApplicationUser appUser, string password)
        {
            var manager = GetUserManager();
            return CreateUser(manager, appUser, password);
        }

        public static string CreateUser(UserManager manager, ApplicationUser appUser)
        {
            var user = new IdentityUser(appUser);
            var result = manager.Create(user);
            return result.Succeeded ? null : result.Errors.First();
        }

        public static string CreateUser(UserManager manager, ApplicationUser appUser, string password)
        {
            var user = new IdentityUser(appUser);
            var result = manager.Create(user, password);
            return result.Succeeded ? null : result.Errors.First();
        }

        private static IdentityUser GetIdentityUser(string username, string email, string name, string userClass = null)
        {
            IdentityUser defaultUser = new IdentityUser(new ApplicationUser { UserName = username, Email = email, Name = name });
            try
            {
                if(!string.IsNullOrWhiteSpace(userClass) && userClass != "ApplicationUser")
                {
                    var unwrappedUser = Activator.CreateInstance(@"CLMS.Framework", $@"CLMS.Framework.Identity.Model.{userClass}").Unwrap() as ApplicationUser;
                    unwrappedUser.UserName = username;
                    unwrappedUser.Email = email;
                    unwrappedUser.Name = name;
                    return new IdentityUser(unwrappedUser);
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
            var loginInfo = HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfo();
            if (loginInfo == null)
            {
                //RedirectOnFail(HttpContext.Current.Response);
                return new[] {"No External Login information found."};
            }
            return CreateAndLoginUser(username, email, name, loginInfo.ExternalIdentity.Claims, loginInfo, userClass);
        }

        private static string RaiseExternalUserCreating(IdentityUser user)
        {
            try
            {
                ServiceLocator.Current.GetInstance<IApplicationHub>()?
                    .RaiseExternalUserCreatingEvent(user.User);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(IdentityHelper)).Error($"Exception Caught when calling the [ExternalUserCreating] event for user [{user.User.UserName}]: {e.Message}\r\n(StackTrace: {e.StackTrace})");
                return e.Message;
            }
            return null;
        }

        public static string[] CreateAndLoginUser(string username, string email, string name, IEnumerable<Claim> claims, ExternalLoginInfo externalLoginInfo, string userClass = null)
        {
            var manager = GetUserManager();
            var user = GetIdentityUser(username, email, name, userClass);
            foreach (var claim in claims)
            {
                user.User.AddClaims(new ApplicationUserClaim
                {
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    ClaimValueType = claim.ValueType
                });
            }
            string raiseExternalUserCreatingResult = RaiseExternalUserCreating(user);
            if (!string.IsNullOrWhiteSpace(raiseExternalUserCreatingResult))
            {
                HttpContext.Current?.Response?.AddHeader(nameof(AuthenticationException), raiseExternalUserCreatingResult);
                return new string[] { raiseExternalUserCreatingResult };
            }
            var _savedUser = manager.FindByName(user.User.UserName);
            if (_savedUser == null)
            {
                var msg = $"Application User {user.User.UserName} was not created";
				HttpContext.Current?.Response?.AddHeader(nameof(AuthenticationException), msg);
				return new string[] { msg };
            }
            var result = manager.AddLogin(user.Id, externalLoginInfo.Login);
            if (result.Succeeded)
            {
                SignIn(externalLoginInfo, isPersistent: false);
                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // var code = manager.GenerateEmailConfirmationToken(user.Id);
                // Send this link via email: IdentityHelper.GetUserConfirmationRedirectUrl(code, user.Id)
                //IdentityHelper.RedirectToReturnUrl(returnUrl, HttpContext.Current.Response);
                return new string[0]; //successful login
            }
            return result.Errors.ToArray();
        }

        public static bool ValidateExternalLoginAndLogin(out ApplicationUser appUser, out IEnumerable<string> errors)
        {
            errors = new List<string>();
            appUser = null;
            // Get external login info
            var manager = GetUserManager();
            var loginInfo = HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfo();
            if (loginInfo == null)
            {
                //RedirectOnFail(HttpContext.Current.Response);
                errors = new[] {"No External Login information found."};
                return false;
            }
            // Find local user associated with the external login
            var user = manager.Find(loginInfo.Login);
            if (user != null)
            {
                // If local user exists -> Login
                SignIn(loginInfo, isPersistent: false);
                appUser = user.User;
                return true;
                //IdentityHelper.RedirectToReturnUrl(HttpContext.Current.Request.QueryString["ReturnUrl"], HttpContext.Current.Response);
            }
            if (HttpContext.Current.User.Identity.IsAuthenticated)
            {
                // If the external login is not associated with the currently logged in User -> associate
                // Apply Xsrf check when linking
                loginInfo = HttpContext.Current.GetOwinContext().Authentication.GetExternalLoginInfo(XsrfKey, HttpContext.Current.User.Identity.GetUserId());
                if (loginInfo == null)
                {
                    errors = new[] {"No External Login information found."};
                    return false;
                }
                var id = HttpContext.Current.User.Identity.GetUserId();
                var addLoginResult = manager.AddLogin(id, loginInfo.Login);
                if (addLoginResult.Succeeded)
                {
                    // If successfully associated the user with the external login -> redirect to ReturnUrl
                    //IdentityHelper.RedirectToReturnUrl(HttpContext.Current.Request.QueryString["ReturnUrl"], HttpContext.Current.Response);
                    appUser = manager.Find(loginInfo.Login).User;
                    return true;
                }
                // Failure to associate -> Return any Errors (result is already populated)
                errors = addLoginResult.Errors;
                return false;
            }
            // If not local user exists and no user is already logged in in this session -> return to UI to get more info
            errors = new[] {"No local user exists. Please create your local user by providing the following details."};
            appUser = new ApplicationUser
            {
                Email = loginInfo.Email,
                UserName = loginInfo.DefaultUserName,
                Name = loginInfo.DefaultUserName,
                Claims = new List<ApplicationUserClaim>()
            };
            foreach (var claim in loginInfo.ExternalIdentity.Claims)
            {
                appUser.AddClaims(new ApplicationUserClaim
                {
                    ClaimType = claim.Type,
                    ClaimValue = claim.Value,
                    ClaimValueType = claim.ValueType,
                    Issuer = claim.Issuer,
                    OriginalIssuer = claim.OriginalIssuer,
                });
            }
            return false;
        }

        public static ClaimsPrincipal CreateLocalUserForWindowsIdentity(ClaimsPrincipal incomingPrincipal)
        {
            var log = LogManager.GetLogger(typeof (IdentityHelper));
            var localLoginClaim = incomingPrincipal.FindFirst(Model.ClaimTypes.LocalLogin);
            if (localLoginClaim != null && localLoginClaim.Value == bool.TrueString)
            {
                log.DebugFormat("Identity {0}, has local login and has been processed.", incomingPrincipal.Identity.Name);
                return incomingPrincipal;
            }
            log.DebugFormat("Identity {0}, has not been processed.", incomingPrincipal.Identity.Name);
            // Find local user associated with the windows login
            var manager = GetUserManager();
            var user = manager.FindByName(incomingPrincipal.Identity.Name);
            if (user == null)
            {
                log.DebugFormat("Identity {0}, does not have local login. Creating...", incomingPrincipal.Identity.Name);
                var externalLoginInfo = new ExternalLoginInfo
                {
                    Login = new UserLoginInfo("NTLM", incomingPrincipal.Identity.Name),
                    DefaultUserName = incomingPrincipal.Identity.Name,
                    Email = incomingPrincipal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value,
                    ExternalIdentity = incomingPrincipal.Identities.FirstOrDefault()
                };
                var emailClaim = incomingPrincipal.FindFirst(System.Security.Claims.ClaimTypes.Email);
                var nameClaim = incomingPrincipal.FindFirst(System.Security.Claims.ClaimTypes.GivenName);
                var errors = CreateAndLoginUser(incomingPrincipal.Identity.Name,
                                                emailClaim != null ? emailClaim.Value : string.Empty,
                                                nameClaim != null ? nameClaim.Value : string.Empty,
                                                incomingPrincipal.Claims,
                                                externalLoginInfo,
                                                null);
                if (errors.Any())
                {
                    throw new ApplicationException("Could not create local user!\r\n" + string.Join("\r\n", errors));
                }
                user = manager.FindByName(incomingPrincipal.Identity.Name);
                if(user == null) return null;
            }
            else
            {
                log.Debug($"User {incomingPrincipal.Identity.Name} already has local login.");
            }
            PrintUser(user, log);
            var clientKey = HttpContext.Current.Request.Browser.Type;
            if (user.User.Clients.Count(c => c.ClientKey == clientKey) == 0)
            {
                manager.SignInClient(user,
                                     clientKey,
                                     HttpContext.Current.Request.UserHostAddress,
                                     HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : string.Empty);
            }
            log.DebugFormat("Identity {0}, has local login. Adding Permission and Role Claims...", incomingPrincipal.Identity.Name);
            var newIdentity = new ClaimsIdentity("Negotiate");
            newIdentity.AddClaims(incomingPrincipal.Claims);
            newIdentity.AddClaims(GetCurrentApplicationUserPermissions().Select(p=> new Claim(Model.ClaimTypes.Permission, p)));
            newIdentity.AddClaims(user.User.Roles.Select(r => new Claim(System.Security.Claims.ClaimTypes.Role, r.Name)));
            newIdentity.AddClaim(new Claim(Model.ClaimTypes.LocalLogin, bool.TrueString));
            return new ClaimsPrincipal(newIdentity);
        }

        private static void PrintUser(IdentityUser user, ILog log)
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

        public const string ProviderNameKey = "providerName";

        public static string GetProviderNameFromRequest(HttpRequest request)
        {
            return request[ProviderNameKey];
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
            response.RedirectToRoute((HttpContext.Current.User.Identity.IsAuthenticated) ? "Home" : "SignIn");
        }

        public static void RedirectToReturnUrl(string returnUrl, HttpResponse response)
        {
            if (!String.IsNullOrEmpty(returnUrl) && IsLocalUrl(returnUrl))
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
                var result = manager.Create(new IdentityUser(applicationUser)); //, ApplicationUser.PasswordHash);
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Could not create User. {string.Join("\r\n", result.Errors.ToArray())}");
                }
            }
            else
            {
                var manager = GetUserManager();
                var result = manager.Update(new IdentityUser(applicationUser));
                if (!result.Succeeded)
                {
                    throw new ApplicationException($"Could not update User. {string.Join("\r\n", result.Errors.ToArray())}");
                }
            }
        }

        public static string ChangePassword(ApplicationUser user, string currentPassword, string newPassword)
        {
            var manager = GetUserManager();
            var result = manager.ChangePassword(user.UserName, currentPassword, newPassword);
            return result.Succeeded ? null : result.Errors.First();
        }

        public static string ResetPassword(ApplicationUser user, string newPassword)
        {
            var manager = GetUserManager();
            var result = manager.RemovePassword(user.UserName);
            if (result.Succeeded)
            {
                result = manager.AddPassword(user.UserName, newPassword);
            }
            return result.Succeeded ? null : result.Errors.First();
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

		public static void SignOutUserFromAllSessions(ApplicationUser user)
        {
            if (user == null) return;
            var activeSessionIds = ActiveSessions.Where(s => s.Value == user.UserName).Select(x => x.Key);
            foreach (var id in activeSessionIds)
            {
                RemoveUserSession(id);
            }
            ServiceLocator.Current.GetInstance<IApplicationHub>()?.ForceUserPageReloadEvent(user.UserName);
            InvalidateUserSecurityData(user);
        }
        public static void InvalidateUserSecurityData(ApplicationUser user)
        {
            if (user == null)
            {
                return;
            }
            using (var manager = new MiniSessionManager())
            {
                manager.OpenSessionWithTransaction();
                var repo = ServiceLocator.Current
                    .GetInstance<Data.DAL.IRepositoryBuilder>()
                    .CreateCreateRepository(manager);
                user.SecurityStamp = Guid.NewGuid().ToString();
                repo.Save(user);
                manager.CommitChanges();
            }
        }

        public static void LogAction(string controller, string action, bool success, string errorMessage)
        {
            using (var manager = new MiniSessionManager())
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
                    string.Compare(logSetting.Value, "false", true) == 0 ||
                    string.Compare(logSetting.Value, "0", true) == 0
                )
                    return;
                ApplicationUser user = null;
                List<string> activePermissions = new List<string>();
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

#region External Account Functions

        public static bool ExternalAccountIsLinked(string loginProvider, ApplicationUser user = null)
        {
            if (string.IsNullOrWhiteSpace(loginProvider))
            {
                throw new ArgumentNullException(nameof(loginProvider));
            }
            if(user == null)
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
            var loginInfo = GetSignInManager().AuthenticationManager.GetExternalLoginInfoAsync();
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
            var result = GetSignInManager().ExternalSignInAsync(loginInfo, isPersistent);
            return HandleLoginResult(result.Result, loginInfo);
        }

        private static bool HandleLoginResult(SignInStatus result, string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentNullException(nameof(username), "Username is empty!");
            }
            var log = LogManager.GetLogger(typeof(IdentityHelper));
            switch (result)
            {
            case SignInStatus.Success:
                GetUserManager().SignInClient(username,
                                              HttpContext.Current.Request.Browser.Type,
                                              HttpContext.Current.Request.UserHostAddress,
                                              HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : string.Empty);
                AddUserSession(username);
                ServiceLocator.Current.GetInstance<IApplicationHub>()?
                    .RaiseSignInEvent(username, DateTime.UtcNow);
                return true;
            case SignInStatus.RequiresVerification:
                log.Error("Requires Verification!");
                throw new NotImplementedException();
            case SignInStatus.LockedOut:
                log.Error("User is locked out!");
                return false;
            case SignInStatus.Failure:
                log.Error("Failure to Login!");
                return false;
            default:
                throw new ArgumentOutOfRangeException();
            }
        }
        private static bool HandleLoginResult(SignInStatus result, ExternalLoginInfo loginInfo)
        {
            var log = LogManager.GetLogger(typeof(IdentityHelper));
            switch (result)
            {
            case SignInStatus.Success:
                GetUserManager().SignInClient(loginInfo,
                                              HttpContext.Current.Request.Browser.Type,
                                              HttpContext.Current.Request.UserHostAddress,
                                              HttpContext.Current.Session != null ? HttpContext.Current.Session.SessionID : string.Empty);
                AddUserSession(loginInfo.DefaultUserName);
                ServiceLocator.Current.GetInstance<IApplicationHub>()?.RaiseSignInEvent(loginInfo.DefaultUserName, DateTime.UtcNow);
                return true;
            case SignInStatus.RequiresVerification:
                log.Error("Requires Verification!");
                throw new NotImplementedException();
            case SignInStatus.LockedOut:
                log.Error("User is locked out!");
                return false;
            case SignInStatus.Failure:
                log.Error("Failure to Login!");
                return false;
            default:
                return HandleLoginResult(result, loginInfo.DefaultUserName);
            }
        }

        public static ApplicationUserExternalProfile GetExternalProfile()
        {
            var profile = new ApplicationUserExternalProfile();
            var loginInfo = GetSignInManager().AuthenticationManager.GetExternalLoginInfoAsync();
            var claims = new List<Claim>();
            if (loginInfo != null && loginInfo.Result != null)
            {
                var info = loginInfo.Result;
                claims = info.ExternalIdentity?.Claims?.ToList();
                profile.Provider = info.Login.LoginProvider;
                profile.Email = info.Email;
            }
            else
            {
                claims = GetSignInManager().AuthenticationManager.User?.Claims?.ToList();
            }
            if(claims == null || !claims.Any())
            {
                return null;
            }
            profile.Name = claims.FirstOrDefault(x => x.Type == Model.ClaimTypes.Name)?.Value;
            profile.Surname = claims.FirstOrDefault(x => x.Type == Model.ClaimTypes.Surname)?.Value;
            profile.DisplayName = claims.FirstOrDefault(x => x.Type == Model.ClaimTypes.DisplayName)?.Value;
            var gender = claims.FirstOrDefault(x => x.Type == Model.ClaimTypes.Gender)?.Value;
            if (string.IsNullOrWhiteSpace(gender))
                profile.Gender = "";
            else if (string.Compare(gender, "male", true) == 0)
                profile.Gender = "Male";
            else if (string.Compare(gender, "female", true) == 0)
                profile.Gender = "Female";
            else
                profile.Gender = "Other";
            if(string.IsNullOrWhiteSpace(profile.Provider)) profile.Provider = claims.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Issuer))?.Issuer;
            if(string.IsNullOrWhiteSpace(profile.Email))    profile.Email = claims.FirstOrDefault(x => x.Type == Model.ClaimTypes.Email)?.Value;
            return profile;
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