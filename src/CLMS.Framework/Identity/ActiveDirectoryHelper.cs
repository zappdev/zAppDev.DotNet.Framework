using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;

namespace CLMS.Framework.Identity
{
    public class ActiveDirectoryHelper
    {
        public static List<string> GetAllUsers(string domain)
        {
            var users = new List<string>();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                {
                    foreach (var result in searcher.FindAll())
                    {
                        DirectoryEntry de = result.GetUnderlyingObject() as DirectoryEntry;
                        users.Add(de.Properties["samAccountName"].Value?.ToString());
                    }
                }
            }
            return users;
        }

        public static List<string> GetAllUsers(string domain, string group)
        {
            if (string.IsNullOrWhiteSpace(group)) return GetAllUsers(domain);
            var users = new List<string>();
            using (var ctx = new PrincipalContext(ContextType.Domain, domain))
            {
                GroupPrincipal grp = GroupPrincipal.FindByIdentity(ctx, group);
                if (grp != null)
                {
                    foreach (Principal p in grp.GetMembers())
                    {
                        if (p is UserPrincipal)
                        {
                            users.Add((p as UserPrincipal).SamAccountName);
                        }
                    }
                }
                else
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper)).Debug($"Active Directory Group '{group}' not found under '{domain}' domain");
                }
            }
            return users;
        }

        public static string GetCurrentUserProperty(string domain, string key)
        {
            return GetUserProperty(IdentityHelper.GetCurrentUserName(), domain, key);
        }

        public static string GetUserProperty(string userName, string domain, string key)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));
                if (user == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Debug($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return "";
                }
                DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                return directoryEntry.Properties[key].Value?.ToString();
            }
        }

        /// <summary>
        /// Returns a collection of a given User's properties
        /// </summary>
        /// <param name="userName">The username, within a specified Domain, whose properties are to be retrieved</param>
        /// <param name="domain">The Domain name</param>
        /// <param name="keys">The properties to be fetched. If null or empty, all properties will be retrieved</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetUserProperties(string userName, string domain, List<string> keys = null)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));
                var userProperties = new Dictionary<string, object>();
                if (user == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Debug($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return userProperties;
                }
                DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                bool fetchAll = keys?.Any() != true;
                foreach (PropertyValueCollection property in directoryEntry.Properties)
                {
                    if (fetchAll || (keys.FirstOrDefault(x => string.Compare(x, property.PropertyName, true) == 0) != null))
                    {
                        userProperties[property.PropertyName] = property.Value;
                    }
                }
                return userProperties;
            }
        }

        public static DateTime? GetPasswordExpirationDate(string userName, string domain)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));
                var userProperties = new Dictionary<string, object>();
                if (user == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Debug($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return null;
                }
                DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                return (DateTime?)directoryEntry.InvokeGet("PasswordExpirationDate");
            }
        }

        public static bool ChangePassword(string userName, string domain, string oldPassword, string newPassword)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));
                var userProperties = new Dictionary<string, object>();
                if (user == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Debug($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return false;
                }
                user.ChangePassword(oldPassword, newPassword);
                return true;
            }
        }

        public static List<string> GetCurrentUserGroups(string domain)
        {
            return GetUserGroups(IdentityHelper.GetCurrentUserName(), domain);
        }

        public static List<string> GetUserGroups(string userName, string domain)
        {
            var groups = new List<string>();
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                UserPrincipal user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));
                if (user == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Debug($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return groups;
                }
                DirectoryEntry directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                var members = directoryEntry.Properties["memberOf"];
                if (members == null)
                {
                    return new List<string>();
                }
                for (var i = 0; i < members.Count; i++)
                {
                    var dn = members[i]?.ToString();
                    var equalsIndex = dn.IndexOf("=", 1);
                    var commaIndex = dn.IndexOf(",", 1);
                    if (-1 == equalsIndex)
                    {
                        return null;
                    }
                    groups.Add(dn.Substring((equalsIndex + 1),
                                            (commaIndex - equalsIndex) - 1));
                }
            }
            return groups;
        }

        private static string NormalizeUsername(string username)
        {
            var indexOfSeperator = username.LastIndexOf("\\");
            if (indexOfSeperator == -1)
            {
                return username;
            }
            return username.Substring(indexOfSeperator + 1);
        }
    }
}
