// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Data.DAL
{
    public interface ICreateRepository: IRetrieveRepository
    {
        void SaveWithoutTransaction<T>(T entity) where T : class;

        void Save<T>(T entity) where T : class;

        void Insert<T>(T entity) where T : class;
    }
}
