// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using log4net;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using zAppDev.DotNet.Framework.Utilities;

namespace zAppDev.DotNet.Framework.Data
{
    public static class StaleDataManager
    {
        private class EntityInfo
        {
            public bool IsVersioned
            {
                get;
                set;
            }
            public AbstractEntityPersister ClassMetaData
            {
                get;
                set;
            }
            public Type EntityType
            {
                get;
                set;
            }

            public Domain.IDomainModelClass DomainModelClass
            {
                get;
                set;
            }
        }

        private static ConcurrentDictionary<string, EntityInfo> entityInfos = new ConcurrentDictionary<string, EntityInfo>();
        private static List<string> excludeFromConcurencyControlList = new List<string>();

        public static void ExcludeFromConcurencyControl(List<string> domainModelClasses)
        {
            excludeFromConcurencyControlList = domainModelClasses;
        }

        public static void IncludeToConcurencyControl(List<string> domainModelClasses)
        {
            foreach( var cls in domainModelClasses)
            {
                if(excludeFromConcurencyControlList.Contains(cls))
                {
                    excludeFromConcurencyControlList.Remove(cls);
                }
            }
        }            


        private static bool MustCheck(Domain.IDomainModelClass cls)
        {
            var domainModelClassTypeFullName = cls.GetType().FullName;
            var name = cls.GetType().Name;
            if (excludeFromConcurencyControlList.Contains(name) || (entityInfos.ContainsKey(domainModelClassTypeFullName) && !entityInfos[domainModelClassTypeFullName].IsVersioned))
            {
                return false;
            }
            return true;
        }

#if NETFRAMEWORK
        private static EntityInfo GetEntityInfo(MiniSessionManager manager, Domain.IDomainModelClass domainModelClass)
#else
        private static EntityInfo GetEntityInfo(IMiniSessionService manager, Domain.IDomainModelClass domainModelClass)
#endif
        {
            EntityInfo entityInfo;
            var domainModelClassType = domainModelClass.GetType();
            if (entityInfos.ContainsKey(domainModelClassType.FullName))
            {
                entityInfo = entityInfos[domainModelClassType.FullName];
            }
            else
            {
                entityInfo = new EntityInfo();
                entityInfo.EntityType = domainModelClass.GetType();
                entityInfo.ClassMetaData = ((AbstractEntityPersister)manager.Session.SessionFactory.GetAllClassMetadata()[entityInfo.EntityType.FullName]);
                entityInfo.IsVersioned = entityInfo.ClassMetaData?.IsVersioned == true && entityInfo.ClassMetaData?.VersionType?.Name == "Int32";
                entityInfo.DomainModelClass = domainModelClass;
                entityInfos.TryAdd(domainModelClassType.FullName, entityInfo);
            }
            return entityInfo;
        }

        public static bool IsStale(Domain.IDomainModelClass domainModelClass, out string entityName, out object identifier)
        {
            var _entityName = "";
            object _identifier = null;
            entityName = _entityName;
            identifier = _identifier;
            if (!MustCheck(domainModelClass))
            {
                return false;
            }
            try
            {
                bool isStale = false;


#if NETFRAMEWORK
                var miniSessionManager = MiniSessionManager.InstanceSafe;
#else
                var miniSessionManager = ServiceLocator.Current.GetInstance<IMiniSessionService>();
#endif

                var entityInfo = GetEntityInfo(miniSessionManager, domainModelClass);
                if (!entityInfo.IsVersioned)
                {
                    return false;
                }
                _entityName = entityInfo.EntityType.Name;
                var tableName = entityInfo.ClassMetaData.RootTableName;
                string _entityID = entityInfo.ClassMetaData.IdentifierPropertyName;
                _identifier = (domainModelClass.GetType().GetProperty(_entityID)).GetValue(domainModelClass, null);
                string currentTimestampString = (domainModelClass.GetType().GetProperty("VersionTimestamp")).GetValue(domainModelClass, null)?.ToString();
                int currentTimestamp = 0;
                if (int.TryParse(currentTimestampString, out int cts))
                {
                    currentTimestamp = cts;
                }
                var sql = $"select 1 from {tableName} where {_entityID} = :idValue and VersionTimestamp > :currentTimestamp";
                var sqlQuery = miniSessionManager.Session.CreateSQLQuery(sql).SetParameter("idValue", _identifier).SetParameter("currentTimestamp", currentTimestamp);
                isStale = sqlQuery.List().Count > 0;

                entityName = _entityName;
                identifier = _identifier;
                return isStale;
            }
            catch (Exception e)
            {
                LogManager.GetLogger(typeof(StaleDataManager)).Error($"Error while examining Entity [{entityName}]", e);
                return false;
            }
        }
    }
}
