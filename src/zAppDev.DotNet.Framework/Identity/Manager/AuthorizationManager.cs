// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Xml;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Data.Domain;

using log4net;
using NHibernate;
using NHibernate.Linq;
using zAppDev.DotNet.Framework.Data;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using System.IdentityModel.Services;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public class AuthorizationManager : ClaimsAuthorizationManager
    {
        private readonly Dictionary<ResourceAction, Func<ClaimsPrincipal, bool>> _policies = new Dictionary<ResourceAction, Func<ClaimsPrincipal, bool>>();
        private readonly PolicyReader _policyReader = new PolicyReader();

        public static bool AdministratorExists(string adminRoleName = "Administrator")
        {
            return MiniSessionManager.Instance.Session.Query<ApplicationUser>()
                   .Where(u => u.Roles.Any(r => r.Name == adminRoleName))
                   .WithOptions(options => options.SetCacheable(true))
                   .Any();
        }

        public static ApplicationUser GetFirstAdministratorUser(string adminRoleName = "Administrator")
        {
            return MiniSessionManager.Instance.Session.Query<ApplicationUser>()
                   .Where(u => u.Roles.Any(r => r.Name == adminRoleName))
                   .WithOptions(options => options.SetCacheable(true))
                   .FirstOrDefault();
        }

        /// <summary>
        /// Creates a new instance of the MyClaimsAuthorizationManager
        /// </summary>
        public AuthorizationManager()
        {
        }

        public static void InvalidateCache()
        {
            _operationsDictionary = null;
        }


        private readonly Object _operationsLock = new object();
        private static ConcurrentDictionary<string, List<ApplicationOperation>> _operationsDictionary = null;



        private ConcurrentDictionary<string, List<ApplicationOperation>> GetOperationsDictionary()
        {
            if (_operationsDictionary != null) return _operationsDictionary;
            GetOperationsInner();
            return _operationsDictionary;
        }

        public string GetOperationDictionaryKey(ApplicationOperation operation)
        {
            return
                GetOperationDictionaryKey(operation.Type,operation.Name,operation.ParentControllerName);
        }

        public string GetOperationDictionaryKey(string type, string name, string parentControllerName)
        {
            return type + "|" + name + "|" + parentControllerName;
        }

        private bool GetOperationsInner()
        {
            var w = System.Diagnostics.Stopwatch.StartNew();
            lock (_operationsLock)
            {
                if (_operationsDictionary != null)
                    return true;
                MiniSessionManager.Instance.OpenSession();
                // Load & Cache Operations
                var operations = MiniSessionManager.Instance.Session.Query<ApplicationOperation>()
                                 .FetchMany(a => a.Permissions)
                                 .WithOptions(options =>
                {
                    options.SetCacheable(true);
                    options.SetCacheRegion("security");
                })
                .ToList();
                // ThereBeDragonsHere! - Hack to lazy initialize the Roles
                var roles = operations.SelectMany(o => o.Permissions.Select(p => p.Roles)).ToList();
                var localOperationsDictionary = new ConcurrentDictionary<string, List<ApplicationOperation>>();
                foreach (var operation in operations)
                {
                    var key = GetOperationDictionaryKey(operation);
                    if (!localOperationsDictionary.ContainsKey(key))
                    {
                        var list = new List<ApplicationOperation>
                        {
                            operation
                        };
                        localOperationsDictionary.TryAdd(key, list);
                    }
                    else
                    {
                        var list = localOperationsDictionary[key];
                        if (!list.Contains(operation))
                        {
                            list.Add(operation);
                        }
                    }
                }
                _operationsDictionary = localOperationsDictionary;
            }
            w.Stop();
            LogManager.GetLogger(this.GetType()).Debug($"Got all Operations/Permissions/Roles in {w.Elapsed}");
            return true;
        }

        /// <summary>
        /// Overloads  the base class method to load the custom policies from the config file
        /// </summary>
        /// <param name="nodelist">XmlNodeList containing the policy information read from the config file</param>
        public override void LoadCustomConfiguration(XmlNodeList nodelist)
        {
            Expression<Func<ClaimsPrincipal, bool>> policyExpression;
            foreach (XmlNode node in nodelist)
            {
                //
                // Initialize the policy cache
                //
                using (var sr = new StringReader(node.OuterXml))
                {
                    using (var xtr = new XmlTextReader(sr))
                    {
                        //XmlDictionaryReader rdr =
                        //    XmlDictionaryReader.CreateDictionaryReader(new XmlTextReader(new StringReader(node.OuterXml)));
                        using (XmlDictionaryReader rdr = XmlDictionaryReader.CreateDictionaryReader(xtr))
                        {
                            rdr.MoveToContent();
                            string resource = rdr.GetAttribute("resource");
                            string action = rdr.GetAttribute("action");
                            policyExpression = _policyReader.ReadPolicy(rdr);
                            //
                            // Compile the policy expression into a function
                            //
                            Func<ClaimsPrincipal, bool> policy = policyExpression.Compile();
                            //
                            // Insert the policy function into the policy cache
                            //
                            _policies[new ResourceAction(resource, action)] = policy;
                        }
                    }
                }
            }
        }


        private bool IsValidSecurityStamp(UserManager manager, ClaimsIdentity identity, string stamp)
        {
            try
            {
                return (stamp == manager.GetSecurityStampAsync(identity.GetUserName()).Result);
            }
            catch (Exception x)
            {
                LogManager.GetLogger(this.GetType()).Debug("Error in IsValidSecurityStamp", x);
                return false;
            }
        }


        /// <summary>
        /// Checks if the principal specified in the authorization context is authorized to perform action specified in the authorization context
        /// on the specified resoure
        /// </summary>
        /// <param name="pec">Authorization context</param>
        /// <returns>true if authorized, false otherwise</returns>
        public override bool CheckAccess(AuthorizationContext pec)
        {
            return CheckAccess(pec, pec.Principal.Identity as ClaimsIdentity);
        }

        public bool CheckAccess(AuthorizationContext pec, ClaimsIdentity identity)
        {
            //
            // Evaluate the policy against the claims of the
            // principal to determine access
            //
            bool access;
            var firstResourceType = pec.Resource.First().Type;
            try
            {
                if (firstResourceType == Model.ClaimTypes.ControllerAction ||
                        firstResourceType == Model.ClaimTypes.Dataset ||
                        firstResourceType == Model.ClaimTypes.GenericAction ||
                        firstResourceType == Model.ClaimTypes.IDEF0Activity ||
                        firstResourceType == Model.ClaimTypes.Url ||
                        firstResourceType == Model.ClaimTypes.ExposedService)
                {
                    ConcurrentDictionary<string, List<ApplicationOperation>>
                    operationsDictionary = GetOperationsDictionary();
                    if (operationsDictionary == null)
                    {
                        throw new ApplicationException(
                            $"operationsDictionary == null, Parent: '{pec.Resource.First().Value}', Action: '{pec.Action.First().Value}', Type: '{firstResourceType}'");
                    }
                    //var ops = operations.Where(a => a.Type == firstResourceType
                    //                           && a.ParentControllerName == pec.Resource.First().Value
                    //                           && a.Name == pec.Action.First().Value).ToArray();
                    string key = GetOperationDictionaryKey(firstResourceType,
                                                           pec.Action.First().Value, pec.Resource.First().Value);
                    if (!operationsDictionary.ContainsKey(key))
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{pec.Resource.First().Value}` of `{firstResourceType}` - `{pec.Action.First().Value}` was not found.");
                        return true;
                    }
                    List<ApplicationOperation> applicationOperationsList = operationsDictionary[key];
                    if (applicationOperationsList.Count > 1)
                    {
                        throw new ApplicationException(
                            $"Duplicate Operations found! Parent: '{pec.Resource.First().Value}', Action: '{pec.Action.First().Value}', Type: '{firstResourceType}'");
                    }
                    if (applicationOperationsList.Count == 0)
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{pec.Resource.First().Value}` of `{firstResourceType}` - `{pec.Action.First().Value}` was not found.");
                        return true;
                    }
                    var op = applicationOperationsList[0];
                    if (op == null)
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{pec.Resource.First().Value}` of `{firstResourceType}` - `{pec.Action.First().Value}` was not found.");
                        return true;
                    }
                    var opPermissions = op.Permissions.Select(a => new Claim(Model.ClaimTypes.Permission, a.Name));
                    var opRoles =
                        op.Permissions.SelectMany(a => a.Roles.Select(r => new Claim(System.Security.Claims.ClaimTypes.Role, r.Name)));
                    // User is Anonymous and Operations is available to anonymous
                    if (!identity.IsAuthenticated && op.IsAvailableToAnonymous) return true;
                    // If Anonymous and reached this point -> not authorized
                    if (!identity.IsAuthenticated) return false;
                    // Available to all Authenticated?
                    if (op.IsAvailableToAllAuthorizedUsers) return identity.IsAuthenticated;
                    // Check Permissions
                    var claims = identity.Claims;
                    var principalPermissions = claims.Where(a => a.Type == Model.ClaimTypes.Permission);
                    if (principalPermissions.Intersect(opPermissions, new ClaimsEqualityComparer()).Any())
                    {
                        return true;
                    }
                    // Check Roles (Since Roles are (for us) a set of permissions then could skip this step
                    // IF we add the relevant permissions at the ClaimsIdentity transformation/factory
                    var principalRoles = claims.Where(a => a.Type == System.Security.Claims.ClaimTypes.Role);
                    return principalRoles.Intersect(opRoles, new ClaimsEqualityComparer()).Any();
                    // TODO - Log Operation not found
                    // Normally, we must return false. Leave it commented until
                    // this coding facility is stable enough to test this
                    //return false;);
                }
                if (pec.Resource.First().Type == Model.ClaimTypes.ApplicationAccess)
                {
                    return pec.Principal.Claims.Any(a => a.Type == Model.ClaimTypes.Permission ||
                                                    a.Type == System.Security.Claims.ClaimTypes.Role);
                }
                var ra = _policies.FirstOrDefault(a =>
                                                  a.Key.ResourceType == pec.Resource.First().Type &&
                                                  a.Key.Resource == pec.Resource.First().Value &&
                                                  a.Key.ActionType == pec.Action.First().Type &&
                                                  a.Key.Action == pec.Action.First().Value).Key;
                access = ra != null ? _policies[ra](pec.Principal) : base.CheckAccess(pec);
            }
            catch (Exception x)
            {
                LogManager.GetLogger(this.GetType()).Error("Error in CheckAccess", x);
                access = false; // this may result in TOO_MANY_REDIRECTS error, maybe we should rethrow...
            }
            return access;
        }
    }

    public class ClaimsEqualityComparer : IEqualityComparer<Claim>
    {
        public bool Equals(Claim x, Claim y)
        {
            return x.Issuer == y.Issuer
                   && x.Type == y.Type
                   && x.Value == y.Value
                   && x.ValueType == y.ValueType;
        }

        public int GetHashCode(Claim obj)
        {
            return (obj.Issuer
                    + obj.Type
                    + obj.Value
                    + obj.ValueType).GetHashCode();
        }
    }
}

#endif