#if NETFRAMEWORK
#else
using System;
using NHibernate;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;
using log4net;
using NHibernate.Linq;
using zAppDev.DotNet.Framework.Data;
using Microsoft.AspNetCore.Authorization;

using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
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

    public class OperationAuthorizeAttribute : AuthorizeAttribute
    {
        public const string POLICY_PREFIX = "OperationAuthorize";

        public OperationAuthorizeAttribute(string controller, string name, string type)
        {
            Policy = $"{POLICY_PREFIX}|{controller}|{name}|{type}";
        }
    }

    public class OperationRequirement : IAuthorizationRequirement
    {
        public string ActionName { get; private set; }
        public string ClaimType { get; private set; }
        public string ControllerName { get; private set; }

        public OperationRequirement(string controller, string name, string type)
        {
            ActionName = name;
            ClaimType = type;
            ControllerName = controller;
        }
    }

    public class OperationPolicyProvider : IAuthorizationPolicyProvider
    {
        public Task<AuthorizationPolicy> GetDefaultPolicyAsync() =>
            Task.FromResult(new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());

        public Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            if (policyName.StartsWith(OperationAuthorizeAttribute.POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                var policy = new AuthorizationPolicyBuilder();

                var data = policyName.Substring(OperationAuthorizeAttribute.POLICY_PREFIX.Length + 1);

                var args = data.Split('|');

                policy.AddRequirements(new OperationRequirement(args[0], args[1], args[2]));
                return Task.FromResult(policy.Build());
            }

            return Task.FromResult<AuthorizationPolicy>(null);
        }
    }

    public class OperationAuthorizationHandler : AuthorizationHandler<OperationRequirement>
    {
        private readonly OperationAuthorizationService _operationAuthorizationService;

        public OperationAuthorizationHandler(OperationAuthorizationService operationAuthorizationService)
        {
            _operationAuthorizationService = operationAuthorizationService;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, OperationRequirement requirement)
        {
            if (_operationAuthorizationService.CheckAccess(context, context.User.Identity as ClaimsIdentity, requirement))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }

    public class OperationAuthorizationService
    {
        private static ConcurrentDictionary<string, List<ApplicationOperation>> _operationsDictionary = null;

        private IMiniSessionService _sessionService;
        private readonly Dictionary<ResourceAction, Func<ClaimsPrincipal, bool>> _policies = new Dictionary<ResourceAction, Func<ClaimsPrincipal, bool>>();
        private readonly object _operationsLock = new object();
        private readonly PolicyReader _policyReader = new PolicyReader();

        /// <summary>
        /// Creates a new instance of the MyClaimsAuthorizationManager
        /// </summary>
        public OperationAuthorizationService(ISessionFactory factory)
        {
            _sessionService = new MiniSessionService(factory);
        }

        public bool AdministratorExists(string adminRoleName = "Administrator")
        {
            return _sessionService
                   .Session.Query<ApplicationUser>()
                   .Where(u => u.Roles.Any(r => r.Name == adminRoleName))
                   .WithOptions(options => options.SetCacheable(true))
                   .Any();
        }

        public ApplicationUser GetFirstAdministratorUser(string adminRoleName = "Administrator")
        {
            return _sessionService
                    .Session.Query<ApplicationUser>()
                   .Where(u => u.Roles.Any(r => r.Name == adminRoleName))
                   .WithOptions(options => options.SetCacheable(true))
                   .FirstOrDefault();
        }

        public static void InvalidateCache()
        {
            _operationsDictionary = null;
        }

        public string GetOperationDictionaryKey(ApplicationOperation operation)
        {
            return
                GetOperationDictionaryKey(operation.Type, operation.Name, operation.ParentControllerName);
        }

        public string GetOperationDictionaryKey(string type, string name, string parentControllerName)
        {
            return type + "|" + name + "|" + parentControllerName;
        }

        public void LoadCustomConfiguration(XmlNodeList nodelist)
        {
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
                        using (var rdr = XmlDictionaryReader.CreateDictionaryReader(xtr))
                        {
                            rdr.MoveToContent();
                            var resource = rdr.GetAttribute("resource");
                            var action = rdr.GetAttribute("action");
                            var policyExpression = _policyReader.ReadPolicy(rdr);
                            //
                            // Compile the policy expression into a function
                            //
                            var policy = policyExpression.Compile();
                            //
                            // Insert the policy function into the policy cache
                            //
                            _policies[new ResourceAction(resource, action)] = policy;
                        }
                    }
                }
            }
        }

        public bool CheckAccess(AuthorizationHandlerContext context)
        {
            return CheckAccess(context, context.User.Identity as ClaimsIdentity, null);
        }

        public bool CheckAccess(AuthorizationHandlerContext context, ClaimsIdentity identity)
        {
            return CheckAccess(context, identity, null);
        }

        public bool CheckAccess(AuthorizationHandlerContext pec, ClaimsIdentity identity, OperationRequirement requirement)
        {
            var access = false;
            var firstResourceType = requirement.ClaimType;

            try
            {
                if (firstResourceType == Model.ClaimTypes.ControllerAction ||
                        firstResourceType == Model.ClaimTypes.Dataset ||
                        firstResourceType == Model.ClaimTypes.GenericAction ||
                        firstResourceType == Model.ClaimTypes.IDEF0Activity ||
                        firstResourceType == Model.ClaimTypes.Url ||
                        firstResourceType == Model.ClaimTypes.ExposedService)
                {
                    var operationsDictionary = GetOperationsDictionary();

                    if (operationsDictionary == null)
                    {
                        throw new ApplicationException(
                            $"OperationsDictionary is empty! Parent: '{requirement.ControllerName}', Action: '{requirement.ActionName}', Type: '{firstResourceType}'");
                    }

                    string key = GetOperationDictionaryKey(firstResourceType, requirement.ActionName, requirement.ControllerName);

                    if (!operationsDictionary.ContainsKey(key))
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{requirement.ControllerName}`.`{requirement.ActionName}` of `{firstResourceType}` was not found.");
                        return true;
                    }

                    var applicationOperationsList = operationsDictionary[key];
                    if (applicationOperationsList.Count > 1)
                    {
                        throw new ApplicationException(
                            $"Duplicate Operations found! Parent: '{requirement.ControllerName}', Action: '{requirement.ActionName}', Type: '{firstResourceType}'");
                    }

                    if (applicationOperationsList.Count == 0)
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{requirement.ControllerName}`.`{requirement.ActionName}` of `{firstResourceType}` was not found.");
                        return true;
                    }

                    var operation = applicationOperationsList[0];
                    if (operation == null)
                    {
                        // Operation was not found. Regard as accessible. Probably internal.
                        LogManager.GetLogger(GetType()).Debug($"Operation `{requirement.ControllerName}`.`{requirement.ActionName}` of `{firstResourceType}` was not found.");
                        return true;
                    }

                    // User is Anonymous and Operations is available to anonymous
                    if (!identity.IsAuthenticated && operation.IsAvailableToAnonymous) return true;
                    // If Anonymous and reached this point -> not authorized
                    if (!identity.IsAuthenticated) return false;
                    // Available to all Authenticated?
                    if (operation.IsAvailableToAllAuthorizedUsers) return identity.IsAuthenticated;

                    var opPermissions = operation.Permissions
                        .Select(a => new Claim(Model.ClaimTypes.Permission, a.Name));
                    var opRoles = operation.Permissions
                        .SelectMany(a => a.Roles.Select(r => new Claim(System.Security.Claims.ClaimTypes.Role, r.Name)));

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

                if (requirement.ClaimType == Model.ClaimTypes.ApplicationAccess)
                {
                    return pec.User.Claims.Any(a => a.Type == Model.ClaimTypes.Permission ||
                                                    a.Type == System.Security.Claims.ClaimTypes.Role);
                }
                //var ra = _policies.FirstOrDefault(a =>
                //                                  a.Key.ResourceType == requirement.ClaimType &&
                //                                  a.Key.Resource == requirement.ControllerName &&
                //                                  a.Key.ActionType == requirement.ClaimType &&
                //                                  a.Key.Action == requirement.ActionName);
                // access = ra != null ? _policies[ra](pec.Principal) : base.CheckAccess(pec);

            }
            catch (Exception ex)
            {
                LogManager.GetLogger(GetType())
                    .Error("Error in CheckAccess", ex);
                access = false; // this may result in TOO_MANY_REDIRECTS error, maybe we should rethrow...
            }

            return access;
        }

        private bool GetOperationsInner()
        {
            var w = System.Diagnostics.Stopwatch.StartNew();
            lock (_operationsLock)
            {
                if (_operationsDictionary != null)
                    return true;
                _sessionService.OpenSession();
                // Load & Cache Operations
                var operations = _sessionService.Session.Query<ApplicationOperation>()
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
            LogManager.GetLogger(GetType()).Debug($"Got all Operations/Permissions/Roles in {w.Elapsed}");
            return true;
        }

        private ConcurrentDictionary<string, List<ApplicationOperation>> GetOperationsDictionary()
        {
            if (_operationsDictionary != null) return _operationsDictionary;
            GetOperationsInner();
            return _operationsDictionary;
        }
    }
}
#endif