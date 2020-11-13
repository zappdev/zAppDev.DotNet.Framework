// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
#if NETFRAMEWORK
#else
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

using Ionic.Zip;
using Microsoft.AspNetCore.Mvc.Formatters;


namespace zAppDev.DotNet.Framework.Mvc
{
    public class AjaxActionInputFormatter : InputFormatter
    {
        public AjaxActionInputFormatter()
        {
            SupportedMediaTypes.Add("application/zappdev");
        }

        protected override bool CanReadType(Type type)
        {
            if (type.IsGenericType && type.Name.StartsWith("AjaxRequest"))
            {
                return base.CanReadType(type);
            }
            return false;
        }

        public override async Task<InputFormatterResult> ReadAsync(InputFormatterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (context.ModelType.IsGenericType == false)
            {
                return await InputFormatterResult.FailureAsync();
            }
            var postedData = ParsePostedData(context);
            var type = context.ModelType;
            var viewModelType = type.GenericTypeArguments.First();
            var obj = type.GetConstructor(new Type[] { }).Invoke(new object[] { });
            var rawPropertyInfo = type.GetProperty("Raw");
            rawPropertyInfo.SetValue(obj, postedData);
            if (postedData["_isDirty"] != null)
            {
                var isDirty = (bool)(((JValue)postedData["_isDirty"]).Value);
                var propertyInfo = type.GetProperty("IsDirty");
                propertyInfo.SetValue(obj, isDirty);
            }
            object _vm;
            if (postedData["model"] != null)
            {
                var serializedModel = postedData["model"].ToString();
                _vm = Utilities.Deserialize(serializedModel, viewModelType);
            }
            else
            {
                _vm = viewModelType
                      .GetConstructor(new Type[] { })
                      .Invoke(new object[] { });
            }
            var modelPropertyInfo = type.GetProperty("Model");
            modelPropertyInfo.SetValue(obj, _vm);
            return await InputFormatterResult.SuccessAsync(obj);
        }

        public override async Task<InputFormatterResult> ReadRequestBodyAsync(InputFormatterContext context)
        {
            return await ReadAsync(context);
        }

        protected JObject ParsePostedData(InputFormatterContext context)
        {
            var request = context.HttpContext.Request;
            var json = "";
            if (request.Headers["IsZipped"] == "true")
            {
                json = Unzip(request.Body);
            }
            else
            {
                using (var reader = new StreamReader(request.Body))
                {
                    json = reader.ReadToEndAsync().Result;
                }
            }
            return JObject.Parse(json);
        }

        protected string Unzip(Stream stream)
        {
            using (var zipFile = ZipFile.Read(stream))
            {
                using (MemoryStream output = new MemoryStream())
                {
                    zipFile["form.data"].Extract(output);
                    return Encoding.UTF8.GetString(output.ToArray());
                }
            }
        }
    }
}

#endif