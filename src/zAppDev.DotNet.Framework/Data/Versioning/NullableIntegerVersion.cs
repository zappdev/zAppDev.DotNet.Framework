using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.UserTypes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;

namespace zAppDev.DotNet.Framework.Data.Versioning
{
    public class NullableIntegerVersion : IUserVersionType
    {
        private static readonly int NULL = 1;

        private IComparer Comparator => Comparer<int?>.Default;

        private int TryParseDef(object value, int def)
        {
            if(int.TryParse(value.ToString(), out int result))
            {
                return result;
            }
            return def;
        }


        public SqlType[] SqlTypes => new[] { new SqlType(System.Data.DbType.Int32) };

        public Type ReturnedType => typeof(int?);

        public bool IsMutable => false;

        public object Assemble(object cached, object owner)
        {
            return TryParseDef(cached, NULL);
        }

        public int Compare(object x, object y)
        {
            return Comparator.Compare(x, y);
        }

        public object DeepCopy(object value)
        {
            return TryParseDef(value, NULL);
        }

        public object Disassemble(object value)
        {
            return TryParseDef(value, NULL);
        }

        public new bool Equals(object x, object y)
        {
            return Comparator.Compare(x, y) == 0;
        }

        public int GetHashCode(object x)
        {
            return TryParseDef(x, NULL).GetHashCode();
        }

        public object Next(object current, ISessionImplementor session)
        {
            return TryParseDef(current, NULL) + 1;
        }

        public object NullSafeGet(DbDataReader rs, string[] names, ISessionImplementor session, object owner)
        {
            return NHibernateUtil.Int32.NullSafeGet(rs, names[0], session, owner);
        }

        public void NullSafeSet(DbCommand cmd, object value, int index, ISessionImplementor session)
        {
            NHibernateUtil.Int32.NullSafeSet(cmd, TryParseDef(value, NULL), index, session);
        }

        public object Replace(object original, object target, object owner)
        {
            return original;
        }

        public object Seed(ISessionImplementor session)
        {
            return 1;
        }
    }
}
