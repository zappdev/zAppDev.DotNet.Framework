// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Serialization;
using NHibernate.Proxy;
using System.Linq;

namespace zAppDev.DotNet.Framework.Utilities
{
    public class NHibernateContractResolver : DefaultContractResolver
    {
        private bool _ignoreCalculatedKeyProperty;

        public NHibernateContractResolver(bool ignoreCalculatedKeys = false)
        {
            _ignoreCalculatedKeyProperty = ignoreCalculatedKeys;
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (typeof(INHibernateProxy).IsAssignableFrom(objectType))
                return base.CreateContract(objectType.BaseType);
            
            return base.CreateContract(objectType);
        }

        protected override List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            List<MemberInfo> members = null;

            if (typeof(INHibernateProxy).IsAssignableFrom(objectType)) {
                members = base.GetSerializableMembers(objectType.BaseType);
            }

            /*if (typeof (DateTime?).IsAssignableFrom(objectType)) {
                return new List<MemberInfo>();
            }*/

            members = base.GetSerializableMembers(objectType);

            return _ignoreCalculatedKeyProperty
                ? members.Where(p => p.Name != "_CalculatedKey").ToList()
                : members;
        }
    }
}