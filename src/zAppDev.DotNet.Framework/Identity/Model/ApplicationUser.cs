// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.Serialization;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using zAppDev.DotNet.Framework.Linq;
using zAppDev.DotNet.Framework.Data.Domain;
using zAppDev.DotNet.Framework.Workflow;
using zAppDev.DotNet.Framework.Exceptions;
using zAppDev.DotNet.Framework.Profiling;

namespace zAppDev.DotNet.Framework.Identity.Model
{
    /// <summary>
    /// The ApplicationUser class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public class ApplicationUser : IDomainModelClass
    {
        #region ApplicationUser's Fields

        protected Guid _transientId= Guid.NewGuid();
        public virtual Guid TransientId
        {
            get
            {
                return _transientId;
            }
            set
            {
                _transientId = value;
            }
        }
        [DataMember(Name="UserName")]
        protected string userName = string.Empty;
        [DataMember(Name="PasswordHash")]
        protected string passwordHash;
        [DataMember(Name="SecurityStamp")]
        protected string securityStamp;
        [DataMember(Name="EmailConfirmed")]
        protected bool emailConfirmed;
        [DataMember(Name="LockoutEnabled")]
        protected bool lockoutEnabled;
        [DataMember(Name="PhoneNumberConfirmed")]
        protected bool phoneNumberConfirmed;
        [DataMember(Name="TwoFactorEnabled")]
        protected bool twoFactorEnabled;
        [DataMember(Name="AccessFailedCount")]
        protected int? accessFailedCount;
        [DataMember(Name="Name")]
        protected string name;
        [DataMember(Name="Email")]
        protected string email;
        [DataMember(Name="PhoneNumber")]
        protected string phoneNumber;
        [DataMember(Name="LockoutEndDate")]
        protected DateTime? lockoutEndDate;
        [DataMember(Name="VersionTimestamp")]
        protected int? versionTimestamp;

#pragma warning disable 0649
        private bool disableInternalAdditions;
#pragma warning restore 0649
        #endregion
        #region ApplicationUser's Properties
/// <summary>
/// The UserName property
///
/// </summary>
///
        [Key]
        public virtual string UserName
        {
            get
            {
                return userName;
            }
            set
            {
                userName = value;
            }
        }
/// <summary>
/// The PasswordHash property
///
/// </summary>
///
        public virtual string PasswordHash
        {
            get
            {
                return passwordHash;
            }
            set
            {
                passwordHash = value;
            }
        }
/// <summary>
/// The SecurityStamp property
///
/// </summary>
///
        public virtual string SecurityStamp
        {
            get
            {
                return securityStamp;
            }
            set
            {
                securityStamp = value;
            }
        }
/// <summary>
/// The EmailConfirmed property
///
/// </summary>
///
        public virtual bool EmailConfirmed
        {
            get
            {
                return emailConfirmed;
            }
            set
            {
                emailConfirmed = value;
            }
        }
/// <summary>
/// The LockoutEnabled property
///
/// </summary>
///
        public virtual bool LockoutEnabled
        {
            get
            {
                return lockoutEnabled;
            }
            set
            {
                lockoutEnabled = value;
            }
        }
/// <summary>
/// The PhoneNumberConfirmed property
///
/// </summary>
///
        public virtual bool PhoneNumberConfirmed
        {
            get
            {
                return phoneNumberConfirmed;
            }
            set
            {
                phoneNumberConfirmed = value;
            }
        }
/// <summary>
/// The TwoFactorEnabled property
///
/// </summary>
///
        public virtual bool TwoFactorEnabled
        {
            get
            {
                return twoFactorEnabled;
            }
            set
            {
                twoFactorEnabled = value;
            }
        }
/// <summary>
/// The AccessFailedCount property
///
/// </summary>
///
        public virtual int? AccessFailedCount
        {
            get
            {
                return accessFailedCount;
            }
            set
            {
                accessFailedCount = value;
            }
        }
/// <summary>
/// The Name property
///
/// </summary>
///
        public virtual string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
            }
        }
/// <summary>
/// The Email property
///
/// </summary>
///
        public virtual string Email
        {
            get
            {
                return email;
            }
            set
            {
                email = value;
            }
        }
/// <summary>
/// The PhoneNumber property
///
/// </summary>
///
        public virtual string PhoneNumber
        {
            get
            {
                return phoneNumber;
            }
            set
            {
                phoneNumber = value;
            }
        }
/// <summary>
/// The LockoutEndDate property
///
/// </summary>
///
        public virtual DateTime? LockoutEndDate
        {
            get
            {
                return lockoutEndDate;
            }
            set
            {
                lockoutEndDate = value;
            }
        }
        /// <summary>
        /// The VersionTimestamp property
        ///Provides concurrency control for the class
        /// </summary>
        ///
        public virtual int? VersionTimestamp
        {
            get
            {
                return versionTimestamp;
            }
            set
            {
                versionTimestamp = value;
            }
        }
        #endregion
        #region ApplicationUser's Participant Properties
        [DataMember(Name="Permissions")]
        protected IList<ApplicationPermission> permissions = new List<ApplicationPermission>();
        public virtual List<ApplicationPermission> Permissions
        {
            get
            {
                if (permissions is ApplicationPermission[])
                {
                    permissions = permissions.ToList();
                }
                if (permissions == null)
                {
                    permissions = new List<ApplicationPermission>();
                }
                return permissions.ToList();
            }
            set
            {
                if (permissions is ApplicationPermission[])
                {
                    permissions = permissions.ToList();
                }
                if (permissions != null)
                {
                    var __itemsToDelete = new List<ApplicationPermission>(permissions);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemovePermissions(__item);
                    }
                }
                if(value == null)
                {
                    permissions = new List<ApplicationPermission>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddPermissions(__item);
                }
            }
        }
        public virtual void AddPermissions(IList<ApplicationPermission> __items)
        {
            foreach (var __item in __items)
            {
                AddPermissions(__item);
            }
        }

        public virtual void InternalAddPermissions(ApplicationPermission __item)
        {
            if (__item == null || disableInternalAdditions) return;
            permissions?.Add(__item);
        }

        public virtual void InternalRemovePermissions(ApplicationPermission __item)
        {
            if (__item == null) return;
            permissions?.Remove(__item);
        }

        public virtual void AddPermissions(ApplicationPermission __item)
        {
            if (__item == null) return;
            if (!permissions.Contains(__item))
                InternalAddPermissions(__item);
            if (!__item.Users.Contains(this))
                __item.AddUsers(this);
        }

        public virtual void AddAtIndexPermissions(int index, ApplicationPermission __item)
        {
            if (__item == null) return;
            if (!permissions.Contains(__item))
                permissions.Insert(index, __item);
            if (!__item.Users.Contains(this))
                __item.AddUsers(this);
        }

        public virtual void RemovePermissions(ApplicationPermission __item)
        {
            if (__item != null)
            {
                if (permissions.Contains(__item))
                    InternalRemovePermissions(__item);
                if(__item.Users.Contains(this))
                    __item.RemoveUsers(this);
            }
        }
        public virtual void SetPermissionsAt(ApplicationPermission __item, int __index)
        {
            if (__item == null)
            {
                permissions[__index].RemoveUsers(this);
            }
            else
            {
                permissions[__index] = __item;
                if (!__item.Users.Contains(this))
                    __item.AddUsers(this);
            }
        }

        public virtual void ClearPermissions()
        {
            if (permissions!=null)
            {
                var __itemsToRemove = permissions.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemovePermissions(__item);
                }
            }
        }
        [DataMember(Name="Roles")]
        protected IList<ApplicationRole> roles = new List<ApplicationRole>();
        public virtual List<ApplicationRole> Roles
        {
            get
            {
                if (roles is ApplicationRole[])
                {
                    roles = roles.ToList();
                }
                if (roles == null)
                {
                    roles = new List<ApplicationRole>();
                }
                return roles.ToList();
            }
            set
            {
                if (roles is ApplicationRole[])
                {
                    roles = roles.ToList();
                }
                if (roles != null)
                {
                    var __itemsToDelete = new List<ApplicationRole>(roles);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveRoles(__item);
                    }
                }
                if(value == null)
                {
                    roles = new List<ApplicationRole>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddRoles(__item);
                }
            }
        }
        public virtual void AddRoles(IList<ApplicationRole> __items)
        {
            foreach (var __item in __items)
            {
                AddRoles(__item);
            }
        }

        public virtual void InternalAddRoles(ApplicationRole __item)
        {
            if (__item == null || disableInternalAdditions) return;
            roles?.Add(__item);
        }

        public virtual void InternalRemoveRoles(ApplicationRole __item)
        {
            if (__item == null) return;
            roles?.Remove(__item);
        }

        public virtual void AddRoles(ApplicationRole __item)
        {
            if (__item == null) return;
            if (!roles.Contains(__item))
                InternalAddRoles(__item);
            if (!__item.Users.Contains(this))
                __item.AddUsers(this);
        }

        public virtual void AddAtIndexRoles(int index, ApplicationRole __item)
        {
            if (__item == null) return;
            if (!roles.Contains(__item))
                roles.Insert(index, __item);
            if (!__item.Users.Contains(this))
                __item.AddUsers(this);
        }

        public virtual void RemoveRoles(ApplicationRole __item)
        {
            if (__item != null)
            {
                if (roles.Contains(__item))
                    InternalRemoveRoles(__item);
                if(__item.Users.Contains(this))
                    __item.RemoveUsers(this);
            }
        }
        public virtual void SetRolesAt(ApplicationRole __item, int __index)
        {
            if (__item == null)
            {
                roles[__index].RemoveUsers(this);
            }
            else
            {
                roles[__index] = __item;
                if (!__item.Users.Contains(this))
                    __item.AddUsers(this);
            }
        }

        public virtual void ClearRoles()
        {
            if (roles!=null)
            {
                var __itemsToRemove = roles.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveRoles(__item);
                }
            }
        }
        [DataMember(Name="Clients")]
        protected IList<ApplicationClient> clients = new List<ApplicationClient>();
        public virtual List<ApplicationClient> Clients
        {
            get
            {
                if (clients is ApplicationClient[])
                {
                    clients = clients.ToList();
                }
                if (clients == null)
                {
                    clients = new List<ApplicationClient>();
                }
                return clients.ToList();
            }
            set
            {
                if (clients is ApplicationClient[])
                {
                    clients = clients.ToList();
                }
                if (clients != null)
                {
                    var __itemsToDelete = new List<ApplicationClient>(clients);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveClients(__item);
                    }
                }
                if(value == null)
                {
                    clients = new List<ApplicationClient>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddClients(__item);
                }
            }
        }
        public virtual void AddClients(IList<ApplicationClient> __items)
        {
            foreach (var __item in __items)
            {
                AddClients(__item);
            }
        }

        public virtual void InternalAddClients(ApplicationClient __item)
        {
            if (__item == null || disableInternalAdditions) return;
            clients?.Add(__item);
        }

        public virtual void InternalRemoveClients(ApplicationClient __item)
        {
            if (__item == null) return;
            clients?.Remove(__item);
        }

        public virtual void AddClients(ApplicationClient __item)
        {
            if (__item == null) return;
            if (__item.User != this)
                __item.User = this;
        }

        public virtual void AddAtIndexClients(int index, ApplicationClient __item)
        {
            if (__item == null) return;
            clients?.Insert(index, __item);
            disableInternalAdditions = true;
            try
            {
                if (__item.User != this)
                    __item.User = this;
            }
            finally
            {
                disableInternalAdditions = false;
            }
        }

        public virtual void RemoveClients(ApplicationClient __item)
        {
            if (__item != null)
            {
                __item.User = null;
            }
        }
        public virtual void SetClientsAt(ApplicationClient __item, int __index)
        {
            if (__item == null)
            {
                clients[__index].User = null;
            }
            else
            {
                clients[__index] = __item;
                if (__item.User != this)
                    __item.User = this;
            }
        }

        public virtual void ClearClients()
        {
            if (clients!=null)
            {
                var __itemsToRemove = clients.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveClients(__item);
                }
            }
        }
        [DataMember(Name="Logins")]
        protected IList<ApplicationUserLogin> logins = new List<ApplicationUserLogin>();
        public virtual List<ApplicationUserLogin> Logins
        {
            get
            {
                if (logins is ApplicationUserLogin[])
                {
                    logins = logins.ToList();
                }
                if (logins == null)
                {
                    logins = new List<ApplicationUserLogin>();
                }
                return logins.ToList();
            }
            set
            {
                if (logins is ApplicationUserLogin[])
                {
                    logins = logins.ToList();
                }
                if (logins != null)
                {
                    var __itemsToDelete = new List<ApplicationUserLogin>(logins);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveLogins(__item);
                    }
                }
                if(value == null)
                {
                    logins = new List<ApplicationUserLogin>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddLogins(__item);
                }
            }
        }
        public virtual void AddLogins(IList<ApplicationUserLogin> __items)
        {
            foreach (var __item in __items)
            {
                AddLogins(__item);
            }
        }

        public virtual void InternalAddLogins(ApplicationUserLogin __item)
        {
            if (__item == null || disableInternalAdditions) return;
            logins?.Add(__item);
        }

        public virtual void InternalRemoveLogins(ApplicationUserLogin __item)
        {
            if (__item == null) return;
            logins?.Remove(__item);
        }

        public virtual void AddLogins(ApplicationUserLogin __item)
        {
            if (__item == null) return;
            if (__item.User != this)
                __item.User = this;
        }

        public virtual void AddAtIndexLogins(int index, ApplicationUserLogin __item)
        {
            if (__item == null) return;
            logins?.Insert(index, __item);
            disableInternalAdditions = true;
            try
            {
                if (__item.User != this)
                    __item.User = this;
            }
            finally
            {
                disableInternalAdditions = false;
            }
        }

        public virtual void RemoveLogins(ApplicationUserLogin __item)
        {
            if (__item != null)
            {
                __item.User = null;
            }
        }
        public virtual void SetLoginsAt(ApplicationUserLogin __item, int __index)
        {
            if (__item == null)
            {
                logins[__index].User = null;
            }
            else
            {
                logins[__index] = __item;
                if (__item.User != this)
                    __item.User = this;
            }
        }

        public virtual void ClearLogins()
        {
            if (logins!=null)
            {
                var __itemsToRemove = logins.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveLogins(__item);
                }
            }
        }
        [DataMember(Name="Claims")]
        protected IList<ApplicationUserClaim> claims = new List<ApplicationUserClaim>();
        public virtual List<ApplicationUserClaim> Claims
        {
            get
            {
                if (claims is ApplicationUserClaim[])
                {
                    claims = claims.ToList();
                }
                if (claims == null)
                {
                    claims = new List<ApplicationUserClaim>();
                }
                return claims.ToList();
            }
            set
            {
                if (claims is ApplicationUserClaim[])
                {
                    claims = claims.ToList();
                }
                if (claims != null)
                {
                    var __itemsToDelete = new List<ApplicationUserClaim>(claims);
                    foreach (var __item in __itemsToDelete)
                    {
                        RemoveClaims(__item);
                    }
                }
                if(value == null)
                {
                    claims = new List<ApplicationUserClaim>();
                    return;
                }
                foreach(var __item in value)
                {
                    AddClaims(__item);
                }
            }
        }
        public virtual void AddClaims(IList<ApplicationUserClaim> __items)
        {
            foreach (var __item in __items)
            {
                AddClaims(__item);
            }
        }

        public virtual void InternalAddClaims(ApplicationUserClaim __item)
        {
            if (__item == null || disableInternalAdditions) return;
            claims?.Add(__item);
        }

        public virtual void InternalRemoveClaims(ApplicationUserClaim __item)
        {
            if (__item == null) return;
            claims?.Remove(__item);
        }

        public virtual void AddClaims(ApplicationUserClaim __item)
        {
            if (__item == null) return;
            if (__item.User != this)
                __item.User = this;
        }

        public virtual void AddAtIndexClaims(int index, ApplicationUserClaim __item)
        {
            if (__item == null) return;
            claims?.Insert(index, __item);
            disableInternalAdditions = true;
            try
            {
                if (__item.User != this)
                    __item.User = this;
            }
            finally
            {
                disableInternalAdditions = false;
            }
        }

        public virtual void RemoveClaims(ApplicationUserClaim __item)
        {
            if (__item != null)
            {
                __item.User = null;
            }
        }
        public virtual void SetClaimsAt(ApplicationUserClaim __item, int __index)
        {
            if (__item == null)
            {
                claims[__index].User = null;
            }
            else
            {
                claims[__index] = __item;
                if (__item.User != this)
                    __item.User = this;
            }
        }

        public virtual void ClearClaims()
        {
            if (claims!=null)
            {
                var __itemsToRemove = claims.ToList();
                foreach(var __item in __itemsToRemove)
                {
                    RemoveClaims(__item);
                }
            }
        }
        [DataMember(Name="Profile")]
        protected Profile profile;
        public virtual Profile Profile
        {
            get
            {
                return profile;
            }
            set
            {
                if(Equals(profile, value)) return;
                var __oldValue = profile;
                if (value != null)
                {
                    profile = value;
                }
                else
                {
                    if (profile != null)
                    {
                        profile = null;
                    }
                }
            }
        }
        #endregion
        #region Constructors
/// <summary>
/// Public constructors of the ApplicationUser class
/// </summary>
/// <returns>New ApplicationUser object</returns>
/// <remarks></remarks>
        public ApplicationUser() {}
        #endregion
        #region Methods

        public virtual List<string> _Validate(bool throwException = true)
        {
            var __errors = new List<string>();
            if (UserName == null)
            {
                __errors.Add("Property 'UserName' is required.");
            }
            if (UserName != null && string.IsNullOrWhiteSpace(UserName))
            {
                __errors.Add("String 'UserName' cannot be empty.");
            }
            if (UserName != null && UserName.Length > 256)
            {
                __errors.Add("Length of property 'UserName' cannot be greater than 256.");
            }
            if (PasswordHash != null && PasswordHash.Length > 2147483647)
            {
                __errors.Add("Length of property 'PasswordHash' cannot be greater than 2147483647.");
            }
            if (SecurityStamp != null && SecurityStamp.Length > 2147483647)
            {
                __errors.Add("Length of property 'SecurityStamp' cannot be greater than 2147483647.");
            }
            if (Name != null && Name.Length > 256)
            {
                __errors.Add("Length of property 'Name' cannot be greater than 256.");
            }
            if (Email != null && Email.Length > 255)
            {
                __errors.Add("Length of property 'Email' cannot be greater than 255.");
            }
            if (PhoneNumber != null && PhoneNumber.Length > 255)
            {
                __errors.Add("Length of property 'PhoneNumber' cannot be greater than 255.");
            }
            if (throwException && __errors.Any())
            {
                throw new BusinessException("An instance of TypeClass 'ApplicationUser' has validation errors:\r\n\r\n" + string.Join("\r\n", __errors));
            }
            return __errors;
        }

        public virtual int _GetUniqueIdentifier()
        {
            var hashCode = 399326290;
            hashCode = hashCode * -1521134295 + (UserName?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PasswordHash?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (SecurityStamp?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (EmailConfirmed.GetHashCode() );
            hashCode = hashCode * -1521134295 + (LockoutEnabled.GetHashCode() );
            hashCode = hashCode * -1521134295 + (PhoneNumberConfirmed.GetHashCode() );
            hashCode = hashCode * -1521134295 + (TwoFactorEnabled.GetHashCode() );
            hashCode = hashCode * -1521134295 + (AccessFailedCount?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Name?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (Email?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (PhoneNumber?.GetHashCode() ?? 0);
            hashCode = hashCode * -1521134295 + (LockoutEndDate?.GetHashCode() ?? 0);
            return hashCode;
        }






/// <summary>
/// Copies the current object to a new instance
/// </summary>
/// <param name="deep">Copy members that refer to objects external to this class (not dependent)</param>
/// <param name="copiedObjects">Objects that should be reused</param>
/// <param name="asNew">Copy the current object as a new one, ready to be persisted, along all its members.</param>
/// <param name="reuseNestedObjects">If asNew is true, this flag if set, forces the reuse of all external objects.</param>
/// <param name="copy">Optional - An existing [ApplicationUser] instance to use as the destination.</param>
/// <returns>A copy of the object</returns>
        public virtual ApplicationUser Copy(bool deep=false, Hashtable copiedObjects=null, bool asNew=false, bool reuseNestedObjects = false, ApplicationUser copy = null)
        {
            if(copiedObjects == null)
            {
                copiedObjects = new Hashtable();
            }
            if (copy == null && copiedObjects.Contains(this))
                return (ApplicationUser)copiedObjects[this];
            copy = copy ?? new ApplicationUser();
            if (!asNew)
            {
                copy.TransientId = this.TransientId;
                copy.UserName = this.UserName;
            }
            copy.PasswordHash = this.PasswordHash;
            copy.SecurityStamp = this.SecurityStamp;
            copy.EmailConfirmed = this.EmailConfirmed;
            copy.LockoutEnabled = this.LockoutEnabled;
            copy.PhoneNumberConfirmed = this.PhoneNumberConfirmed;
            copy.TwoFactorEnabled = this.TwoFactorEnabled;
            copy.AccessFailedCount = this.AccessFailedCount;
            copy.Name = this.Name;
            copy.Email = this.Email;
            copy.PhoneNumber = this.PhoneNumber;
            copy.LockoutEndDate = this.LockoutEndDate;
            if (!copiedObjects.Contains(this))
            {
                copiedObjects.Add(this, copy);
            }
            copy.permissions = new List<ApplicationPermission>();
            if(deep && this.permissions != null)
            {
                foreach (var __item in this.permissions)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddPermissions(__item);
                        else
                            copy.AddPermissions(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddPermissions((ApplicationPermission)copiedObjects[__item]);
                    }
                }
            }
            copy.roles = new List<ApplicationRole>();
            if(deep && this.roles != null)
            {
                foreach (var __item in this.roles)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddRoles(__item);
                        else
                            copy.AddRoles(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddRoles((ApplicationRole)copiedObjects[__item]);
                    }
                }
            }
            copy.clients = new List<ApplicationClient>();
            if(deep && this.clients != null)
            {
                foreach (var __item in this.clients)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddClients(__item);
                        else
                            copy.AddClients(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddClients((ApplicationClient)copiedObjects[__item]);
                    }
                }
            }
            copy.logins = new List<ApplicationUserLogin>();
            if(deep && this.logins != null)
            {
                foreach (var __item in this.logins)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddLogins(__item);
                        else
                            copy.AddLogins(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddLogins((ApplicationUserLogin)copiedObjects[__item]);
                    }
                }
            }
            copy.claims = new List<ApplicationUserClaim>();
            if(deep && this.claims != null)
            {
                foreach (var __item in this.claims)
                {
                    if (!copiedObjects.Contains(__item))
                    {
                        if (asNew && reuseNestedObjects)
                            copy.AddClaims(__item);
                        else
                            copy.AddClaims(__item.Copy(deep, copiedObjects, asNew));
                    }
                    else
                    {
                        copy.AddClaims((ApplicationUserClaim)copiedObjects[__item]);
                    }
                }
            }
            if(deep && this.profile != null)
            {
                if (!copiedObjects.Contains(this.profile))
                {
                    if (asNew && reuseNestedObjects)
                        copy.Profile = this.Profile;
                    else if (asNew)
                        copy.Profile = this.Profile.Copy(deep, copiedObjects, true);
                    else
                        copy.profile = this.profile.Copy(deep, copiedObjects, false);
                }
                else
                {
                    if (asNew)
                        copy.Profile = (Profile)copiedObjects[this.Profile];
                    else
                        copy.profile = (Profile)copiedObjects[this.Profile];
                }
            }
            return copy;
        }

        public override bool Equals(object obj)
        {
            var compareTo = obj as ApplicationUser;
            if (ReferenceEquals(this, compareTo))
            {
                return true;
            }
            if (compareTo == null || !this.GetType().Equals(compareTo.GetTypeUnproxied()))
            {
                return false;
            }
            if (this.HasSameNonDefaultIdAs(compareTo))
            {
                return true;
            }
            // Since the Ids aren't the same, both of them must be transient to
            // compare domain signatures; because if one is transient and the
            // other is a persisted entity, then they cannot be the same object.
            return this.IsTransient() && compareTo.IsTransient() && (base.Equals(compareTo) || this.TransientId.Equals(compareTo.TransientId));
        }

// Maintain equality operator semantics for entities.
        public static bool operator ==(ApplicationUser x, ApplicationUser y)
        {
            // By default, == and Equals compares references. In order to
            // maintain these semantics with entities, we need to compare by
            // identity value. The Equals(x, y) override is used to guard
            // against null values; it then calls EntityEquals().
            return Equals(x, y);
        }

// Maintain inequality operator semantics for entities.
        public static bool operator !=(ApplicationUser x, ApplicationUser y)
        {
            return !(x == y);
        }

        private PropertyInfo __propertyKeyCache;
        public virtual PropertyInfo GetPrimaryKey()
        {
            if (__propertyKeyCache == null)
            {
                __propertyKeyCache = this.GetType().GetProperty("UserName");
            }
            return __propertyKeyCache;
        }


/// <summary>
///     To help ensure hashcode uniqueness, a carefully selected random number multiplier
///     is used within the calculation.  Goodrich and Tamassia's Data Structures and
///     Algorithms in Java asserts that 31, 33, 37, 39 and 41 will produce the fewest number
///     of collissions.  See http://computinglife.wordpress.com/2008/11/20/why-do-hash-functions-use-prime-numbers/
///     for more information.
/// </summary>
        private const int HashMultiplier = 31;
        private int? cachedHashcode;

        public override int GetHashCode()
        {
            if (this.cachedHashcode.HasValue)
            {
                return this.cachedHashcode.Value;
            }
            if (this.IsTransient())
            {
                //this.cachedHashcode = base.GetHashCode();
                return this.TransientId.GetHashCode(); //don't cache because this won't stay transient forever
            }
            else
            {
                unchecked
                {
                    // It's possible for two objects to return the same hash code based on
                    // identically valued properties, even if they're of two different types,
                    // so we include the object's type in the hash calculation
                    var hashCode = this.GetType().GetHashCode();
                    this.cachedHashcode = (hashCode * HashMultiplier) ^ this.UserName.GetHashCode();
                }
            }
            return this.cachedHashcode.Value;
        }

/// <summary>
///     Transient objects are not associated with an item already in storage.  For instance,
///     a Customer is transient if its Id is 0.  It's virtual to allow NHibernate-backed
///     objects to be lazily loaded.
/// </summary>
        public virtual bool IsTransient()
        {
            return string.IsNullOrEmpty(this.UserName);
        }

/// <summary>
///     When NHibernate proxies objects, it masks the type of the actual entity object.
///     This wrapper burrows into the proxied object to get its actual type.
///
///     Although this assumes NHibernate is being used, it doesn't require any NHibernate
///     related dependencies and has no bad side effects if NHibernate isn't being used.
///
///     Related discussion is at http://groups.google.com/group/sharp-architecture/browse_thread/thread/ddd05f9baede023a ...thanks Jay Oliver!
/// </summary>
        protected virtual System.Type GetTypeUnproxied()
        {
            return this.GetType();
        }

/// <summary>
///     Returns true if self and the provided entity have the same Id values
///     and the Ids are not of the default Id value
/// </summary>
        protected bool HasSameNonDefaultIdAs(ApplicationUser compareTo)
        {
            return !this.IsTransient() && !compareTo.IsTransient() && this.UserName.Equals(compareTo.UserName);
        }

        #endregion

        public virtual bool IsInRole(string roleName)
        {
#if NETFRAMEWORK
            using (new Profiler(nameof(ApplicationUser), Profiling.AppDevSymbolType.ClassOperation, nameof(ApplicationUser.IsInRole)))
            {
                return Roles?.Any((r) => r.Name == roleName) ?? false;
            }
#else
            return Roles?.Any((r) => r.Name == roleName) ?? false;
#endif
        }

        public virtual bool HasPermission(string permission)
        {
#if NETFRAMEWORK
            using (new Profiler(nameof(ApplicationUser), Profiling.AppDevSymbolType.ClassOperation, nameof(ApplicationUser.HasPermission)))
            {
                bool hasPermissionfromRoles = (Roles?.Any((rr) => rr.Permissions.Any((pp) => pp.Name == permission)) ?? false);
                return hasPermissionfromRoles || (Permissions?.Any((pp) => pp.Name == permission) ?? false);
            }
#else
            bool hasPermissionfromRoles = (Roles?.Any((rr) => rr.Permissions.Any((pp) => pp.Name == permission)) ?? false);
            return hasPermissionfromRoles || (Permissions?.Any((pp) => pp.Name == permission) ?? false);
#endif
        }

    }
}
