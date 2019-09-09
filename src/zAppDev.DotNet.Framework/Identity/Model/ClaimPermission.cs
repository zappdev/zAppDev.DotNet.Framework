// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IdentityModel.Services;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Claims;
using System.Security.Permissions;
using System.Security.Policy;
using System.Text;
using System.Web;
using zAppDev.DotNet.Framework.Owin;
using Microsoft.AspNet.Identity.Owin;

namespace zAppDev.DotNet.Framework.Identity.Model
{
    /// <summary>
    /// Encapsulates calls to ClaimsAuthorizationManager with custom claim types in a CLR permission
    /// </summary>
    [Serializable]
    public sealed class ClaimPermission : IPermission, IUnrestrictedPermission
    {
        private readonly List<ResourceAction> _resourceActions;

        private ClaimPermission(IEnumerable<ResourceAction> resourceActions)
        {
            this._resourceActions = new List<ResourceAction>();
            foreach (ResourceAction action in resourceActions)
            {
                this._resourceActions.Add(new ResourceAction(action.ResourceType, action.Resource, action.ActionType, action.Action));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimPermission"/> class.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        public ClaimPermission(string resource, string action) : this(ClaimTypes.ResourceType, resource, ClaimTypes.ActionType, action) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimPermission"/> class.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        public ClaimPermission(string resourceType, string resource, string action)
        {
            this._resourceActions = new List<ResourceAction> {new ResourceAction(resourceType, resource, ClaimTypes.ActionType, action)};
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClaimPermission"/> class.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="actionType">Type of the action.</param>
        /// <param name="action">The action.</param>
        public ClaimPermission(string resourceType, string resource, string actionType, string action)
        {
            this._resourceActions = new List<ResourceAction> {new ResourceAction(resourceType, resource, actionType, action)};
        }

        /// <summary>
        /// Creates and returns an identical copy of the current permission.
        /// </summary>
        /// <returns>
        /// A copy of the current permission.
        /// </returns>
        public IPermission Copy()
        {
            return new ClaimPermission(_resourceActions);
        }

        /// <summary>
        /// Throws a <see cref="T:System.Security.SecurityException"/> at run time if the security requirement is not met.
        /// </summary>
        public void Demand()
        {
            var claimsAuthorizationManager = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager;
            var currentPrincipal = ClaimsPrincipal.Current;
            foreach (var resourceAction in _resourceActions)
            {
                var context = CreateAuthorizationContext(currentPrincipal, resourceAction);
                if (!claimsAuthorizationManager.CheckAccess(context))
                {
                    ThrowSecurityException();
                }
            }
        }

        public static void CheckAccessWithException(string resourcetype, string resouce, string action)
        {
            var perm = new ClaimPermission(resourcetype, resouce, action);
            perm.Demand();
        }

        public static bool CheckAccess(string resourceType, string resource, string action, string userName)
        {
            var resourceAction = new ResourceAction(resourceType, resource, ClaimTypes.ActionType, action);
            var user = IdentityHelper.GetIdentityUserByName(userName);
            var manager = IdentityHelper.GetUserManager();
            ClaimsIdentity userIdentity = null;
            if (user == null)
            {
                log4net.LogManager.GetLogger(nameof(ClaimPermission)).Warn($"A user with username '{userName}' was not found by user manager. Creating Claims Identity from Owin Context Request User.Identity ...");
                userIdentity = (OwinHelper.GetOwinContext(HttpContext.Current).Request.User.Identity as ClaimsIdentity);
            }
            else
            {
                userIdentity = user.GenerateUserIdentityAsync(manager).Result;
            }
            var context = CreateAuthorizationContext(ClaimsPrincipal.Current, resourceAction);
            var claimsAuthorizationManager =
                FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager as AuthorizationManager;
            return claimsAuthorizationManager.CheckAccess(context, userIdentity);
        }

        /// <summary>
        /// Calls ClaimsAuthorizationManager.
        /// </summary>
        /// <param name="resourceType">The resource type.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="action">The action.</param>
        /// <returns>True when access is granted. Otherwise false.</returns>
        public static bool CheckAccess(string resourceType, string resource, string action)
        {
            var resourceAction = new ResourceAction(resourceType, resource, ClaimTypes.ActionType, action);
            var context = CreateAuthorizationContext(ClaimsPrincipal.Current, resourceAction);
            var claimsAuthorizationManager = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager;
            return claimsAuthorizationManager.CheckAccess(context);
        }

        /// <summary>
        /// Calls ClaimsAuthorizationManager.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="resource">The resource.</param>
        /// <param name="additionalResources">Additional resources.</param>
        /// <returns>True when access is granted. Otherwise false.</returns>
        public static bool CheckAccess(string action, string resource, params Claim[] additionalResources)
        {
            var resourceAction = new ResourceAction(ClaimTypes.ResourceType, resource, ClaimTypes.ActionType, action);
            var context = CreateAuthorizationContext(ClaimsPrincipal.Current, resourceAction);
            additionalResources.ToList().ForEach(claim => context.Resource.Add(claim));
            var claimsAuthorizationManager = FederatedAuthentication.FederationConfiguration.IdentityConfiguration.ClaimsAuthorizationManager;
            return claimsAuthorizationManager.CheckAccess(context);
        }

        private static AuthorizationContext CreateAuthorizationContext(ClaimsPrincipal currentPrincipal, ResourceAction resourceAction)
        {
            var resourceClaim = new Claim(resourceAction.ResourceType, resourceAction.Resource);
            var actionClaim = new Claim(resourceAction.ActionType, resourceAction.Action);
            return new AuthorizationContext(currentPrincipal,new Collection<Claim> {resourceClaim},new Collection<Claim> {actionClaim});
        }

        #region CLR Permission Implementation

        /// <summary>
        /// Reconstructs a security object with a specified state from an XML encoding.
        /// </summary>
        /// <param name="e">The XML encoding to use to reconstruct the security object.</param>
        public void FromXml(SecurityElement e)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates and returns a permission that is the intersection of the current permission and the specified permission.
        /// </summary>
        /// <param name="target">A permission to intersect with the current permission. It must be of the same type as the current permission.</param>
        /// <returns>
        /// A new permission that represents the intersection of the current permission and the specified permission. This new permission is null if the intersection is empty.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The <paramref name="target"/> parameter is not null and is not an instance of the same class as the current permission. </exception>
        public IPermission Intersect(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            var permission = target as ClaimPermission;
            if (permission == null)
            {
                return null;
            }
            var resourceActions = new List<ResourceAction>();
            foreach (ResourceAction action in permission._resourceActions)
            {
                if (this._resourceActions.Contains(action))
                {
                    resourceActions.Add(action);
                }
            }
            return new ClaimPermission(resourceActions);
        }

        /// <summary>
        /// Determines whether the current permission is a subset of the specified permission.
        /// </summary>
        /// <param name="target">A permission that is to be tested for the subset relationship. This permission must be of the same type as the current permission.</param>
        /// <returns>
        /// true if the current permission is a subset of the specified permission; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The <paramref name="target"/> parameter is not null and is not of the same type as the current permission. </exception>
        public bool IsSubsetOf(IPermission target)
        {
            if (target == null)
            {
                return false;
            }
            var permission = target as ClaimPermission;
            if (permission == null)
            {
                return false;
            }
            foreach (ResourceAction action in this._resourceActions)
            {
                if (!permission._resourceActions.Contains(action))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether unrestricted access to the resource protected by the permission is allowed.
        /// </summary>
        /// <returns>
        /// true if unrestricted use of the resource protected by the permission is allowed; otherwise, false.
        /// </returns>
        public bool IsUnrestricted()
        {
            return true;
        }

        private void ThrowSecurityException()
        {
            AssemblyName assemblyName = null;
            Evidence evidence = null;
            new PermissionSet(PermissionState.Unrestricted).Assert();
            try
            {
                Assembly callingAssembly = Assembly.GetCallingAssembly();
                assemblyName = callingAssembly.GetName();
                if (callingAssembly != Assembly.GetExecutingAssembly())
                {
                    evidence = callingAssembly.Evidence;
                }
            }
            catch
            {
                PermissionSet.RevertAssert();
                throw new SecurityException("Access Denied", assemblyName, null, null, null, SecurityAction.Demand, this, this, evidence);
            }
            PermissionSet.RevertAssert();
            throw new SecurityException("Access Denied", assemblyName, null, null, null, SecurityAction.Demand, this, this, evidence);
        }

        /// <summary>
        /// Creates an XML encoding of the security object and its current state.
        /// </summary>
        /// <returns>
        /// An XML encoding of the security object, including any state information.
        /// </returns>
        public SecurityElement ToXml()
        {
            var element = new SecurityElement("IPermission");
            Type type = GetType();
            var builder = new StringBuilder(type.Assembly.ToString());
            builder.Replace('"', '\'');
            element.AddAttribute("class", type.FullName + ", " + builder);
            element.AddAttribute("version", "1");
            foreach (ResourceAction action in this._resourceActions)
            {
                var child = new SecurityElement("ResourceAction");
                child.AddAttribute("resource", action.Resource);
                child.AddAttribute("action", action.Action);
                element.AddChild(child);
            }
            return element;
        }

        /// <summary>
        /// Creates a permission that is the union of the current permission and the specified permission.
        /// </summary>
        /// <param name="target">A permission to combine with the current permission. It must be of the same type as the current permission.</param>
        /// <returns>
        /// A new permission that represents the union of the current permission and the specified permission.
        /// </returns>
        /// <exception cref="T:System.ArgumentException">The <paramref name="target"/> parameter is not null and is not of the same type as the current permission. </exception>
        public IPermission Union(IPermission target)
        {
            if (target == null)
            {
                return null;
            }
            var permission = target as ClaimPermission;
            if (permission == null)
            {
                return null;
            }
            var resourceActions = new List<ResourceAction>();
            resourceActions.AddRange(permission._resourceActions);
            foreach (ResourceAction action in this._resourceActions)
            {
                if (!resourceActions.Contains(action))
                {
                    resourceActions.Add(action);
                }
            }
            return new ClaimPermission(resourceActions);
        }
    }

    #endregion
}
#endif