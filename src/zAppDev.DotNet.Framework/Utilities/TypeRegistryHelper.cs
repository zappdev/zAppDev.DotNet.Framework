using zAppDev.DotNet.Framework.Auditing.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class TypeRegistryHelper
    {
        private static List<Type> _domainTypes = null;

        public static List<Type> GetSystemDomainModelTypes()
        {
            if (_domainTypes == null)
            {
                _domainTypes = typeof(TypeRegistryHelper).Assembly
                    .GetTypes()
                    .Where(t => 
                        t.GetInterfaces().Contains(typeof(Data.Domain.IDomainModelClass)))
                    .ToList();
            }

            return _domainTypes;
        }

        public static List<Type> GetAuditableSystemDomainModelTypes() => 
            GetSystemDomainModelTypes()
                .Where(t => !t.Namespace.Equals(typeof(AuditEntityConfiguration).Namespace))
                .ToList();
    }
}
