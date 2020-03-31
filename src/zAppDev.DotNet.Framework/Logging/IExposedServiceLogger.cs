using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace zAppDev.DotNet.Framework.Logging
{
    public interface IExposedServiceLogger
    {

        void LogExposedAPIMetadata(ExposedServiceMetadataStruct metadata);
    }
}
