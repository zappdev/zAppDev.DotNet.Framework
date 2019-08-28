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
