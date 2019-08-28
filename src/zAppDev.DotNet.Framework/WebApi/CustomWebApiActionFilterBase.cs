#if NETFRAMEWORK
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using zAppDev.DotNet.Framework.Data;
using zAppDev.DotNet.Framework.Logging;
using log4net;
using zAppDev.DotNet.Framework.Identity;

namespace zAppDev.DotNet.Framework.WebApi
{
    public class CustomWebApiActionFilterBase : ActionFilterAttribute
    {
        protected IAPILogger APILogger =>
            GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IAPILogger)) as IAPILogger;

        protected readonly Guid _id = Guid.NewGuid();
        protected TimeSpan _elapsed;

        public bool LogEnabled
        {
            get;
            set;
        }

        public bool AllowPartialResponse
        {
            get;
            set;
        }

        public override void OnActionExecuting(HttpActionContext context)
        {
            var timer = Stopwatch.StartNew();
            context.Request.Properties["logtimer"] = timer;
            context.Request.Properties["requestIsLogged"] = false;
            context.Request.Properties["exposed-service-requestId"] = _id;
            MiniSessionManager.Instance.OpenSessionWithTransaction();
            base.OnActionExecuting(context);
        }

        public override void OnActionExecuted(HttpActionExecutedContext context)
        {
            try
            {
                MiniSessionManager.Instance.CommitChanges(context.Exception);
                if (AllowPartialResponse) TryToCreatePartialResponse(context);
            }
            catch (Exception x)
            {
                context.Exception = x;
            }
            HandleException(context);
            var timer = (Stopwatch)context.Request.Properties["logtimer"];
            timer.Stop();
            _elapsed = timer.Elapsed;
            if (!LogEnabled) return;
            IdentityHelper.LogAction(
                context.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName,
                context.ActionContext.ActionDescriptor.ActionName,
                context.Exception == null,
                context.Exception?.Message);
            if (!(bool)context.Request.Properties["requestIsLogged"])
            {
                context.Request.Properties["requestIsLogged"] = true;
            }
        }

        protected static void HandleException(HttpActionExecutedContext filterContext)
        {
            if (filterContext.Exception == null) return;
            var actionDescriptor = filterContext.ActionContext.ActionDescriptor;
            LogManager.GetLogger(actionDescriptor.ControllerDescriptor.ControllerType)
                .Error(filterContext.Exception);
            try
            {
                var friendlyMessageHandler = new zAppDev.DotNet.Framework.Utilities.ExceptionHandler();
                var msgObject = friendlyMessageHandler.HandleException(filterContext.Exception);

                if (HttpContext.Current?.IsDebuggingEnabled == false)
                {
                    msgObject.OriginalStackTrace = null;
                    msgObject.Entries = null;
                }

                filterContext.Response = filterContext.Request.CreateResponse(HttpStatusCode.InternalServerError, msgObject);
            }
            catch (Exception e)
            {
                LogManager.GetLogger(actionDescriptor.ControllerDescriptor.ControllerType)
                    .Error("Could not produce friendly message for exception!", e);
                filterContext.Response = filterContext.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, filterContext.Exception);
            }
        }
        
        #region Partial Responses
        protected static void TryToCreatePartialResponse(HttpActionExecutedContext filterContext)
        {
            var fields = filterContext.Request.GetQueryNameValuePairs().FirstOrDefault(a => a.Key == "fields").Value?.Split(',');
            if (fields?.Any() != true || fields.First() == "*") return;
            var originalResult = filterContext.Response.Content as ObjectContent;
            if (originalResult == null) return;
            if (typeof(IEnumerable).IsAssignableFrom(originalResult.ObjectType))
            {
                var objects = ((IEnumerable<object>)originalResult.Value).Select(o => CreateShappedObject(o, fields)).ToList();
                filterContext.Response = CreateResponse(filterContext.Request, objects, "");
            }
            else
            {
                filterContext.Response = CreateResponse(filterContext.Request, CreateShappedObject(originalResult.Value, fields), "");
            }
        }

        protected static object CreateShappedObject(object obj, string[] lstFields)
        {
            if (!lstFields.Any())
            {
                return obj;
            }
            var objectToReturn = new ExpandoObject();
            foreach (var field in lstFields)
            {
                // TODO - Fix support nested objects!
                if (field.Contains("/"))
                {
                    throw new NotImplementedException();
                    /*
                                        var fieldParts = field.Split('/');
                                        var value = obj;
                                        foreach (var part in fieldParts)
                                        {
                                            value = obj.GetType()
                                                .GetProperty(part, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                                ?.GetValue(value, null);
                                        }
                                        ((IDictionary<string, object>)objectToReturn).Add(field, value);
                    */
                }
                else
                {
                    var fieldValue = obj.GetType()
                                     .GetProperty(field, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance)
                                     ?.GetValue(obj, null);
                    ((IDictionary<string, object>)objectToReturn).Add(field, fieldValue);
                }
            }
            return objectToReturn;
        }

        protected static HttpResponseMessage CreateResponse(HttpRequestMessage request, object obj, string key)
        {
            HttpResponseMessage response;
            try
            {
                var xmlMediaType = request.Headers.Accept != null && request.Headers.Accept.Any(a => a.MediaType == "application/xml");
                var fields = request.GetQueryNameValuePairs().FirstOrDefault(a => a.Key == "fields").Value?.Split(',');
                var fieldsDefined = fields?.Any() == true;
                if (xmlMediaType && fieldsDefined)
                {
                    // Remove Accept Header
                    response = request.CreateResponse(HttpStatusCode.NotAcceptable, obj, "application/json");
                }
                else
                {
                    response = request.CreateResponse(HttpStatusCode.OK, obj);
                }
            }
            catch (Exception e)
            {
                LogManager.GetLogger("CreateResponseFromFilter").Error($"Could not create response for [{key}].", e);
                return null;
            }
            return response;
        }
        #endregion
    }
}
#endif