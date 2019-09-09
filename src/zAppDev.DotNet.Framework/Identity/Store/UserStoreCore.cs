// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using zAppDev.DotNet.Framework.Data;
using Microsoft.AspNetCore.Identity;
using NHibernate;
using NHibernate.Linq;
using zAppDev.DotNet.Framework.Identity.Model;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Identity
{
    /// <summary>
    /// Summary description for UserStore
    /// </summary>
    public class UserStore :
        IUserPasswordStore<Model.IdentityUser>,
        IUserClaimStore<Model.IdentityUser>,
        IUserSecurityStampStore<Model.IdentityUser>,
        IUserLoginStore<Model.IdentityUser>,
        IQueryableUserStore<Model.IdentityUser>,
        IUserLockoutStore<Model.IdentityUser>,
        IUserEmailStore<Model.IdentityUser>,
        IUserPhoneNumberStore<Model.IdentityUser>,
        IUserTwoFactorStore<Model.IdentityUser>
    {
        private readonly IMiniSessionService _sessionManager;
        private bool _disposed;

        public UserStore(IMiniSessionService sessionManager)
        {
            _sessionManager = sessionManager;
        }

        private ISession GetCurrentSession()
        {
            _sessionManager.OpenSession();
            return _sessionManager.Session;
        }

        private ApplicationUser PersistApplicationUser(ApplicationUser user)
        {
            using (var manager = new MiniSessionService(_sessionManager.SessionFactory))
            {
                manager.OpenSessionWithTransaction();
                ServiceLocator.Current
                    .GetInstance<Data.DAL.IRepositoryBuilder>()
                    .CreateCreateRepository(manager)
                    .Save(user);
                manager.CommitChanges();
                return user;
            }
        }

        private ApplicationUser RetrieveApplicationUser(string username, Expression<Func<ApplicationUser, bool>> filter = null)
        {
            if (filter == null)
            {
                filter = user => user.UserName == username;
                //return GetCurrentSession().Get<BO.ApplicationUser>(username);
            }
            return GetCurrentSession().Query<ApplicationUser>()
                .WithOptions(options => options.SetCacheable(false))
                .SingleOrDefault(filter);
        }

        public void Dispose()
        {
            _disposed = true;
        }

        public Task<IdentityResult> CreateAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (user.User.Profile == null) user.User.Profile = new Profile();
            user.User = PersistApplicationUser(user.User);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> UpdateAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User = PersistApplicationUser(user.User);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<IdentityResult> DeleteAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var repo =
                ServiceLocator.Current
                    .GetInstance<Data.DAL.IRepositoryBuilder>()
                    .CreateIdentityRepository(_sessionManager);

            repo.DeleteApplicationUser(user.User);
            return Task.FromResult(IdentityResult.Success);
        }

        public Task<Model.IdentityUser> FindByIdAsync(string userId, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var appUser = RetrieveApplicationUser(userId);
            return appUser != null ? Task.FromResult(new Model.IdentityUser(appUser)) : Task.FromResult<Model.IdentityUser>(null);
        }

        public Task<Model.IdentityUser> FindByNameAsync(string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            return FindByIdAsync(userName, cancellationToken);
        }

        public Task SetPasswordHashAsync(Model.IdentityUser user, string passwordHash, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.PasswordHash = passwordHash;
            return Task.FromResult(0);
        }

        public Task<string> GetPasswordHashAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.PasswordHash);
        }

        public Task<bool> HasPasswordAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(string.IsNullOrWhiteSpace(user.User.PasswordHash));
        }

        public Task<IList<Claim>> GetClaimsAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var result = (IList<Claim>)new List<Claim>();
            var appUser = RetrieveApplicationUser(user.Id);

            if (appUser == null) return Task.FromResult(result);

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

            return Task.FromResult(result);
        }

        public Task SetSecurityStampAsync(Model.IdentityUser user, string stamp, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            user.User.SecurityStamp = stamp;
            return Task.FromResult(0);
        }

        public Task<string> GetSecurityStampAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.SecurityStamp);
        }

        public Task AddLoginAsync(Model.IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            user.User.AddLogins(new ApplicationUserLogin { LoginProvider = login.LoginProvider, ProviderKey = login.ProviderKey });
            user.User = PersistApplicationUser(user.User);
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(Model.IdentityUser user, UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (login == null)
                throw new ArgumentNullException(nameof(login));

            var provider = login.LoginProvider;
            var key = login.ProviderKey;
            var entry = user.User.Logins.SingleOrDefault(l => l.LoginProvider == provider && l.ProviderKey == key);

            if (entry != null)
            {
                user.User.RemoveLogins(entry);
                user.User = PersistApplicationUser(user.User);
            }

            return Task.FromResult(0);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            IList<UserLoginInfo> result = user.User.Logins.Select(l => new UserLoginInfo(l.LoginProvider, l.ProviderKey, l.Id?.ToString())).ToList();
            return Task.FromResult(result);
        }

        public Task<Model.IdentityUser> FindAsync(UserLoginInfo login, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (login == null)
                throw new ArgumentNullException(nameof(login));

            var appUser = RetrieveApplicationUser(string.Empty,
                                                  user => user.Logins.Any(a => a.LoginProvider == login.LoginProvider && a.ProviderKey == login.ProviderKey));
            return appUser != null ? Task.FromResult(new Model.IdentityUser(appUser)) : Task.FromResult<Model.IdentityUser>(null);
        }

        public IQueryable<Model.IdentityUser> Users
        {
            get
            {
                return GetCurrentSession().Query<ApplicationUser>()
                       .WithOptions(options => options.SetCacheable(true))
                       .ToList()
                       .Select(a => new Model.IdentityUser(a)).AsQueryable();
            }
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            DateTimeOffset dateTimeOffset;

            if (user.User.LockoutEndDate.HasValue)
            {
                var lockoutEndDateUtc = user.User.LockoutEndDate;
                dateTimeOffset = new DateTimeOffset(DateTime.SpecifyKind(lockoutEndDateUtc.Value, DateTimeKind.Utc));
            }
            else
            {
                dateTimeOffset = new DateTimeOffset();
            }

            return Task.FromResult<DateTimeOffset?>(dateTimeOffset);
        }

        public Task SetLockoutEndDateAsync(Model.IdentityUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.LockoutEndDate = (lockoutEnd == null)
                ? default(DateTime?)
                : DateTime.SpecifyKind(lockoutEnd.Value.DateTime, DateTimeKind.Utc);
            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.AccessFailedCount = user.User.AccessFailedCount.GetValueOrDefault() + 1;
            return Task.FromResult(user.User.AccessFailedCount.GetValueOrDefault());
        }

        public Task ResetAccessFailedCountAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.AccessFailedCount = 0;
            return Task.FromResult(0);
        }

        public Task<int> GetAccessFailedCountAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.AccessFailedCount.GetValueOrDefault());
        }

        public Task<bool> GetLockoutEnabledAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.LockoutEnabled);
        }

        public Task SetLockoutEnabledAsync(Model.IdentityUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.LockoutEnabled = enabled;
            PersistApplicationUser(user.User);
            return Task.FromResult(0);
        }

        public Task SetEmailAsync(Model.IdentityUser user, string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.User.Email = email;
            PersistApplicationUser(user.User);

            return Task.FromResult(0);
        }

        public Task<string> GetEmailAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.EmailConfirmed);
        }

        public Task SetEmailConfirmedAsync(Model.IdentityUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.User.EmailConfirmed = confirmed;

            PersistApplicationUser(user.User);
            return Task.FromResult(0);
        }

        public Task<Model.IdentityUser> FindByEmailAsync(string email, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            var appUser = RetrieveApplicationUser("", user => string.Equals(user.Email, email, StringComparison.CurrentCultureIgnoreCase));

            return Task.FromResult(new Model.IdentityUser(appUser));
        }

        public Task SetPhoneNumberAsync(Model.IdentityUser user, string phoneNumber, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.PhoneNumber = phoneNumber;
            PersistApplicationUser(user.User);

            return Task.FromResult(0);
        }

        public Task<string> GetPhoneNumberAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            return Task.FromResult(user.User.PhoneNumberConfirmed);
        }

        public Task SetPhoneNumberConfirmedAsync(Model.IdentityUser user, bool confirmed, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.PhoneNumberConfirmed = confirmed;
            PersistApplicationUser(user.User);
            return Task.FromResult(0);
        }

        public Task SetTwoFactorEnabledAsync(Model.IdentityUser user, bool enabled, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            user.User.TwoFactorEnabled = enabled;
            PersistApplicationUser(user.User);

            return Task.FromResult(0);
        }

        public Task<bool> GetTwoFactorEnabledAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.User.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.User.UserName);
        }

        public Task<string> GetUserNameAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.UserName);
        }

        public Task SetUserNameAsync(Model.IdentityUser user, string userName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedUserNameAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task SetNormalizedUserNameAsync(Model.IdentityUser user, string normalizedName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(Model.IdentityUser user, string normalizedEmail, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task<string> GetNormalizedEmailAsync(Model.IdentityUser user, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task AddClaimsAsync(Model.IdentityUser user, IEnumerable<Claim> claims,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                AddClaimAsync(user, claim, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(Model.IdentityUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));
            if (newClaim == null)
                throw new ArgumentNullException(nameof(newClaim));

            user.User.RemoveClaims(new ApplicationUserClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                ClaimValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer
            });

            user.User.AddClaims(new ApplicationUserClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                ClaimValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer
            });

            throw new NotImplementedException();
        }

        public Task RemoveClaimsAsync(Model.IdentityUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                RemoveClaimAsync(user, claim, cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task<IList<Model.IdentityUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return Task.FromResult<IList<Model.IdentityUser>>(null);

        }

        public Task AddClaimAsync(Model.IdentityUser user, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            user.User.Claims.Add(new ApplicationUserClaim
            {
                ClaimType = claim.Type,
                ClaimValue = claim.Value,
                ClaimValueType = claim.ValueType,
                Issuer = claim.Issuer,
                OriginalIssuer = claim.OriginalIssuer
            });

            return Task.CompletedTask; ;
        }

        public Task RemoveClaimAsync(Model.IdentityUser user, Claim claim, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            var claimsToRemove = user.User.Claims.Where(a => a.ClaimType == claim.Type
                                                             && a.ClaimValue == claim.Value).ToList();
            foreach (var c in claimsToRemove)
            {
                user.User.Claims.Remove(c);
            }
            return Task.FromResult(0);
        }

        public Task RemoveLoginAsync(Model.IdentityUser user, string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (user == null)
                throw new ArgumentNullException(nameof(user));
            if (loginProvider == null)
                throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null)
                throw new ArgumentNullException(nameof(providerKey));

            var applicationLoginProvider = user.User.Logins
                .SingleOrDefault(login => login.LoginProvider == loginProvider && login.ProviderKey == providerKey);

            user.User.RemoveLogins(applicationLoginProvider);

            return Task.CompletedTask;
        }

        public Task<Model.IdentityUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();

            if (loginProvider == null)
                throw new ArgumentNullException(nameof(loginProvider));
            if (providerKey == null)
                throw new ArgumentNullException(nameof(providerKey));

            Expression<Func<ApplicationUserLogin, bool>> filter = login =>
                login.LoginProvider == loginProvider && login.ProviderKey == providerKey;

            var appUser = GetCurrentSession()
                .Query<ApplicationUserLogin>()
                .WithOptions(options => options.SetCacheable(true))
                .SingleOrDefault(filter);

            return appUser != null ? Task.FromResult(new Model.IdentityUser(appUser.User)) : Task.FromResult<Model.IdentityUser>(null);
        }

        protected void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

    }
}
#endif