// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using System;
using System.Linq;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.AccountManagement;
using System.Security.Principal;

namespace zAppDev.DotNet.Framework.Identity
{
    public class ActiveDirectoryHelper
    {
        private static ILog _logger = log4net.LogManager.GetLogger(typeof(ActiveDirectoryHelper));

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

        public static string GetCurrentUserProperty(string domain, string key, string group = null)
        {
            return GetUserProperty(IdentityHelper.GetCurrentUserName(), domain, key, group);
        }

        public static string GetUserProperty(string userName, string domain, string key, string group = null)
        {
            var result = GetUserProperties(userName, domain, new List<string>() { key }, group);
            if (result == null) return null;
            return result[key]?.ToString();
        }

        private static void FillUserProperties(UserPrincipal user, Dictionary<string, object> results, List<string> keys = null)
        {
            _logger.Debug("FillUserProperties: Filling in user's properties");

            var fetchAll = keys?.Any() != true;

            var properties = user.GetType().GetProperties();
            foreach (var property in properties)
            {
                _logger.Debug($"FillUserProperties: Examining [{property.Name}]");
                if (fetchAll || keys.FirstOrDefault(x => string.CompareOrdinal(x, property.Name) == 0) != null)
                {
                    var value = property.GetValue(user, null);
                    results[property.Name] = value;
                    _logger.Debug($"FillUserProperties: [{property.Name} = {value}]");
                }
            }
        }

        private static string GetUserPrimaryGroup(DirectoryEntry de)
        {
            _logger.Debug("GetUserPrimaryGroup: Getting user's Primary Group");

            _logger.Debug("GetUserPrimaryGroup: Refreshing Cache with primaryGroupID and objectSid");
            de.RefreshCache(new[] { "primaryGroupID", "objectSid" });

            //Get the user's SID as a string
            var sid = new SecurityIdentifier((byte[])de.Properties["objectSid"].Value, 0).ToString();
            _logger.Debug($"GetUserPrimaryGroup: Fetched [sid = {sid}]");

            //Replace the RID portion of the user's SID with the primaryGroupId
            //so we're left with the group's SID
            sid = sid.Remove(sid.LastIndexOf("-", StringComparison.Ordinal) + 1);
            sid = sid + de.Properties["primaryGroupId"].Value;
            _logger.Debug($"GetUserPrimaryGroup: Created [sid = {sid}]");


            //Find the group by its SID
            _logger.Debug($"GetUserPrimaryGroup: Finding the group by its SID, refreshing cn");
            var group = new DirectoryEntry($"LDAP://<SID={sid}>");
            group.RefreshCache(new[] { "cn" });

            var cn = group.Properties["cn"].Value as string;
            _logger.Debug($"GetUserPrimaryGroup: [cn = {cn}]");
            return cn;
        }

        private static bool PropertyAdded(Dictionary<string, object> properties, string name)
        {
            foreach (var key in properties.Keys)
            {
                if (string.CompareOrdinal(key.Trim(), name.Trim()) == 0)
                {
                    return true;
                }
            }
            return false;
        }




        /// <summary>
        /// Returns a collection of a given User's properties
        /// </summary>
        /// <param name="userName">The username, within a specified Domain, whose properties are to be retrieved</param>
        /// <param name="domain">The Domain name</param>
        /// <param name="keys">The properties to be fetched. If null or empty, all properties will be retrieved</param>
        /// <returns></returns>
        public static Dictionary<string, object> GetUserProperties(string userName, string domain, List<string> keys = null, string group = null)
        {
            using (var context = new PrincipalContext(ContextType.Domain, domain))
            {
                _logger.Debug($"GetUserProperties: Getting user's properties");
                UserPrincipal user = null;
                DirectoryEntry userDirectoryEntry = null;
                if (string.IsNullOrWhiteSpace(group))
                {
                    _logger.Debug($"GetUserProperties: Searching for User inside his/her Domain");
                    PrincipalContext ctx = new PrincipalContext(ContextType.Domain);
                    user = UserPrincipal.FindByIdentity(ctx, NormalizeUsername(userName));

                    //OK this is an obvious overkill, but I don't know what else to do 
                    if (user == null)
                    {
                        _logger.Debug($"GetUserProperties: Didn't find the user. Searching within all Directory Entries");
                        using (var searcher = new PrincipalSearcher(new UserPrincipal(context)))
                        {
                            foreach (var result in searcher.FindAll())
                            {
                                var de = result.GetUnderlyingObject() as DirectoryEntry;
                                de.RefreshCache(new string[] { "samAccountName" });
                                var samAccountName = de.Properties["samAccountName"].Value?.ToString();
                                if (string.CompareOrdinal(samAccountName, userName) == 0)
                                {
                                    userDirectoryEntry = de;
                                }
                            }
                        }
                    }
                }
                else
                {
                    _logger.Debug($"GetUserProperties: Searching for User inside his/her group: {group}");
                    GroupPrincipal grp = GroupPrincipal.FindByIdentity(context, group);
                    if (grp != null)
                    {
                        foreach (Principal p in grp.GetMembers())
                        {
                            if (p is UserPrincipal)
                            {
                                var userPrincipal = p as UserPrincipal;
                                if (string.CompareOrdinal(userPrincipal.SamAccountName, userName) == 0)
                                    user = userPrincipal;
                            }
                        }
                    }
                }


                var userProperties = new Dictionary<string, object>();
                if (user == null && userDirectoryEntry == null)
                {
                    LogManager.GetLogger(typeof(ActiveDirectoryHelper))
                    .Error($"User '{userName}'(normalized: '{NormalizeUsername(userName)}) not found under '{domain}' domain");
                    return userProperties;
                }

                _logger.Debug($"GetUserProperties: Found user [{userName}] in AD");

                DirectoryEntry directoryEntry = null;
                if (user != null)
                    directoryEntry = user.GetUnderlyingObject() as DirectoryEntry;
                else
                    directoryEntry = userDirectoryEntry;


                _logger.Debug($"GetUserProperties: Refreshing Cache");
                directoryEntry.RefreshCache();

                bool fetchAll = keys?.Any() != true;

                if (fetchAll)
                    directoryEntry.RefreshCache();
                else
                {
                    foreach (var key in keys)
                    {
                        _logger.Debug($"GetUserProperties: Refreshing cache for key [{key}]");
                        directoryEntry.RefreshCache(new string[] { key });
                    }
                }

                _logger.Debug($"GetUserProperties: Filling user properties");
                FillUserProperties(user, userProperties, keys);


                if ((fetchAll || keys.FirstOrDefault(x => string.CompareOrdinal(x, "PrimaryGroup") == 0) != null) && (!PropertyAdded(userProperties, "PrimaryGroup")))
                {
                    _logger.Debug($"GetUserProperties: Getting user's Primary Group");
                    userProperties["PrimaryGroup"] = GetUserPrimaryGroup(directoryEntry);
                }

                _logger.Debug($"GetUserProperties: Browsing through the properties");
                foreach (PropertyValueCollection property in directoryEntry.Properties)
                {
                    if (fetchAll || (keys.FirstOrDefault(x => string.Compare(x, property.PropertyName, true) == 0) != null))
                    {
                        if (PropertyAdded(userProperties, property.PropertyName)) continue;
                        userProperties[property.PropertyName] = property.Value;
                        _logger.Debug($"GetUserProperties: [{property.PropertyName} = {property.Value}]");
                    }
                }

                _logger.Debug($"GetUserProperties: Browsing through the keys, as a fail-safe");
                foreach (var key in keys)
                {
                    if (PropertyAdded(userProperties, key)) continue;
                    var value = directoryEntry.Properties[key]?.Value;
                    if (value != null) userProperties[key] = value;
                    _logger.Debug($"GetUserProperties: [{key} = {value}]");
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
