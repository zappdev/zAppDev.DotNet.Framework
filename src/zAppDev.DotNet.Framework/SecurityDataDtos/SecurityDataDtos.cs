using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.SecurityDataDtos
{
    public class ApplicationOperation
    {        
        public int? Id { get; set; }
        public string Name { get; set; }
        public string ParentControllerName { get; set; }
        public string Type { get; set; }
        public bool IsAvailableToAnonymous { get; set; }
        public bool IsAvailableToAllAuthorizedUsers { get; set; }
        public List<ApplicationPermission> Permissions { get; set; }                
    }

    public class ApplicationPermission
    {       
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool IsCustom { get; set; }
    }

    public class ApplicationRole
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool IsCustom { get; set; }
        public List<ApplicationPermission> Permissions { get; set; }
    }

    public class ApplicationUser
    {
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool LockoutEnabled { get; set; }

        public Profile Profile { get; set; }
        public List<ApplicationPermission> Permissions { get; set; }
        public List<ApplicationRole> Roles { get; set; }
    }

    public class ApplicationTheme
    {
        public string Name { get; set; }
    }

    public class ApplicationLanguage
    {
        public byte[] Icon { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public int? Id { get; set; }        
        public DateTimeFormat DateTimeFormat { get; set; }
    }


    public class DateTimeFormat
    {
        public int? Id { get; set; }
        public string YearMonthPattern { get; set; }
        public string ShortTimePattern { get; set; }
        public string ShortDatePattern { get; set; }
        public string RFC1123Pattern { get; set; }
        public string MonthDayPattern { get; set; }
        public string LongTimePattern { get; set; }
        public string LongDatePattern { get; set; }        
        public ApplicationLanguage ApplicationLanguage { get; set; }        
    }

    public class Profile
    {
        public int? Id { get; set; }
        public int? LanguageLCID { get; set; }
        public int? LocaleLCID { get; set; }
        public string Theme { get; set; }



    }
}
