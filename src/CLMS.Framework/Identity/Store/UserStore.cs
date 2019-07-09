#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using NHibernate;
using NHibernate.Linq;
using CLMS.Framework.Data;
using NHibernate.Criterion;
using CLMS.Framework.Identity.Model;
using CLMS.Framework.Utilities;

namespace CLMS.Framework.Identity
{
    /// <summary>
    /// Summary description for UserStore
    /// </summary>
    public class UserStore :
        IUserStore<IdentityUser>,
        IUserPasswordStore<IdentityUser>,
        IUserClaimStore<IdentityUser>,
        IUserSecurityStampStore<IdentityUser>,
        IUserLoginStore<IdentityUser>,
        IQueryableUserStore<IdentityUser>,
        IUserLockoutStore<IdentityUser, string>,
        IUserEmailStore<IdentityUser>,
        IUserPhoneNumberStore<IdentityUser>,
        IUserTwoFactorStore<IdentityUser, string>
    {
        private readonly MiniSessionManager _sessionManager;
        private readonly Data.DAL.IRepositoryBuilder _repositoryBuilder;

        public UserStore(MiniSessionManager sessionManager, Data.DAL.IRepositoryBuilder repositoryBuilder = null)
        {
            _sessionManager = sessionManager;
            _repositoryBuilder = repositoryBuilder;
        }

        private ISession GetCurrentSession()
        {
            // Make sure session is open
            _sessionManager.OpenSession();
            return _sessionManager.Session;
        }

        private ApplicationUser PersistApplicationUser(ApplicationUser user)
        {
            if (MiniSessionManager.TryGetInstance() == null)
            {
                MiniSessionManager.ExecuteInUoW(manager =>
                {
                    _repositoryBuilder
                        .CreateCreateRepository()
                        .Save(user);
                }, _sessionManager);
            }
            else
            {
                _repositoryBuilder
                    .CreateCreateRepository(_sessionManager)
                    .Save(user);
            }
            return user;
        }

        private ApplicationUser RetrieveApplicationUser(string username, Expression<Func<ApplicationUser, bool>> filter = null)
        {
            ApplicationUser user;
            if (filter == null)
            {
                user = GetCurrentSession()
                       .CreateCriteria(typeof(ApplicationUser))
                       .Add(new IdentifierEqExpression(username))
                       .SetCacheable(true)
                       .UniqueResult<ApplicationUser>();
            }
            else
            {
                user = GetCurrentSession().Query<ApplicationUser>().WithOptions(options => options.SetCacheable(true)).SingleOrDefault(filter);
            }
            return user;
        }

        public void Dispose()
        {
        }

        public System.Threading.Tasks.Task CreateAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if(user.User.Profile == null) user.User.Profile = new Profile();
            user.User = PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        public System.Threading.Tasks.Task UpdateAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User = PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        public System.Threading.Tasks.Task DeleteAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var repo = _repositoryBuilder.CreateIdentityRepository(_sessionManager);
            repo.DeleteApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<object>(null);
        }

        public System.Threading.Tasks.Task<IdentityUser> FindByIdAsync(string userId)
        {
            var appUser = RetrieveApplicationUser(userId);
            return appUser != null ? System.Threading.Tasks.Task.FromResult(new IdentityUser(appUser)) : System.Threading.Tasks.Task.FromResult<IdentityUser>(null);
        }

        public System.Threading.Tasks.Task<IdentityUser> FindByNameAsync(string userName)
        {
            return FindByIdAsync(userName);
        }

        public System.Threading.Tasks.Task SetPasswordHashAsync(IdentityUser user, string passwordHash)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.PasswordHash = passwordHash;
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task<string> GetPasswordHashAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult(user.User.PasswordHash);
        }

        public System.Threading.Tasks.Task<bool> HasPasswordAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult(string.IsNullOrWhiteSpace(user.User.PasswordHash));
        }

        public System.Threading.Tasks.Task<IList<Claim>> GetClaimsAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var result = (IList<Claim>)new List<Claim>();
            var appUser = RetrieveApplicationUser(user.Id);
            if (appUser != null)
            {
                foreach (var claim in appUser.Claims)
                {
                    result.Add(new Claim(claim.ClaimType, claim.ClaimValue, claim.ClaimValueType, claim.Issuer, claim.OriginalIssuer));
                }
                foreach (var permission in appUser.Permissions)
                {
                    result.Add(new Claim(Model.ClaimTypes.Permission, permission.Name));
                }
                foreach (var role in appUser.Roles)
                {
                    result.Add(new Claim(System.Security.Claims.ClaimTypes.Role, role.Name));
                }
            }
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public System.Threading.Tasks.Task AddClaimAsync(IdentityUser user, Claim claim)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            user.User.AddClaims(new ApplicationUserClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                ClaimValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer
            });
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task RemoveClaimAsync(IdentityUser user, Claim claim)
        {
            if ((object)user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (claim == null)
            {
                throw new ArgumentNullException(nameof(claim));
            }
            var claimsToRemove = user.User.Claims.Where(a => a.ClaimType == claim.Type
                                 && a.ClaimValue == claim.Value).ToList();
            foreach (var c in claimsToRemove)
            {
                user.User.Claims.Remove(c);
            }
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task SetSecurityStampAsync(IdentityUser user, string stamp)
        {
            user.User.SecurityStamp = stamp;
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task<string> GetSecurityStampAsync(IdentityUser user)
        {
            if ((object)user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            else
            {
                return System.Threading.Tasks.Task.FromResult<string>(user.User.SecurityStamp);
            }
        }

        public System.Threading.Tasks.Task AddLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            user.User.AddLogins(new ApplicationUserLogin { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey });
            user.User = PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task RemoveLoginAsync(IdentityUser user, UserLoginInfo login)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            var entry = user.User.Logins.SingleOrDefault(l => l.LoginProvider == provider && l.ProviderKey == key);
            if (entry != null)
            {
                user.User.RemoveLogins(entry);
                user.User = PersistApplicationUser(user.User);
            }
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            IList<UserLoginInfo> result = user.User.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey)).ToList();
            return System.Threading.Tasks.Task.FromResult(result);
        }

        public System.Threading.Tasks.Task<IdentityUser> FindAsync(UserLoginInfo login)
        {
            if (login == null)
            {
                throw new ArgumentNullException(nameof(login));
            }
            var appUser = RetrieveApplicationUser(string.Empty,
                                                  user => user.Logins.Any(a => a.LoginProvider == login.LoginProvider && a.ProviderKey == login.ProviderKey));
            return appUser != null ? System.Threading.Tasks.Task.FromResult(new IdentityUser(appUser)) : System.Threading.Tasks.Task.FromResult<IdentityUser>(null);
        }

        public IQueryable<IdentityUser> Users
        {
            get
            {
                return GetCurrentSession().Query<ApplicationUser>()
                       .WithOptions(options => options.SetCacheable(true))
                       .ToList()
                       .Select(a=> new IdentityUser(a)).AsQueryable();
            }
        }

        public System.Threading.Tasks.Task<DateTimeOffset> GetLockoutEndDateAsync(IdentityUser user)
        {
            DateTimeOffset dateTimeOffset;
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            if (user.User.LockoutEndDate.HasValue)
            {
                var lockoutEndDateUtc = user.User.LockoutEndDate;
                dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDateUtc.Value, DateTimeKind.Utc));
            }
            else
            {
                dateTimeOffset = new DateTimeOffset();
            }
            return System.Threading.Tasks.Task.FromResult(dateTimeOffset);
        }

        public System.Threading.Tasks.Task SetLockoutEndDateAsync(IdentityUser user, DateTimeOffset lockoutEnd)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            var nullable = lockoutEnd == DateTimeOffset.MinValue ? null : new DateTime?(lockoutEnd.UtcDateTime);
            user.User.LockoutEndDate = nullable;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task<int> IncrementAccessFailedCountAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.AccessFailedCount = user.User.AccessFailedCount.GetValueOrDefault() + 1;
            return System.Threading.Tasks.Task.FromResult<int>(user.User.AccessFailedCount.GetValueOrDefault());
        }

        public System.Threading.Tasks.Task ResetAccessFailedCountAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.AccessFailedCount = 0;
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task<int> GetAccessFailedCountAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<int>(user.User.AccessFailedCount.GetValueOrDefault());
        }

        public System.Threading.Tasks.Task<bool> GetLockoutEnabledAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<bool>(user.User.LockoutEnabled);
        }

        public System.Threading.Tasks.Task SetLockoutEnabledAsync(IdentityUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.LockoutEnabled = enabled;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task SetEmailAsync(IdentityUser user, string email)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.Email = email;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task<string> GetEmailAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<string>(user.User.Email);
        }

        public System.Threading.Tasks.Task<bool> GetEmailConfirmedAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<bool>(user.User.EmailConfirmed);
        }

        public System.Threading.Tasks.Task SetEmailConfirmedAsync(IdentityUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.EmailConfirmed = confirmed;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task<IdentityUser> FindByEmailAsync(string email)
        {
            var appUser = RetrieveApplicationUser("", user => string.Equals(user.Email, email, StringComparison.CurrentCultureIgnoreCase));
            return System.Threading.Tasks.Task.FromResult(new IdentityUser(appUser));
        }

        public System.Threading.Tasks.Task SetPhoneNumberAsync(IdentityUser user, string phoneNumber)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.PhoneNumber = phoneNumber;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult<int>(0);
        }

        public System.Threading.Tasks.Task<string> GetPhoneNumberAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<string>(user.User.PhoneNumber);
        }

        public System.Threading.Tasks.Task<bool> GetPhoneNumberConfirmedAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<bool>(user.User.PhoneNumberConfirmed);
        }

        public System.Threading.Tasks.Task SetPhoneNumberConfirmedAsync(IdentityUser user, bool confirmed)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.PhoneNumberConfirmed = confirmed;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task SetTwoFactorEnabledAsync(IdentityUser user, bool enabled)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            user.User.TwoFactorEnabled = enabled;
            PersistApplicationUser(user.User);
            return System.Threading.Tasks.Task.FromResult(0);
        }

        public System.Threading.Tasks.Task<bool> GetTwoFactorEnabledAsync(IdentityUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return System.Threading.Tasks.Task.FromResult<bool>(user.User.TwoFactorEnabled);
        }
    }
}
#endif