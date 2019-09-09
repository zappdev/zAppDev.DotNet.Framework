// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;

namespace zAppDev.DotNet.Framework.Mvc
{
    [Serializable]
    [DataContract]
    public class AjaxRequest<T>
    {
        [DataMember(Name = "_isDirty")]
        public bool IsDirty
        {
            get;
            set;
        }

        [DataMember(Name = "_suppressWarning")]
        public bool SuppressWarning
        {
            get;
            set;
        }

        [DataMember(Name = "model")]
        public T Model
        {
            get;
            set;
        }

        public JObject Raw
        {
            get;
            set;
        }
    }
}
