using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Powershell
{
    public class GenericInvocationResult<T>
    {
        public bool Successful { get; set; }
    }

    public class InvocationResult<T> : GenericInvocationResult<T>
    {        
        public T Result { get; set; }
    }

    public class InvocationResults<T> : GenericInvocationResult<T>
    {
        public List<T> Result { get; set; }
    }
}
