using log4net;
using NHibernate;
using NHibernate.Event;
using NHibernate.Persister.Collection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using CLMS.Framework.Data;
using CLMS.Framework.Auditing.Model;
using CLMS.Framework.Utilities;
using CLMS.Framework.Data.DAL;

namespace CLMS.Framework.Auditing
{
    public class NHAuditTrailManager : INHAuditTrailManager
    {
        private readonly ILog _log = LogManager.GetLogger(typeof(NHAuditTrailManager));
        private const string NoValueString = "*No Value*";
        private Dictionary<string, AuditableEntity> _auditableEntities;
        private Dictionary<string, AuditLogEntryType> _auditLogEntryTypes;
        private Dictionary<string, AuditLogPropertyActionType> _auditLogPropertyActionTypes;
        private Func<AuditContext> _getAuditContext;
        private bool _isEnabled;

        public bool IsTemporarilyDisabled
        {
            get;
            set;
        } = false;

        public NHAuditTrailManager()
        {

        }

        public void Enable(List<Type> auditableTypes, Func<AuditContext> getAuditContext)
        {
            _getAuditContext = getAuditContext;
            AuditEntityConfiguration.SetAuditableTypes(auditableTypes);
            InitializeConfiguration();
            _isEnabled = true;
        }

        public static INHAuditTrailManager GetInstance()
        {
            return ServiceLocator.Current.GetInstance<INHAuditTrailManager>();
        }

        public void ClearAuditTrailCache()
        {            
            MiniSessionManager.ExecuteInUoW(manager =>
            {
                var repo = ServiceLocator.Current.GetInstance<IRepositoryBuilder>().CreateCreateRepository(manager);

                _auditableEntities = new Dictionary<string, AuditableEntity>();
                repo.GetAll<AuditEntityConfiguration>()
                    .ForEach(entity =>
                    {
                        var auditableEntity = new AuditableEntity {Name = entity.FullName};
                        entity.Properties
                            .Where(property => property.IsAuditable)
                            .ToList()
                            .ForEach(property => auditableEntity.Properties.Add(property.Name, new AuditableProperty
                            {
                                Name = property.Name,
                                Datatype = property.DataType,
                                IsComplex = property.IsComplex,
                                IsCollection = property.IsCollection
                            }));
                        if (!auditableEntity.Properties.Any()) return;
                        _log.Debug($@"Monitoring object: [{entity.ShortName}]. Monitored Properties:
                                                                * {string.Join("\r\n* ", auditableEntity.Properties.Keys)}");
                        _auditableEntities.Add(entity.FullName, auditableEntity);
                    });

                _auditLogEntryTypes = new Dictionary<string, AuditLogEntryType>();
                repo.GetAll<AuditLogEntryType>()
                    .ForEach(type => _auditLogEntryTypes.Add(type.Code, type));

                _auditLogPropertyActionTypes = new Dictionary<string, AuditLogPropertyActionType>();
                repo.GetAll<AuditLogPropertyActionType>()
                    .ForEach(type => _auditLogPropertyActionTypes.Add(type.Code, type));
            });
        }

        private void InitializeConfiguration()
        {
            MiniSessionManager.ExecuteInUoW(manager =>
            {
                var repo = ServiceLocator.Current.GetInstance<IRepositoryBuilder>().CreateAuditingRepository(manager);

                var oldEntityConfigurations = repo.GetAll<AuditEntityConfiguration>();

                List<AuditEntityConfiguration> newEntityConfigurations = AuditEntityConfiguration.GetAllEntityConfigurations();
                newEntityConfigurations.ForEach(newEntityConfiguration =>
                {
                    var item = oldEntityConfigurations.FirstOrDefault(c => c.FullName == newEntityConfiguration.FullName);
                    if (item == null)
                    {
                        repo.Save(newEntityConfiguration);
                    }
                    else
                    {
                        item.UpdateAuditEntityConfiguration(newEntityConfiguration, repo);
                    }
                });

                oldEntityConfigurations.ForEach(oldEntityConfiguration =>
                {
                    var item = newEntityConfigurations.FirstOrDefault(c => c.FullName == oldEntityConfiguration.FullName);
                    if (item == null) repo.DeleteAuditEntityConfiguration(oldEntityConfiguration);
                });

                var auditLogEntryTypes = repo.GetAll<AuditLogEntryType>();
                if (!auditLogEntryTypes.Any())
                {
                    repo.Save(new AuditLogEntryType { Code = "CREATE", Name = "Create" });
                    repo.Save(new AuditLogEntryType { Code = "UPDATE", Name = "Update" });
                    repo.Save(new AuditLogEntryType { Code = "DELETE", Name = "Delete" });
                    auditLogEntryTypes = repo.GetAll<AuditLogEntryType>();
                }

                var auditLogPropertyActionTypes = repo.GetAll<AuditLogPropertyActionType>();
                var addAction = auditLogPropertyActionTypes.FirstOrDefault(a => a.Code == "ADD");
                if (addAction == null)
                {
                    repo.Save(new AuditLogPropertyActionType { Code = "ADD", Name = "Added Element To Collection" });
                }
                var removeAction = auditLogPropertyActionTypes.FirstOrDefault(a => a.Code == "REMOVE");
                if (removeAction == null)
                {
                    repo.Save(new AuditLogPropertyActionType { Code = "REMOVE", Name = "Removed Element From Collection" });
                }
                var assignAction = auditLogPropertyActionTypes.FirstOrDefault(a => a.Code == "ASSIGN");
                if (assignAction == null)
                {
                    repo.Save(new AuditLogPropertyActionType { Code = "ASSIGN", Name = "Assign Entity" });
                }
            });

            ClearAuditTrailCache();
        }

        private void PostDatabaseOperationEvent(IPostDatabaseOperationEventArgs @event, object[] newState, object[] oldState, string type)
        {
            if (IsTemporarilyDisabled) return;
            if (!_isEnabled) return;
            if (@event == null || @event.Entity is AuditLogEntry)
                return;
            // Is the Entity Auditable?
            var fullName = @event.Entity.GetType().FullName;
            if (!_auditableEntities.ContainsKey(fullName))
                return;
            //var session = @event.Session.GetSession(EntityMode.Poco);
            var auditableEntity = _auditableEntities[fullName];
            var username = _getAuditContext().Username;
            if (type == "DELETE")
            {
                var entry = new AuditLogEntry
                {
                    EntityShortName = @event.Entity.GetType().Name,
                    EntityFullName = fullName,
                    UserName = username,
                    EntityId = @event.Id.ToString(),
                    EntryTypeId = _auditLogEntryTypes[type].Id,
                    Timestamp = DateTime.UtcNow
                };
                using (var session = @event.Session.SessionWithOptions().OpenSession())
                {
                    session.Save(entry);
                    session.Flush();
                }
                return;
            }
            var propertyIndexes = oldState != null
                                  ? @event.Persister.FindDirty(newState, oldState, @event.Entity, @event.Session)
                                  : new int[0];
            using (var session = @event.Session.SessionWithOptions().OpenSession())
            {
                for (var position = 0; position < @event.Persister.PropertyNames.Length; ++position)
                {
                    if (@event.Persister.VersionProperty == position)
                    {
                        continue;
                    }
                    // Ignore not Dirty props in case of Update
                    if (oldState != null && propertyIndexes != null && !propertyIndexes.Contains(position))
                    {
                        continue;
                    }
                    var key = @event.Persister.PropertyNames[position];
                    // Is the Property Auditable?
                    if (!auditableEntity.Properties.ContainsKey(key))
                        continue;
                    // Ignore collections
                    var property = auditableEntity.Properties[key];
                    if (property.IsCollection)
                        continue;
                    // Ignore simple properties when CREATE
                    if (type == "CREATE" && !property.IsComplex)
                    {
                        continue;
                    }
                    // Create AuditLogEntry
                    var oldValue = GetStringValueFromStateArray(oldState, position, property, @event.Session);
                    var newValue = GetStringValueFromStateArray(newState, position, property, @event.Session);
                    // No changes, skip.
                    if (oldValue == newValue)
                    {
                        continue;
                    }
                    // Create Entry
                    var entry = new AuditLogEntry
                    {
                        EntityShortName = @event.Entity.GetType().Name,
                        EntityFullName = fullName,
                        UserName = username,
                        EntityId = @event.Id.ToString(),
                        EntryTypeId = _auditLogEntryTypes[type].Id,
                        Timestamp = DateTime.UtcNow,
                        PropertyName = key,
                        ActionTypeId = property.IsComplex ? _auditLogPropertyActionTypes["ASSIGN"].Id : null,
                        OldValue = oldValue ?? NoValueString,
                        NewValue = newValue ?? NoValueString,
                    };
                    session.Save(entry);
                }
                session.Flush();
            }
        }

        public void OnPostInsertLogEvent(PostInsertEvent @event)
        {
            PostDatabaseOperationEvent(@event, @event.State, null, "CREATE");
        }

        public void OnPostUpdateLogEvent(PostUpdateEvent @event)
        {
            PostDatabaseOperationEvent(@event, @event.State, @event.OldState, "UPDATE");
        }

        public void OnPostDeleteLogEvent(PostDeleteEvent @event)
        {
            PostDatabaseOperationEvent(@event, null, @event.DeletedState, "DELETE");
        }

        public void OnPreCollectionUpdateLogEvent(PreCollectionUpdateEvent @event)
        {
            if (IsTemporarilyDisabled) return;
            if (!_isEnabled) return;
            if (@event == null || @event.AffectedOwnerOrNull is AuditLogEntry)
                return;
            // Is the Parent Entity of the collection Auditable?
            var affectedOwnerEntityName = @event.GetAffectedOwnerEntityName();
            if (!_auditableEntities.ContainsKey(affectedOwnerEntityName))
                return;
            // Is the Collection Auditable?
            var auditableEntity = _auditableEntities[affectedOwnerEntityName];
            var role = @event.Collection.Role;
            var key = role.Substring(role.LastIndexOf(".", StringComparison.Ordinal) + 1);
            if (!auditableEntity.Properties.ContainsKey(key))
                return;
            var auditableProperty = auditableEntity.Properties[key];
            var classMetadata = @event.Session.SessionFactory.GetClassMetadata(auditableProperty.Datatype);
            var identifierPropertyName = classMetadata.IdentifierPropertyName;
            var collectionPersister = @event.Session.SessionFactory.GetCollectionMetadata(role) as ICollectionPersister;
            if (collectionPersister == null)
            {
                throw new Exception("CollectionPersister is null. Not sure why.");
            }
            var type = Type.GetType(auditableProperty.Datatype + ", cfTests.BO");
            if (!(type != null))
                return;
            if (collectionPersister.IsInverse)
            {
                _log.Debug($"Collection [{key}] of entity [{affectedOwnerEntityName}] is inverse!");
                return;
            }
            var session = @event.Session.GetSession(EntityMode.Poco);
            var username = _getAuditContext().Username;
            var shouldFlush = false;
            foreach (var obj1 in @event.Collection.GetDeletes(collectionPersister, false))
            {
                var propertyInfo = type.GetProperty(identifierPropertyName);
                if (propertyInfo == null) continue;
                var keyValue = propertyInfo.GetValue(obj1);
                var entry = new AuditLogEntry
                {
                    EntityShortName = @event.AffectedOwnerOrNull.GetType().Name,
                    EntityFullName = affectedOwnerEntityName,
                    UserName = username,
                    EntityId = @event.AffectedOwnerIdOrNull.ToString(),
                    EntryTypeId = _auditLogEntryTypes["UPDATE"].Id,
                    Timestamp = DateTime.UtcNow,
                    ActionTypeId = _auditLogPropertyActionTypes["REMOVE"].Id,
                    PropertyName = key,
                    OldValue = keyValue.ToString()
                };
                session.Save(entry);
                shouldFlush = true;
            }
            var i = -1;
            if (@event.Collection.IsDirty)
            {
                foreach (var entryAdded in @event.Collection.Entries(collectionPersister))
                {
                    ++i;
                    var elemType = NHibernateUtil.GetSerializable(type);
                    if (!@event.Collection.NeedsInserting(entryAdded, i, elemType)) continue;
                    var propertyInfo = type.GetProperty(identifierPropertyName);
                    if (propertyInfo == null) continue;
                    var keyValue = propertyInfo.GetValue(entryAdded);
                    var entry = new AuditLogEntry
                    {
                        EntityShortName = @event.AffectedOwnerOrNull.GetType().Name,
                        EntityFullName = affectedOwnerEntityName,
                        UserName = username,
                        EntityId = @event.AffectedOwnerIdOrNull.ToString(),
                        EntryTypeId = _auditLogEntryTypes["UPDATE"].Id,
                        Timestamp = DateTime.UtcNow,
                        ActionTypeId = _auditLogPropertyActionTypes["ADD"].Id,
                        PropertyName = key,
                        NewValue = keyValue.ToString()
                    };
                    session.Save(entry);
                    shouldFlush = true;
                }
                if (i > 10)
                {
                    _log.Warn($"Parsed {i} Entities for Collection [{key}] of entity [{affectedOwnerEntityName}]!");
                }
            }
            else
            {
                _log.Info($"Collection [{key}] of entity [{affectedOwnerEntityName}] is not Dirty. Skipping looking for inserted entries.");
            }
            if (shouldFlush)
            {
                // Flush only once
                session.Flush();
            }
        }

        private static string GetStringValueFromStateArray(IList<object> stateArray, int position, AuditableProperty property, IEventSource session)
        {
            var value = stateArray?[position];
            if (value == null)
                return NoValueString;
            if (!property.IsComplex)
                return value.ToString() == string.Empty ? NoValueString : value.ToString();
            var type = Type.GetType(property.Datatype + ", cfTests.BO");
            if (type == null)
                return value.ToString() == string.Empty ? NoValueString : value.ToString();
            var identifierPropertyName = session.SessionFactory.GetClassMetadata(property.Datatype).IdentifierPropertyName;
            var propertyInfo = type.GetProperty(identifierPropertyName);
            if (propertyInfo == null) return string.Empty;
            var keyValue = propertyInfo.GetValue(value);
            return keyValue == null || keyValue.ToString() == string.Empty ? NoValueString : keyValue.ToString();
        }

    }

    public class AuditableEntity
    {
        public Dictionary<string, AuditableProperty> Properties = new Dictionary<string, AuditableProperty>();
        public string Name;
    }

    public class AuditableProperty
    {
        public string Name;
        public string Datatype;
        public bool IsComplex;
        public bool IsCollection;
    }

    public class AuditContext
    {
        public string Username;
    }
}
