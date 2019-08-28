using Newtonsoft.Json;
using System;

#if NETFRAMEWORK
using System.Web.Mvc;
using System.Web.SessionState;
using zAppDev.DotNet.Framework.Tools.PerformanceMeasurements;
#else
using Microsoft.AspNetCore.Mvc;
#endif


namespace zAppDev.DotNet.Framework.Mvc
{
#if NETFRAMEWORK
    [SessionState(SessionStateBehavior.Required)]
#endif
    public class CustomControllerBase : Controller
    {
#if NETFRAMEWORK
        public PerformanceMonitor PerformanceMonitor;
#endif

        protected IControllerBase _parentController = null;

        public virtual void CommitAllFiles()
        {
            throw new NotImplementedException();
        }

        public virtual void CommitAllFilesLegacy()
        {
            throw new NotImplementedException();
        }

        public virtual void ClearPendingFiles()
        {
            throw new NotImplementedException();
        }

        public virtual ActionResult PreActionFilterHook(bool causesValidation, bool listenToEvent, string actionName)
        {
            return null;
        }

        public virtual ActionResult PostActionFilterHook(bool hasDefaultResultView, bool fillDropDownInitialValues = false)
        {
            return null;
        }

        public virtual void Log() {}
    }

    
}
