// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
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
                try
                {
                    _domainTypes = typeof(TypeRegistryHelper).Assembly
                        .GetTypes()
                        .Where(t =>
                            t.GetInterfaces().Contains(typeof(Data.Domain.IDomainModelClass)))
                        .ToList();
                }
                catch (System.Reflection.ReflectionTypeLoadException e)
                {
                    log4net.LogManager.GetLogger(typeof(TypeRegistryHelper))
                        .Error("Failed to load Domain Model classes from Assembly. LoaderExceptions:" + string.Join("\r\n\t", e.LoaderExceptions.Select(le => le.Message)));
                    throw;
                }
            }

            return _domainTypes;
        }

        public static List<Type> GetAuditableSystemDomainModelTypes() => 
            GetSystemDomainModelTypes()
                .Where(t => !t.Namespace.Equals(typeof(AuditEntityConfiguration).Namespace))
                .ToList();
    }
}
