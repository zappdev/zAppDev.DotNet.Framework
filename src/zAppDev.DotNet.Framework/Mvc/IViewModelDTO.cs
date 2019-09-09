// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
namespace zAppDev.DotNet.Framework.Mvc
{
    public interface IViewModelDTO<T>
    {     
        T Convert();
		object _key { get; }
        object _clientKey { get; set; }
    }
}