// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Auditing.Model;
using zAppDev.DotNet.Framework.Data.DAL;

namespace zAppDev.DotNet.Framework.Auditing
{
    public interface IAuditingRepository : ICreateRepository
    {
        void DeleteAuditPropertyConfiguration(AuditPropertyConfiguration propertyConfiguration, 
            bool doNotCallDeleteForThis = false, 
            bool isCascaded = false, 
            object calledBy = null);

        void DeleteAuditEntityConfiguration(
            AuditEntityConfiguration auditentityconfiguration,
            bool doNotCallDeleteForThis = false, bool isCascaded = false, object calledBy = null);
    }
}
