// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Configuration;
using System.Globalization;
using System.Threading;
using zAppDev.DotNet.Framework.Utilities;
using zAppDev.DotNet.Framework.Data.Domain;
using System.IO;
using zAppDev.DotNet.Framework.Identity.Model;

namespace zAppDev.DotNet.Framework.Identity
{
    public static class ProfileHelper
    {
        #region CRUD

        public static void InitializeProfile(this ApplicationUser user)
        {
            if (user.Profile != null) return;
            user.Profile = new Profile();
        }

        public static int GetCurrentProfileLocaleLCID()
        {
            var defaultLCID = GetDefaultLanguage()?.Id ?? Thread.CurrentThread.CurrentCulture.LCID;
            try
            {
                var currentUser = IdentityHelper.GetCurrentApplicationUser();
                var id = currentUser?.Profile?.LocaleLCID == null || currentUser.Profile.LocaleLCID == 0
                         ? defaultLCID
                         : currentUser.Profile.LocaleLCID.Value;
                return id;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(ProfileHelper)).Error("Could not get current Locale LCID", e);
                return defaultLCID;
            }
        }

        public static int GetCurrentProfileLanguageLCID()
        {
            var defaultLCID = GetDefaultLanguage()?.Id ?? Thread.CurrentThread.CurrentCulture.LCID;
            try
            {
                var currentUser = IdentityHelper.GetCurrentApplicationUser();
                var id = currentUser?.Profile?.LanguageLCID == null || currentUser.Profile.LanguageLCID == 0
                         ? defaultLCID
                         : currentUser.Profile.LanguageLCID.Value;
                return id;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(ProfileHelper)).Error("Could not get current Language LCID", e);
                return defaultLCID;
            }
        }

        public static string GetCurrentTimezoneId()
        {
            var defaultId = GetDefaultTimezone();
            try
            {
                var currentUser = IdentityHelper.GetCurrentApplicationUser();
                var id = string.IsNullOrEmpty(currentUser?.Profile?.TimezoneInfo?.Id)
                         ? defaultId
                         : currentUser.Profile.TimezoneInfo.Id;
                return id;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(ProfileHelper)).Error("Could not get current Timezone ID", e);
                return defaultId;
            }
        }

        public static object GetCurrentProfileTheme(string defaultThemeName = "DarkTheme")
        {
            var currentUser = IdentityHelper.GetCurrentApplicationUser();
            var profileTheme = (currentUser == null || currentUser.Profile == null || currentUser.Profile.Theme == null)
                               ? defaultThemeName
                               : currentUser.Profile.Theme;  
            var availableThemes = GetAllAvailableThemes();
            return profileTheme != null && availableThemes.Any(t => t.Name == profileTheme)
                   ? profileTheme
                   : availableThemes.FirstOrDefault()?.Name;
        }

        public static object GetProfileSettingValue(string key, object defaultValue = null)
        {
            var currentUser = IdentityHelper.GetCurrentApplicationUser();
            return currentUser == null
                   ? defaultValue
                   : currentUser.GetProfileSettingValue(key, defaultValue);
        }

        public static object GetProfileSettingValue(this ApplicationUser user, string keyName, object defaultValue = null)
        {
            var profile = user.Profile;
            if (profile == null) return defaultValue;
            var key = profile.Settings.FirstOrDefault(s => s.Key == keyName);
            return key == null ? defaultValue : key.Value;
        }

        public static void SetProfileSettingValue(string key, object value)
        {
            var currentUser = IdentityHelper.GetCurrentApplicationUser();
            if (currentUser == null) return;
            currentUser.SetProfileSettingValue(key, value);
        }

        public static void SetProfileSettingValue(this ApplicationUser user, string keyName, object value)
        {
            user.InitializeProfile();
            var profile = user.Profile;
            var key = profile.Settings.FirstOrDefault(s => s.Key == keyName);
            if (key == null)
            {
                profile.AddSettings(new ProfileSetting()
                {
                    Key = keyName, Value = value.ToString()
                });
            }
            else
            {
                key.Value = value.ToString();
            }
            var repo = ServiceLocator.Current
                .GetInstance<Data.DAL.IRepositoryBuilder>()
                .CreateCreateRepository();
            repo.Save<ApplicationUser>(user);
        }

        public static void DeleteProfileSetting(string key)
        {
            var currentUser = IdentityHelper.GetCurrentApplicationUser();
            if (currentUser == null) return;
            currentUser.DeleteProfileSetting(key);
        }

        public static void DeleteProfileSetting(this ApplicationUser user, string keyName)
        {
            var profile = user.Profile;
            if (profile == null) return;
            var key = profile.Settings.FirstOrDefault(s => s.Key == keyName);
            profile.RemoveSettings(key);

            var repo = ServiceLocator.Current
                .GetInstance<Data.DAL.IRepositoryBuilder>()
                .CreateCreateRepository();
            repo.Save<ApplicationUser>(user);
        }

        #endregion

        #region List Specific Methods

        public class ListView
        {
            public ListView(string name, string status)
            {
                ViewName = name;
                SerializedStatus = status;
            }

            public string ViewName;
            public string SerializedStatus;
        }

        public class ListViewsDTO
        {
            public ListViewsDTO()
            {
                Views = new List<ListView>();
            }

            public List<ListView> Views;
            public string DefaultView;
        }

        public static ListViewsDTO GetListAvailableViews(string listName)
        {
            var dto = new ListViewsDTO();
            var availableViewsKey = listName + "_VIEWS";
            var value = GetProfileSettingValue(availableViewsKey);
            if (value == null) return dto;
            var views = value.ToString().Split(new char[] { ';' }, true).ToList();
            foreach (var view in views)
            {
                var viewKey = listName + "_VIEWS_" + view;
                var viewValue = GetProfileSettingValue(viewKey);
                if (viewValue == null) continue;
                dto.Views.Add(new ListView(view, viewValue.ToString()));
            }
            var defaultViewsKey = listName + "_DEFAULT_VIEW";
            var defaultViewValue = GetProfileSettingValue(defaultViewsKey, string.Empty);
            dto.DefaultView = defaultViewValue.ToString();
            return dto;
        }

        public static void DeleteListView(string listName, string viewName)
        {
            var availableViewsKey = listName + "_VIEWS";
            var defaultViewKey = listName + "_DEFAULT_VIEW";
            var viewKey = listName + "_VIEWS_" + viewName;
            var values = GetProfileSettingValue(availableViewsKey);
            var defaultViewValue = GetProfileSettingValue(defaultViewKey);
            if (values == null) return;
            var views = values.ToString().Split(new char[] { ';' }, true).ToList();
            if (!views.Contains(viewName)) return;
            views.Remove(viewName);
            SetProfileSettingValue(availableViewsKey, string.Join(";", views));
            DeleteProfileSetting(viewKey);
            if (defaultViewValue != null && defaultViewValue.ToString() == viewName)
            {
                DeleteProfileSetting(defaultViewKey);
            }
        }

        public static void SaveListView(string listName, string viewName, string data, bool makeDefault = false)
        {
            var availableViewsKey = listName + "_VIEWS";
            var defaultViewKey = listName + "_DEFAULT_VIEW";
            var viewKey = listName + "_VIEWS_" + viewName;
            var values = GetProfileSettingValue(availableViewsKey);
            if (values == null && IdentityHelper.GetCurrentIdentityUser() == null) return;
            if (values == null) values = "";
            var views = values.ToString().Split(new char[] { ';' }, true).ToList();
            if (!views.Contains(viewName))
            {
                views.Add(viewName);
                SetProfileSettingValue(availableViewsKey, string.Join(";", views));
            }
            var defaultViewValue = GetProfileSettingValue(defaultViewKey, string.Empty);
            if (defaultViewValue != null && viewName == defaultViewValue.ToString() && !makeDefault)
            {
                SetProfileSettingValue(defaultViewKey, String.Empty);
            }
            SetProfileSettingValue(viewKey, data);
            if (makeDefault) SetProfileSettingValue(defaultViewKey, viewName);
        }

        #endregion

        #region Language & Theme Methods

        // Maybe we should place them elsewhere, but for the moment they are fine here...

        public static List<ApplicationTimezoneInfo> GetAvailableTimezoneInfos()
        {
            return TimeZoneInfo.GetSystemTimeZones().Select(x => new ApplicationTimezoneInfo
            {
                DisplayName = x.DisplayName,
                StandardName = x.StandardName,
                BaseUtcOffset = x.BaseUtcOffset
            }).ToList();
        }

        public static List<ApplicationLanguage> GetAllAvailableLanguages()
        {
            var langs = new List<ApplicationLanguage>();
            if (ConfigurationManager.AppSettings["Locales"] == null) return langs;
            var locales = ConfigurationManager.AppSettings["Locales"].ToString().Split('|');
            foreach (var locale in locales)
            {
                var localeInfo = locale.Split('$');
                var lang = GetApplicationLanguageFromLCID(int.Parse(localeInfo[0]));
                langs.Add(lang);
            }
            return langs;
        }

        public static ApplicationLanguage GetApplicationLanguageFromLCID(int LCID)
        {
            var cultureInfo = new CultureInfo(LCID);
            var timeFormat = cultureInfo.DateTimeFormat;
            var lang = new ApplicationLanguage
            {
                Id = cultureInfo.LCID,
                Code = cultureInfo.TwoLetterISOLanguageName,
                Name = cultureInfo.NativeName,
                DateTimeFormat = new DateTimeFormat()
                {
                    LongDatePattern = timeFormat.LongDatePattern,
                    LongTimePattern = timeFormat.LongTimePattern,
                    MonthDayPattern = timeFormat.MonthDayPattern,
                    RFC1123Pattern = timeFormat.RFC1123Pattern,
                    ShortDatePattern = timeFormat.ShortDatePattern,
                    ShortTimePattern = timeFormat.ShortTimePattern,
                    YearMonthPattern = timeFormat.YearMonthPattern
                }
            };
            return lang;
        }

        public static ApplicationLanguage GetDefaultLanguage()
        {
            var lang = ConfigurationManager.AppSettings["DefaultLocale"];
            return GetAllAvailableLanguages().Find(language => Equals(language.Id.ToString(), lang));
        }

        public static string GetDefaultTimezone()
        {
            return ConfigurationManager.AppSettings["DefaultTimezoneId"] ?? "UTC";
        }

        public static ApplicationLanguage GetCurrentLanguage()
        {
            return GetApplicationLanguageFromLCID((int)GetCurrentProfileLanguageLCID());
        }

        public static ApplicationLanguage GetCurrentLocale()
        {
            return GetApplicationLanguageFromLCID((int)GetCurrentProfileLocaleLCID());
        }

        public static string GetLocaleDecimalSeparator()
        {
            try
            {
                var locale = GetCurrentLocale();
                var cultureInfo = new CultureInfo(locale.Code);
                return cultureInfo.NumberFormat.NumberDecimalSeparator;
            }
            catch(Exception e)
            {
                log4net.LogManager.GetLogger(typeof(ProfileHelper)).Error("Could not get current Locale Decimal Separator", e);
                return ".";
            }
        }

        public static string GetLocaleNumberGroupSeparator()
        {
            try
            {
                var locale = GetCurrentLocale();
                var cultureInfo = new CultureInfo(locale.Code);
                return cultureInfo.NumberFormat.NumberGroupSeparator;
            }
            catch (Exception e)
            {
                log4net.LogManager.GetLogger(typeof(ProfileHelper)).Error("Could not get current Locale Group Separator", e);
                return ",";
            }
        }

        private static List<ApplicationTheme> _AllThemes;
        public static List<ApplicationTheme> GetAllAvailableThemes()
        {
            if (_AllThemes == null)
            {
                _AllThemes = new List<ApplicationTheme>();
                var availableThemes = Directory.GetDirectories(Utilities.Web.MapPath("~/Themes"));
                foreach (var themeDirectory in availableThemes)
                {
                    var dirInfo = new DirectoryInfo(themeDirectory);
                    _AllThemes.Add(new ApplicationTheme()
                    {
                        Name = dirInfo.Name,
                    });
                }
            }
            return _AllThemes;
        }

        #endregion
    }
}
#endif