using log4net;
using NHibernate.Persister.Entity;
using System;
using System.Collections.Generic;

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
        }

        private static Dictionary<Domain.IDomainModelClass, EntityInfo> entityInfos = new Dictionary<Domain.IDomainModelClass, EntityInfo>();

        private static bool MustCheck(Domain.IDomainModelClass cls)
        {
            if (entityInfos.ContainsKey(cls) && !entityInfos[cls].IsVersioned)
            {
                return false;
            }
            return true;
        }

#if NETFRAMEWORK
        private static EntityInfo GetEntityInfo(MiniSessionManager manager, Domain.IDomainModelClass domainModelClass)
#else
        private static EntityInfo GetEntityInfo(MiniSessionService manager, Domain.IDomainModelClass domainModelClass)
#endif
        {
            EntityInfo entityInfo;
            if (entityInfos.ContainsKey(domainModelClass))
            {
                entityInfo = entityInfos[domainModelClass];
            }
            else
            {
                entityInfo = new EntityInfo();
                entityInfo.EntityType = domainModelClass.GetType();
                entityInfo.ClassMetaData = ((AbstractEntityPersister)manager.Session.SessionFactory.GetAllClassMetadata()[entityInfo.EntityType.FullName]);
                entityInfo.IsVersioned = entityInfo.ClassMetaData?.IsVersioned == true && entityInfo.ClassMetaData?.VersionType?.Name == "Int32";
                entityInfos.Add(domainModelClass, entityInfo);
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
                MiniSessionManager.ExecuteInUoW(manager =>
                {
                    var entityInfo = GetEntityInfo(manager, domainModelClass);
                    if (!entityInfo.IsVersioned)
                    {
                        return;
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
                    var sqlQuery = manager.Session.CreateSQLQuery(sql).SetParameter("idValue", _identifier).SetParameter("currentTimestamp", currentTimestamp);
                    isStale = sqlQuery.List().Count > 0;
                });
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
