using System.Collections.Generic;

namespace zAppDev.DotNet.Framework.Data.Domain
{
    public interface IDomainModelClass
    {
        List<string> _Validate(bool throwException = true);
    }
}
