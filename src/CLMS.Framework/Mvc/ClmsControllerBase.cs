using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Ionic.Zip;
using System.IO;
using Newtonsoft.Json;

#if NETFRAMEWORK
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
#else
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Http;
using CLMS.Framework.Utilities;
#endif

using CLMS.Framework.Identity;

namespace CLMS.Framework.Mvc
{
    public interface IControllerBase
    {
        bool IsDirty
        {
            get;
            set;
        }
    }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public enum MessageType
    {
        Success = 0,
        Error,
        Warning,
        Info
    }

    public class ClmsControllerBase<T, Y> : CustomControllerBase, IControllerBase
    {
        protected T @model;
        public ViewDTO viewDTO = new ViewDTO();

        protected bool _redirectionFromSameController = false; //TODO: Fill this
        protected log4net.ILog _logger;

        public bool IsDirty
        {
            get;
            set;
        }
        
        protected static Dictionary<int, ViewDTO> _viewDTOsDic = new Dictionary<int, ViewDTO>();

        protected static int AddToViewDTOsDicAndGetHash(ViewDTO dto)
        {
            var hash = dto.GetHashCode();
            if (_viewDTOsDic.ContainsKey(hash))
            {
                _viewDTOsDic[hash] = dto;
            }
            else
            {
                _viewDTOsDic.Add(hash, dto);
            }
            return hash;
        }

        protected static ViewDTO GetFromViewDTOsDic(int hash, bool remove = true)
        {
            if (!_viewDTOsDic.ContainsKey(hash)) return null;
            var dto = _viewDTOsDic[hash];
            if (remove)
            {
                _viewDTOsDic.Remove(hash);
            }
            return dto;
        }

        protected static string GetViewFromViewDTOsDicSerialized(int hash)
        {
            var dto = GetFromViewDTOsDic(hash);
            return dto == null
                   ? "null"
                   : dto.Serialize();
        }

        public T GetModel() => @model;

#if NETFRAMEWORK
        private string GetFormNameFromUlr(HttpRequestBase request)
        {
            return GetFormNameFromUlr(request.RawUrl.Replace(Request.ApplicationPath, ""));
        }
#else
        private string GetFormNameFromUlr(HttpRequest request)
        {
            return GetFormNameFromUlr(request.RawUrl().Replace(Web.GetApplicationPath(), ""));
        }
#endif

        private string GetFormNameFromUlr(string url)
        {
            if (string.IsNullOrEmpty(url)) return "";
            var splittedUrl = url.Split(new[] {"/"}, StringSplitOptions.RemoveEmptyEntries);
            return !splittedUrl.Any() ? "" : splittedUrl[0];
        }

        protected void PushToHistory()
        {
            ClientCommand(ClientCommands.PUSH_TO_NAVIGATION_HISTORY);
        }

        protected ActionResult CloseForm()
        {
            if (Request.IsAjaxRequest())
            {
#if NETFRAMEWORK
                var result = Json(new
                {
                    Type = "Redirect",
                    Url = ClientCommands.CLOSE_FORM.ToString()
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
#else
                var result = Json(new
                {
                    Type = "Redirect",
                    Url = ClientCommands.CLOSE_FORM.ToString()
                });
#endif
                return result;
            }
            else
            {
                ClientCommand(ClientCommands.CLOSE_FORM);
                return null;
            }
        }

#if NETFRAMEWORK
        protected JObject _ParsePostedData()
        {
            if (Request.Files.Count > 0)
            {
                var postedData = new JObject();
                foreach (var key in Request.Form.AllKeys)
                {
                    postedData.Add(key, Request.Form[key]);
                }
                return postedData;
            }
            else
            {
                Request.InputStream.Position = 0;
                var json = "";
                if (Request.Headers["IsZipped"] == "true")
                {
                    json = Unzip(Request.InputStream);
                }
                else
                {
                    //json = new StreamReader(new MemoryStream(HttpContext.Request.BinaryRead(HttpContext.Request.ContentLength))).ReadToEnd();
                    using (var ms = new MemoryStream(HttpContext.Request.BinaryRead(HttpContext.Request.ContentLength)))
                    {
                        using (var sr = new StreamReader(ms))
                        {
                            json = sr.ReadToEnd();
                        }
                    }
                }
                return JObject.Parse(json);
            }
        }
#else
        protected JObject _ParsePostedData()
        {
            if (Request.Form.Files.Count > 0)
            {
                var postedData = new JObject();
                foreach (var key in Request.Form.Keys)
                {
                    postedData.Add(key, Request.Form[key].ToString());
                }
                return postedData;
            }
            else
            {
                Request.Body.Position = 0;
                var json = "";
                if (Request.Headers["IsZipped"] == "true")
                {
                    json = Unzip(Request.Body);
                }
                else
                {
                    //json = new StreamReader(new MemoryStream(HttpContext.Request.BinaryRead(HttpContext.Request.ContentLength))).ReadToEnd();
                    using (var sr = new StreamReader(HttpContext.Request.Body))
                    {
                        json = sr.ReadToEnd();
                    }
                }
                return JObject.Parse(json);
            }
        }
#endif

        protected static string Unzip(Stream stream)
        {
            using (ZipFile zipFile = ZipFile.Read(stream))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    zipFile["form.data"].Extract(output);
                    return System.Text.Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }

        protected JObject _LoadViewModel()
        {
            var postedData = _ParsePostedData();
            if (postedData["_isDirty"] != null)
            {
                IsDirty = (bool) (((JValue) (postedData["_isDirty"])).Value);
            }
            if (postedData["model"] != null)
            {
                var serializedData = postedData["model"].ToString();
                var _vm = (IViewModelDTO<T>) Utilities.Deserialize<Y>(serializedData);
                @model = _vm.Convert();
            }
            else
            {
                var type = typeof(T);
                var ctor = type.GetConstructor(new Type[] {});
                @model = (T) ctor.Invoke(new object[] {});
            }
            ViewModelLoaded();
            return postedData;
        }

        protected S ParseProperty<S>(JObject postedData, string parameterName)
        {
            return Utilities.Deserialize<S>(postedData[parameterName].ToString());
        }

        protected virtual void ViewModelLoaded()
        {
        }

        protected JsonResult GetDataValidationsEvaluationResult(EvalTime evalTime)
        {
#if NETFRAMEWORK
            var result = Json(new
            {
                Type = "RuleEvaluation",
                Data = Serialize(new
                {
                    RuleEvaluations = viewDTO.RuleEvaluations
                })
            }, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
#else
            var result = Json(new
            {
                Type = "RuleEvaluation",
                Data = Serialize(new
                {
                    RuleEvaluations = viewDTO.RuleEvaluations
                })
            });
#endif
            return result;
        }

        protected JsonResult PrepareUpdateInstanceResult(Type viewModelType, Type[] partialTypes = null)
        {
            var parsedData = _ParsePostedData();
            var keys =
                parsedData["keys"].ToString().Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries).ToList();
            var jbID = parsedData["jbID"]?.ToString();
            var dataType = parsedData["dataType"].ToString();
            var data = CreateDtoInstancesFromKeys(viewModelType, dataType, keys, jbID, partialTypes);

#if NETFRAMEWORK
            var result = Json(new {Type = "UpdateInstance", Data = Utilities.Serialize(data)},
                              JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
#else
            var result = Json(new { Type = "UpdateInstance", Data = Utilities.Serialize(data) });
#endif
            return result;
        }

        protected object CreateDtoInstancesFromKeys(Type viewModelType, string dataType, List<string> keys, string jbID, Type[] partialTypes = null)
        {
            _logger = log4net.LogManager.GetLogger(typeof(ControllerBase));
            var getInstanceMethod = GetViewModelTypeForPartialControl(viewModelType, dataType, partialTypes);
            object data;
            if (keys.Count() == 1)
            {
                if (string.IsNullOrEmpty(keys[0]) || keys[0] == "0")
                {
                    data = null;
                }
                else
                {
                    try
                    {
                        data = getInstanceMethod.Invoke(null, new object[] { (keys[0]), (jbID) });
                    }
                    catch (Exception ex)
                    {
                        data = null;
                        _logger.Error(ex);
                    }
                }
            }
            else
            {
                var instances = new List<object>();
                foreach (var key in keys)
                {
                    if (string.IsNullOrEmpty(key) || key == "0") continue;
                    try
                    {
                        var instanceMethod = getInstanceMethod.Invoke(null, new object[] { (key), (jbID) });
                        instances.Add(instanceMethod);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex);
                    }
                }
                data = instances;
            }
            return data;
        }

        protected MethodInfo GetViewModelTypeForPartialControl(Type viewModelType, string dataType, Type[] partialTypes = null)
        {
            if (partialTypes == null)
            {
                var type = Assembly.GetExecutingAssembly().GetType($"{viewModelType.Namespace}.{dataType}");
                return type.GetMethod("GetInstance");
            }
            else
            {
                foreach (var partialType in partialTypes)
                {
                    var type = Assembly.GetExecutingAssembly().GetType($"{partialType.Namespace}.{dataType}");
                    if (type == null) continue;
                    var getInstanceMethod = type.GetMethod("GetInstance");
                    if (getInstanceMethod == null) continue;
                    return getInstanceMethod;
                }
                return null;
            }
        }

        protected virtual CustomControllerBase GetPartialController(string partialControlName)
        {
            throw new NotImplementedException("Child Controller should implement this method");
        }

        protected JsonResult PrepareJsonUnauthorizedResult()
        {
            Response.StatusCode = 401;
#if NETFRAMEWORK
            var result = Json(new
            {
                Type = "Unauthorized"
            }, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;
#else
            var result = Json(new
            {
                Type = "Unauthorized"
            });
#endif
            return result;
        }

        protected string Serialize(object o, bool preventCycles = false)
        {
            var settings = preventCycles
                           ? Utilities.DefaultSerializationSettings
                           : Utilities.DefaultSerializationSettingsWithCycles;
            return Utilities.Serialize(o, settings);
        }

        /* De-serializes a ViewModel property and returns a DomainModel instance
         * calling the Convert method of the de-serialized instance, or an already
         * encountered ViewModel Instance */

        protected V DeserializeViewModelProperty<V, VDTO>(string serializedData)
        {
            var deserialized = Utilities.Deserialize<VDTO>(serializedData) as IViewModelDTO<V>;
            if (deserialized == null) return default(V);
            var typeHash = (deserialized as ViewModelDTOBase)?._typeHash;
            var originalTypeClassName = (deserialized as ViewModelDTOBase)?._originalTypeClassName;
            foreach (ViewModelDTOBase dto in ViewModelDTOBase.DTOHelper.SeenModelInstances?.Keys)
            {
                if (dto._clientKey != null && dto._clientKey.Equals(deserialized?._clientKey) &&
                        (dto._typeHash.Equals(typeHash) ||
                         dto._originalTypeClassName.ToString() == originalTypeClassName.ToString()))
                {
                    return (V) ViewModelDTOBase.DTOHelper.SeenModelInstances[dto];
                }
                ;
            }
            return deserialized.Convert();
        }

        protected List<V> DeserializeViewModelCollectionProperty<V, VDTO>(string serializedData)
        {
            var deserialized = Utilities.Deserialize<List<VDTO>>(serializedData); // as List<IViewModelDTO<V>>;
            var instances = new List<V>();
            foreach (var item in deserialized)
            {
                if (item == null)
                {
                    instances.Add(default(V));
                    continue;
                }
                var entry =  item as IViewModelDTO<V>;
                if (entry == null)
                {
                    instances.Add((V) Convert.ChangeType(item, typeof(V)));
                    continue;
                }
                var typeHash = (entry as ViewModelDTOBase)?._typeHash;
                var found = false;
                foreach (ViewModelDTOBase dto in ViewModelDTOBase.DTOHelper.SeenModelInstances?.Keys)
                {
                    if (dto._clientKey != null && dto._clientKey.Equals(entry?._clientKey) && dto._typeHash == typeHash)
                    {
                        instances.Add((V) ViewModelDTOBase.DTOHelper.SeenModelInstances[dto]);
                        found = true;
                        break;
                    }
                    ;
                }
                if (!found)
                {
                    instances.Add(entry.Convert());
                }
            }
            return instances;
        }

        protected V DeserializeViewModelProperty<V>(JValue jValue)
        {
            return (jValue == null || jValue.Value == null) ? default(V) : (V) jValue.Value;
        }

        protected DatasourceRequest DeserializeDatasourceRequest(string serializedData)
        {
            return Utilities.Deserialize<DatasourceRequest>(serializedData);
        }

        protected List<AggregatorInfo<X>> DeserializeAggregatorsRequest<X>(string serializedData)
        {
            return Utilities.Deserialize<List<AggregatorInfo<X>>>(serializedData);
        }

        protected void ClientCommand(ClientCommands command, params object[] args)
        {
            viewDTO.ClientCommands.Add(new ClientCommandInfo(command, args));
        }

        protected ActionResult GetRedirectInfo(string url, string homeController, string homeAction)
        {
            var controller = url;
            var action = "";
            if (string.IsNullOrWhiteSpace(url) || !Url.IsLocalUrl(url))
            {
                controller = homeController;
                action = homeAction;
                url = homeController + "/" + homeAction;
            }
            if (Request.IsAjaxRequest() ||
                    CLMS.Framework.Utilities.Web.CurrentServerRole == CLMS.Framework.Utilities.Web.ServerRole.Application)
            {
#if NETFRAMEWORK
                var result = Json(new
                {
                    Type = "Redirect",
                    Controller = controller.Replace("~/", ""),
                    Action = action,
                    Method = "GET",
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
#else
                var result = Json(new
                {
                    Type = "Redirect",
                    Controller = controller.Replace("~/", ""),
                    Action = action,
                    Method = "GET",
                });
#endif
                return result;
            }
            return Redirect(url);
        }

        protected ActionResult GetRedirectInfo(string formName, string actionName, RouteValueDictionary parameters)
        {
            if (Request.IsAjaxRequest() ||
                    CLMS.Framework.Utilities.Web.CurrentServerRole == CLMS.Framework.Utilities.Web.ServerRole.Application)
            {
#if NETFRAMEWORK
                var result = Json(new
                {
                    Type = "Redirect",
                    Controller = formName,
                    Action = actionName,
                    QueryParameters = parameters?.Select(x => x.Value),
                    Method = "GET",
                }, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue;
#else
                var result = Json(new
                {
                    Type = "Redirect",
                    Controller = formName,
                    Action = actionName,
                    QueryParameters = parameters?.Select(x => x.Value),
                    Method = "GET",
                });
#endif

                return result;
            }
            else
            {
                return RedirectToAction(actionName, formName, parameters);
            }
        }

        protected JsonResult SaveListView(JObject postedData, string formName)
        {
            var controlName = postedData["ControlName"]?.ToString();
            var serializedStatus = postedData["SerializedStatus"]?.ToString();
            var viewName = postedData["ViewName"]?.ToString();
            var setAsDefault = postedData["SetAsDefault"]?.ToString();
            if (string.IsNullOrWhiteSpace(controlName))
            {
                throw new ApplicationException("Request did not provide a control name.");
            }
            if (string.IsNullOrWhiteSpace(serializedStatus))
            {
                throw new ApplicationException("Request did not provide list status data.");
            }
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ApplicationException("Request did not provide a view name.");
            }
            if (string.IsNullOrWhiteSpace(formName))
            {
                throw new ApplicationException("Cannot save view with empty form name.");
            }
            var listName = formName + "_" + controlName;
            var makeDefault = setAsDefault?.ToLower() == "true";
            ProfileHelper.SaveListView(listName, viewName, serializedStatus, makeDefault);
            return Json(new {Result = "OK"});
        }

        protected JsonResult DeleteListView(JObject postedData, string formName)
        {
            var controlName = postedData["ControlName"]?.ToString();
            var viewName = postedData["ViewName"]?.ToString();
            if (string.IsNullOrWhiteSpace(controlName))
            {
                throw new ApplicationException("Request did not provide a control name.");
            }
            if (string.IsNullOrWhiteSpace(viewName))
            {
                throw new ApplicationException("Request did not provide a view name.");
            }
            if (string.IsNullOrWhiteSpace(formName))
            {
                throw new ApplicationException("Cannot delete view with empty form name.");
            }
            var listName = formName + "_" + controlName;
            ProfileHelper.DeleteListView(listName, viewName);
            return Json(new {Result = "OK"});
        }

        protected JsonResult LoadListViews(JObject postedData, string formName)
        {
            var controlName = postedData["ControlName"]?.ToString();
            if (string.IsNullOrWhiteSpace(controlName))
            {
                throw new ApplicationException("Request did not provide a control name.");
            }
            if (string.IsNullOrWhiteSpace(formName))
            {
                throw new ApplicationException("Cannot load views with empty form name.");
            }
            var listName = formName + "_" + controlName;
            var views = ProfileHelper.GetListAvailableViews(listName);
            return Json(new {Data = views});
        }

        public ActionResult _RaiseEvent()
        {
            var _data = _LoadViewModel();
            var methodName = "Raise" + _data["eventName"];
            var assemblyName = "cfTests.Hubs";
            ReflectionHelper.InvokeStaticVoidMethod(assemblyName, methodName, _data["parameters"], false, false);
            // Logging. Parameters may not be exactly 'parseable'
            // to Logger requirements, but this is handled by DebugHelper class
            var parameters = _data["parameters"]?.ToString();
            var paramsArray = Utilities.Deserialize<List<string>>(parameters);
            CLMS.Framework.Utilities.DebugHelper.Log(paramsArray, this.GetType().Name);
            return Content("OK");
        }

        private int ClientResponseIndex = 0;
        private Dictionary<string, ClientUpdateInfo> _clientData = null;

        protected void InitClientResponse()
        {
            ClientResponseIndex = 0;
            _clientData = new Dictionary<string, ClientUpdateInfo>();
        }

        protected void AddToResponse(string path, object instance, Func<object, object> toDtoMethod = null)
        {
            var entry = new ClientUpdateInfo
            {
                Instance = instance,
                DtoConverter = toDtoMethod,
                Order = ClientResponseIndex++
            };
            if (_clientData.ContainsKey(path))
            {
                _clientData[path] = entry;
            }
            else
            {
                _clientData.Add(path, entry);
            }
        }

        protected bool HasClientResponse()
        {
            return _clientData != null;
        }

        protected List<ResponseAssigment> GetClientResponse()
        {
            var resp = new List<ResponseAssigment>();
            foreach (var item in _clientData.OrderBy(i => i.Value.Order))
            {
                resp.Add(new ResponseAssigment
                {
                    Path = item.Key,
                    Value = item.Value.DtoConverter == null ? item.Value.Instance : item.Value.DtoConverter(item.Value.Instance)
                });
            }
            return resp;
        }

#region External Account Functions
        public ActionResult _ExecuteExternalProviderEvent(string actionType, string provider, string controller, string successAction, string failAction, string userClass)
        {
            var validActionTypesText = "(Valid action types: 'Login', 'Link', 'Register')";
            if (string.IsNullOrWhiteSpace(actionType))
            {
                throw new ApplicationException($"Request did not provide an Action Type (actionType). {validActionTypesText}");
            }
            if (string.IsNullOrWhiteSpace(provider))
            {
                throw new ApplicationException("Request did not provide an External Login Provider (provider). (Valid providers: 'Google', 'Facebook')");
            }
            if (string.IsNullOrWhiteSpace(controller))
            {
                throw new ApplicationException("Request did not provide an Callback Controller (controller).");
            }
#if NETFRAMEWORK
            if (string.Compare(actionType, "Link", true) == 0)
            {
                return new ChallengeResult(provider, Url.Action("_LinkExternalAccountCallback", controller, new { successAction = successAction, failAction = failAction }));
            }
            if (string.Compare(actionType, "Login", true) == 0)
            {
                return new ChallengeResult(provider, Url.Action("_LoginWithExternalAccountCallback", controller, new { successAction = successAction, failAction = failAction }));
            }
            if (string.Compare(actionType, "Register", true) == 0)
            {
                return new ChallengeResult(provider, Url.Action("_RegisterExternalAccountCallback", controller, new { successAction = successAction, failAction = failAction, userClass = userClass }));
            }
            if (string.Compare(actionType, "GetProfile", true) == 0)
            {
                return new ChallengeResult(provider, Url.Action("_GetExternalProfileCallback", controller, new { successAction = successAction, failAction = failAction }));
            }
#else
#endif
            throw new ApplicationException($"Request got an invalid Action Type: {actionType}. {validActionTypesText}");
        }

#if NETFRAMEWORK
        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
            : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                LoginProvider = provider;
                RedirectUri = redirectUri;
                UserId = userId;
            }

            public string LoginProvider
            {
                get;
                set;
            }
            public string RedirectUri
            {
                get;
                set;
            }
            public string UserId
            {
                get;
                set;
            }
            private const string XsrfKey = "XsrfId";
            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new Microsoft.Owin.Security.AuthenticationProperties {RedirectUri = RedirectUri};
                if (UserId != null)
                {
                    properties.Dictionary[XsrfKey] = UserId;
                }
                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, LoginProvider);
            }
        }
#else
#endif


#endregion
    }
}
