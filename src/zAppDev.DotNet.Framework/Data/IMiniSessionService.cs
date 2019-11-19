// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using zAppDev.DotNet.Framework.Data.DAL;
using NHibernate;
using System;

namespace zAppDev.DotNet.Framework.Data
{
    public interface IMiniSessionService
    {
        RepositoryAction LastAction { get; set; }
        bool WillFlush { get; set; }
        ISession Session { get; }
        ISessionFactory SessionFactory { get; }

        ITransaction BeginTransaction();
        ISession OpenSession();
        ISession OpenSessionWithTransaction();

        void CloseSession();
        void Dispose();
        ISession OpenTransaction();

        void CommitChanges(Exception exception = null, Action postAction = null);

        T ExecuteInTransaction<T>(Func<T> func);

        void ExecuteInTransaction(Action action);
    }
}