﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Hubs
{
    public interface IApplicationHub: IAccountHub
    {
        void RaiseApplicationStartEvent(string _groupName = null);
        void RaiseApplicationEndEvent(string _groupName = null);
        void RaiseApplicationErrorEvent(Exception exception, string _groupName = null);
        void RaiseSessionStartEvent(string _groupName = null);
        void RaiseOnInstanceSaveEvent(object Instance, string _groupName = null);
        void RaiseFileDownloadEvent(string path, string username, string _groupName = null);
        void ForceUserPageReloadEvent(string user);
    }
}
