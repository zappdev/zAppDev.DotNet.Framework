using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CLMS.Framework.Logging;
using log4net;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Collection;
using NHibernate.Engine;
using NHibernate.Hql.Util;
using NHibernate.Proxy;

namespace CLMS.Framework.Data
{
    public class ObjectGraphWalker
    {
        #region fields
        /// <summary>
        /// A container of all mapped types
        /// with a list of their respective properties.
        /// </summary>
        private static readonly IDictionary<Type, List<FieldInfo>> FieldsToInitialise = new Dictionary<Type, List<FieldInfo>>();

        private List<object> _parsedObjects = new List<object>();

        #endregion

        public T AssociateGraphWithSession<T>(T entity, MiniSessionManager manager)
        {
            manager.Session.PrintGlobalStatistics();

            _parsedObjects = new List<object>();
            WalkAndReassociate(entity, manager.Session, entity.GetType().Name);

            manager.Session.PrintGlobalStatistics();
            return entity;
        }

        public bool? IsPersisted(object obj, ISession session)
        {
            try
            {
                var sessionFactoryImpl = (ISessionFactoryImplementor)session.SessionFactory;
                var persister = new SessionFactoryHelper(sessionFactoryImpl).RequireClassPersister(obj.GetType().FullName);
                return !persister.IsTransient(obj, (ISessionImplementor)session);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Search the object graph recursively for proxies,
        /// until a certain threshold has been reached.
        /// </summary>
        /// <param name="entity">The top node in the object graph where the search start.</param>
        /// <param name="session">The current session to the db</param>
        /// <param name="currPath">The current path</param>
        private object WalkAndReassociate(object entity, ISession session, string currPath)
        {
            if (null == entity) return null;

            if (_parsedObjects.Contains(entity))
            {
                return _parsedObjects.FirstOrDefault(a => a.Equals(entity));
            }

            var entityType = entity.GetType();
            var logger = LogManager.GetLogger(GetType());

            // Check if the entity is a collection.
            // If so, we must iterate the collection and
            // check the items in the collection. 
            // This will increase the depth level.
            var interfaces = entityType.GetInterfaces();
            if (interfaces.Any(iface => iface == typeof(IList)))
            {
                logger.Warn($"walking: {currPath}");
                entity = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(entity);
                var persistentCollection = entity as IPersistentCollection;
                if (persistentCollection != null && !persistentCollection.IsDirty)
                {
                    return entity;
                }

                var collection = (IList)entity;
                var idx = 0;
                var newArray = new object[collection.Count];
                foreach (var item in collection)
                {
                    newArray[idx] = WalkAndReassociate(item, session, currPath + $"[{idx}]");
                    idx++;
                }

                for (var index = 0; index < newArray.Length; index++)
                {
                    collection[index] = newArray[index];
                }

                return entity;
            }

            var known = session.Contains(entity);
            var isPersisted = IsPersisted(entity, session);

            if (isPersisted == null)
            {
                // Unknown obj
                // TODO - iterate the members
                return entity;
            }

            if (!known && isPersisted == true)
            {
                // Refresh
                entity = session.Merge(entity);
                known = true;
            }

            var identifier = known ? session.GetIdentifier(entity) : null;
            logger.Warn($"walking: {currPath}{(known ? "(" + identifier + ")" : "")}");

            _parsedObjects.Add(entity);

            if (!session.IsDirtyEntity(entity))
            {
                // Refresh and return
                return entity;
            }

            if (!FieldsToInitialise.ContainsKey(entityType))
            {
                // Unknown (i.e.: not-mapped or with no participant props) type

                // TODO - Log and Skip for now. Eventually walk all complex properties to find mapped classes
                logger.Warn($"Unknown (i.e.: not-mapped or with no participant props) type: {entityType}. Skipping...");
                return entity;
                /*
                                var properties = entityType.GetProperties();
                                foreach (var p in properties)
                                {

                                }
                */
            }

            // If we get here, then we know that we are
            // not working with a collection, and that the entity
            // holds properties we must search recursively.
            // We are only interested in properties with NHAttributes.
            // Maybe there is a better way to specify this
            // in the GetProperties call (so that we only get an array
            // of PropertyInfo's that have NH mappings).
            var fields = FieldsToInitialise[entityType];
            foreach (var field in fields)
            {
                var proxy = field.GetValue(entity);
                if (null == proxy) continue;
                proxy = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(proxy);

                /*if (!session.IsDirtyProperty(entity, field.Name))
                {
                    continue;
                }
                */
                try
                {
                    var associatedObject = WalkAndReassociate(proxy, session, currPath + $".{field.Name}");
                    field.SetValue(entity, associatedObject);
                }
                catch (LazyInitializationException e)
                {
                    logger.Warn($"LazyInitializationException while walking prop: {field.Name} of {entityType}.", e);
                    // Refresh and retry
                    session.Refresh(entity);

                    proxy = field.GetValue(entity);
                    if (null == proxy) continue;
                    proxy = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(proxy);
                    WalkAndReassociate(proxy, session, currPath + $".{field.Name}");
                }
            }

            // update the entity in the `cache`
            // this works because the remove uses .Equals to find the obj,
            // which does not look at the reference
            _parsedObjects.Remove(entity);
            _parsedObjects.Add(entity);
            return entity;
        }

        #region Helpers

        /// <summary>
        /// Search the object graph recursively for proxies,
        /// until a certain threshold has been reached.
        /// </summary>
        /// <param name="entity">The top node in the object
        /// graph where the search start.</param>
        /// <param name="depth">The current depth from
        /// the top node (which is depth 0)</param>
        /// <param name="maxDepth">The max search depth.</param>
        /// <param name="loadGraphCompletely">Bool flag indicating
        /// whether to ignore depth params</param>
        /// <param name="session">The current session to the db</param>
        private void WalkNHMappedProperties(object entity, int depth, int maxDepth, bool loadGraphCompletely, ISession session)
        {
            bool search;
            if (loadGraphCompletely) search = true;
            else search = depth <= maxDepth;

            if (null == entity) return;

            // Should we stay or should we go now?
            if (!search) return;

            if (_parsedObjects.Contains(entity))
            {
                return;
            }

            _parsedObjects.Add(entity);

            var entityType = entity.GetType();

            // Check if the entity is a collection.
            // If so, we must iterate the collection and
            // check the items in the collection. 
            // This will increase the depth level.
            var interfaces = entityType.GetInterfaces();
            if (interfaces.Any(iface => iface == typeof(ICollection)))
            {
                entity = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(entity);
                var collection = (ICollection)entity;
                foreach (var item in collection)
                    WalkNHMappedProperties(item, depth + 1, maxDepth, loadGraphCompletely, session);
                return;
            }

            if (!FieldsToInitialise.ContainsKey(entityType))
            {
                // Unknown (i.e.: not-mapped) type

                // TODO - Log and Skip for now. Eventually walk all complex properties to find mapped classes
                LogManager.GetLogger(GetType()).Warn($"Unknown (i.e.: not-mapped) type: {entityType}. Skipping...");
                return;
                /*
                                var properties = entityType.GetProperties();
                                foreach (var p in properties)
                                {

                                }
                */
            }

            // If we get here, then we know that we are
            // not working with a collection, and that the entity
            // holds properties we must search recursively.
            // We are only interested in properties with NHAttributes.
            // Maybe there is a better way to specify this
            // in the GetProperties call (so that we only get an array
            // of PropertyInfo's that have NH mappings).
            var fields = FieldsToInitialise[entityType];
            foreach (var field in fields)
            {
                var proxy = field.GetValue(entity);
                if (null == proxy) continue;
                proxy = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(proxy);

                try
                {
                    WalkNHMappedProperties(proxy, depth + 1, maxDepth, loadGraphCompletely, session);
                }
                catch (LazyInitializationException e)
                {
                    LogManager.GetLogger(GetType()).Warn($"LazyInitializationException while walking prop: {field.Name} of {entityType}.", e);
                    // Refresh and retry
                    session.Refresh(entity);

                    proxy = field.GetValue(entity);
                    if (null == proxy) continue;
                    proxy = session.GetSessionImplementation().PersistenceContext.UnproxyAndReassociate(proxy);
                    WalkNHMappedProperties(proxy, depth + 1, maxDepth, loadGraphCompletely, session);
                }
            }
        }


        /// <summary>
        /// Search the object graph recursively for proxies,
        /// until a certain threshold has been reached.
        /// </summary>
        /// <param name="entity">The top node in the object
        /// graph where the search start.</param>
        /// <param name="depth">The current depth from
        /// the top node (which is depth 0)</param>
        /// <param name="maxDepth">The max search depth.</param>
        /// <param name="loadGraphCompletely">Bool flag indicating
        /// whether to ignore depth params</param>
        /// <param name="session">The current session to the db</param>
        private void ExtractNHMappedProperties(object entity, int depth,
            int maxDepth, bool loadGraphCompletely, ISession session)
        {
            bool search;
            if (loadGraphCompletely) search = true;
            else search = (depth <= maxDepth);

            if (null == entity) return;

            // Should we stay or should we go now?
            if (!search) return;

            // Check if the entity is a collection.
            // If so, we must iterate the collection and
            // check the items in the collection. 
            // This will increase the depth level.
            var interfaces = entity.GetType().GetInterfaces();
            if (interfaces.Any(iface => iface == typeof(ICollection)))
            {
                var collection = (ICollection)entity;
                foreach (var item in collection)
                    ExtractNHMappedProperties(item, depth + 1, maxDepth, loadGraphCompletely, session);
                return;
            }

            // If we get here, then we know that we are
            // not working with a collection, and that the entity
            // holds properties we must search recursively.
            // We are only interested in properties with NHAttributes.
            // Maybe there is a better way to specify this
            // in the GetProperties call (so that we only get an array
            // of PropertyInfo's that have NH mappings).
            var fields = FieldsToInitialise[entity.GetType()];
            foreach (var field in fields)
            {
                var proxy = field.GetValue(entity);
                if (!NHibernateUtil.IsInitialized(proxy))
                {
                    LazyInitialise(proxy, entity, session);
                }

                if (null != proxy)
                    ExtractNHMappedProperties(proxy, depth + 1, maxDepth,
                        loadGraphCompletely, session);
            }
        }

        /// <summary>
        /// The core method delegating the hard lazy initialization
        /// work to the hibernate assemblies.
        /// </summary>
        /// <param name="proxy">The proxy to load</param>
        /// <param name="owner">The owning
        /// entity holding the reference</param>
        /// <param name="session">The current session to the db</param>
        private static void LazyInitialise(object proxy, object owner, ISession session)
        {
            if (null == proxy) return;
            var interfaces = proxy.GetType().GetInterfaces();
            foreach (var iface in interfaces)
            {
                if (iface != typeof(INHibernateProxy) && iface != typeof(IPersistentCollection)) continue;

                if (!NHibernateUtil.IsInitialized(proxy))
                {
                    session.Lock(iface == typeof(INHibernateProxy) ? proxy : owner, LockMode.None);
                    NHibernateUtil.Initialize(proxy);
                }

                break;
            }
        }

        public static object DeProxify(object obj)
        {
            if (obj is INHibernateProxy)
            {
                NHibernateUtil.Initialize(obj);
            }
            return obj;
        }
        #endregion

        /// <summary>
        /// Initializes the <see cref="ObjectGraphWalker"/> class.
        /// </summary>
        public static void Initialize(Configuration cfg)
        {
            // get all types having
            // many/one-to-one properties
            var toOneQuery = from persistentClass in cfg.ClassMappings
                             let props = persistentClass.PropertyClosureIterator
                             select new { persistentClass.MappedClass, props }
                into selection
                             from prop in selection.props
                             where prop.Value is NHibernate.Mapping.ToOne
                             //where ((NHibernate.Mapping.ToOne)prop.Value).IsLazy
                             group selection.MappedClass.GetField(prop.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
                             by selection.MappedClass;
            // get all types (with their props) having nh bag properties
            var bagQuery = from persistentClass in cfg.ClassMappings
                           let props = persistentClass.PropertyClosureIterator
                           select new { persistentClass.MappedClass, props }
                into selection
                           from prop in selection.props
                           where prop.Value is NHibernate.Mapping.Collection
                           //where ((NHibernate.Mapping.Collection)prop.Value).IsLazy
                           group selection.MappedClass.GetField(prop.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.NonPublic)
                           by selection.MappedClass;
            // TODO: add queries of any other
            // mapping attribute you use that might be lazy.

            foreach (var value in toOneQuery)
                FieldsToInitialise.Add(value.Key, value.ToList());
            foreach (var value in bagQuery)
            {
                if (FieldsToInitialise.ContainsKey(value.Key))
                    FieldsToInitialise[value.Key].AddRange(value.ToList());
                else
                    FieldsToInitialise.Add(value.Key, value.ToList());
            }
            // TODO: add treatment of any other mapping
            // attribute you use that might be lazy.
        }
    }
}
