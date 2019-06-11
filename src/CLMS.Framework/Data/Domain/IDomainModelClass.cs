using System.Collections.Generic;

namespace CLMS.Framework.Data.Domain
{
    public interface IDomainModelClass
    {
        List<string> _Validate(bool throwException = true);
    }
}
