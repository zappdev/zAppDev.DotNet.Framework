using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Web;
using System.Reflection;
using System.Configuration;
using System.Net.Http.Formatting;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using RSG;
using AppDevCache = CLMS.AppDev.Cache;

namespace CLMS.Framework.Utilities
{
    public class ValidationException : Exception
    {
        public ValidationException(string msg)
            : base(msg)
        {

        }
    }

    public class MambaRuntimeType
    {
        public MambaRuntimeType(MemberInfo info)
        {
            Info = info;
        }

        public MemberInfo Info { get; set; }

        public string Name => Info.Name;

        public object GetValue(object instance, object[] index = null)
        {
            if (index == null) index = new object[] { };

            return Info is PropertyInfo
                        ? (Info as PropertyInfo).GetValue(instance, index)
                        : (Info as FieldInfo).GetValue(instance);
        }

        public void SetValue(object instance, object value, object[] index = null)
        {
            if (Info is PropertyInfo)
            {
                (Info as PropertyInfo).SetValue(instance, value, index);
            }
            else
            {
                (Info as FieldInfo).SetValue(instance, value);
            }
        }

        public Type PropertyType => Info is PropertyInfo
            ? (Info as PropertyInfo).PropertyType
            : (Info as FieldInfo).FieldType;

        public static List<MambaRuntimeType> FromPropertiesList(PropertyInfo[] props)
        {
            var list = new List<MambaRuntimeType>();

            foreach (var p in props)
            {
                list.Add(new MambaRuntimeType(p));
            }

            return list;
        }
    }

    public class Common
    {
        public static string GetExcelFormat(List<string> columnNames, List<List<object>> values, string[] formatStrings = null,
           string sheetName = "Sheet1", bool showBorders = true, string headerBackgroundColor = "rgb(238, 233, 233)",
           string rowBackgroundColor = "rgb(238, 238, 224)", string alternateRowBackgroundColor = "rgb(255, 255, 255)")
        {
            List<object[]> convertedValues = new List<object[]>();
            foreach (var val in values)
            {
                convertedValues.Add(val.ToArray());
            }
            return GetExcelFormat(columnNames.ToArray(), convertedValues.ToArray());
        }

        public static string GetExcelFormat(string[] columnNames, object[][] values, string[] aggregators = null, string[] formatStrings = null,
            string sheetName = "Sheet1", bool showBorders = true, string headerBackgroundColor = "rgb(238, 233, 233)",
            string rowBackgroundColor = "rgb(238, 238, 224)", string alternateRowBackgroundColor = "rgb(255, 255, 255)",
            string aggregatorBackgroundColor = "rgb(255, 255, 255)", string decimalSeparator = null)
        {
            if (columnNames == null) throw new NullReferenceException("The provided column names array is null");
            if (values == null) throw new NullReferenceException("The provided values names array is null");

            var clonedCultureInfo = System.Globalization.CultureInfo.InvariantCulture.Clone() as System.Globalization.CultureInfo;
            if (!string.IsNullOrEmpty(decimalSeparator)) clonedCultureInfo.NumberFormat.NumberDecimalSeparator = decimalSeparator;

            var numericRegex = new System.Text.RegularExpressions.Regex("^-?\\d+(\\.|,)?\\d*$", System.Text.RegularExpressions.RegexOptions.Compiled);

            var headerContent = columnNames.Aggregate("", (current, column) =>
            {
                var columnName = column;
                if (numericRegex.IsMatch(columnName))
                {
                    columnName = $"=\"{columnName}\"";
                }
                return current + $@"<th style='background-color: {headerBackgroundColor}'>{columnName}</th>";
            });

            //This is for count, sum and average calculations
            var columnSum = new decimal?[columnNames.Length];
            for (var i = 0; i < columnNames.Length; i++)
            {
                columnSum[i] = null;
            }

            var rowContent = new StringBuilder();

            for (var rowIndex = 0; rowIndex < values.Length; rowIndex++)
            {
                rowContent.AppendLine("<tr>");
                for (var columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
                {
                    var currentColor = rowIndex % 2 == 0 ? rowBackgroundColor : alternateRowBackgroundColor;
                    var cellStringFormat = formatStrings != null && formatStrings.Length > columnIndex && !string.IsNullOrEmpty(formatStrings[columnIndex])
                                            ? formatStrings[columnIndex]
                                            : null;
                    var additionalStyle = values[rowIndex].Length > columnIndex &&
                                          (values[rowIndex][columnIndex] is DateTime || values[rowIndex][columnIndex] is DateTime?)
                                          ? $@"vnd.ms-excel.numberformat:{ cellStringFormat ?? "dd/MM/yyyy" };"
                                          : "";
                    cellStringFormat = cellStringFormat == null ? "{0}" : $"{{0:{cellStringFormat}}}";

                    if (columnIndex >= values[rowIndex].Length)
                    {
                        rowContent.AppendLine($@"<td style='background-color: {currentColor};{additionalStyle}'></td>");
                    }
                    else
                    {
                        var cellValue = values[rowIndex][columnIndex];

                        if (columnIndex < aggregators?.Length && !string.IsNullOrEmpty(aggregators[columnIndex]) &&
                            numericRegex.IsMatch(cellValue.ToString()))
                        {
                            if (columnSum[columnIndex] == null) columnSum[columnIndex] = decimal.Parse(cellValue.ToString());
                            else columnSum[columnIndex] += decimal.Parse(cellValue.ToString());
                        }

                        //This is to ensure that a string object which contains a numeric value is handled as a string
                        if (cellValue is string && numericRegex.IsMatch(cellValue.ToString()))
                        {
                            cellValue = $"=\"{cellValue}\"";
                        }

                        rowContent.AppendLine($@"<td style='background-color: {currentColor};{additionalStyle}'>{ string.Format(clonedCultureInfo, cellStringFormat, cellValue) }</td>");
                    }
                }

                rowContent.AppendLine("</tr>");
            }

            var countAggregation = string.Empty;
            var sumAggregation = string.Empty;
            var averageAggregation = string.Empty;

            if (aggregators != null && aggregators.Any(a => a.Contains("COUNT")))
            {
                countAggregation = $@"<tr>{columnSum.Aggregate("", (current, column) =>
                {
                    var countValue = column != null ? values.Length.ToString() : "";
                    return current + $@"<td style='background-color: {aggregatorBackgroundColor}'>{countValue}</td>";
                })}<td style='background-color: {aggregatorBackgroundColor}'>COUNT</td></tr>";
            }
            if (aggregators != null && aggregators.Any(a => a.Contains("SUM")))
            {
                sumAggregation = $@"<tr>{columnSum.Aggregate("", (current, column) =>
                {
                    var sumValue = column?.ToString(clonedCultureInfo) ?? "";
                    return current + $@"<td style='background-color: {aggregatorBackgroundColor}'>{sumValue}</td>";
                })}<td style='background-color: {aggregatorBackgroundColor}'>SUM</td></tr>";
            }
            if (aggregators != null && aggregators.Any(a => a.Contains("AVERAGE")))
            {
                averageAggregation = $@"<tr>{columnSum.Aggregate("", (current, column) =>
                {
                    var countValue = column != null ? ((decimal)(column / values.Length)).ToString(clonedCultureInfo) : "";
                    return current + $@"<td style='background-color: {aggregatorBackgroundColor}'>{countValue}</td>";
                })}<td style='background-color: {aggregatorBackgroundColor}'>AVG</td></tr>";
            }

            return $@"
<html xmlns:x=""urn:schemas-microsoft-com:office:excel""> 
<head> 
    <meta http-equiv=Content-Type content=""text/html; charset=utf-8""> 
    <!--[if gte mso 9]> 
    <xml> 
        <x:ExcelWorkbook> 
        <x:ExcelWorksheets> 
        <x:ExcelWorksheet> 
        <x:Name>{sheetName}</x:Name>
        <x:WorksheetOptions> 
        <x:Panes> 
        </x:Panes> 
        </x:WorksheetOptions> 
        </x:ExcelWorksheet> 
        </x:ExcelWorksheets> 
        </x:ExcelWorkbook> 
    </xml> 
    <![endif]--> 
</head> 
<body> 
<table {(showBorders ? "border = '1'" : "")}> 
    <thead> 
        <tr> 
             {headerContent}
        </tr> 
    </thead> 
    <tbody> 
        {rowContent}
    </tbody> 
    <tfoot> 
        {countAggregation}
        {sumAggregation}
        {averageAggregation}
    </tfoot> 
</table> 
</body> 
</html>";
        }

        public class MambaError
        {
            public MambaError()
            {
                Message = "";
                StackTrace = "";
                Exception = null;
            }

            public MambaError(Exception ex)
            {
                Message = ex.Message;
                StackTrace = ex.StackTrace;
                Exception = ex;
            }

            public string GetFriendlyMessage()
            {
                var friendlyMessageHandler = new CLMS.Framework.Utilities.ExceptionHandler();
                return friendlyMessageHandler.GetFriendlyMessageEntriesHTML(this.Exception);
            }

            public string Message;
            public string StackTrace;
            public Exception Exception;
        }

        //public static DateTime? ParseExactDate(string date, string format, Language lang)
        //{            
        //    try {                
        //        return DateTime.ParseExact(date, format, lang.DateTimeFormat);
        //    }
        //    catch
        //    {               
        //        return null;
        //    }            
        //}

        public static T GetItemFromList<T>(IList<T> items, int? index)
        {
            if (items == null) return default(T);
            if (index == null)
            {
                throw new Exception("Index value is null");
            }

            return items[index.GetValueOrDefault(0)];
        }

        public static void SetItemFromList<T>(IList<T> items, int? index, T newValue)
        {
            if (items == null)
            {
                throw new Exception("List is null");
            }
            if (index == null)
            {
                throw new Exception("Index value is null");
            }

            items[index.GetValueOrDefault(0)] = newValue;
        }

        public static T GetItemFromArray<T>(T[] items, int? index)
        {
            if (items == null) return default(T);
            if (index == null)
            {
                throw new Exception("Index value is null");
            }

            return items[index.GetValueOrDefault(0)];
        }

        public static void SetItemFromArray<T>(T[] items, int? index, T newValue)
        {
            if (items == null)
            {
                throw new Exception("Array is null");
            }
            if (index == null)
            {
                throw new Exception("Index value is null");
            }

            items[index.GetValueOrDefault(0)] = newValue;
        }

        public static System.Globalization.CultureInfo GetCultureInfo(int? id)
        {
            if (id == null || id.HasValue == false) return System.Threading.Thread.CurrentThread.CurrentCulture;

            return new System.Globalization.CultureInfo(id.Value);
        }

        public static long? ToUnixTime(DateTime? datetime)
        {
            return
                System.Convert.ToInt64((datetime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc))?.TotalSeconds);
        }

        public static DateTime? ParseExactDate(string date, string format, System.Globalization.CultureInfo culture)
        {
            try
            {
                var flags = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal;
                return DateTime.ParseExact(date, format, culture, flags).ToUniversalTime();
            }
            catch
            {
                return null;
            }
        }

        public static bool DateHasValue(DateTime t)
        {
            return t != default(DateTime);
        }

        public static bool DateHasValue(DateTime? t)
        {
            return t.HasValue;
        }

        public static string RegisterUser(string username, string password, string email = "", bool isApproved = true)
        {
            //var status = System.Web.Security.MembershipCreateStatus.Success;
            //using (var sc = new System.Transactions.TransactionScope(System.Transactions.TransactionScopeOption.Suppress))
            //{
            //    var user = System.Web.Security.Membership.CreateUser(username, password, email, null, null, isApproved, out status); sc.Complete();
            //};

            //if (status != System.Web.Security.MembershipCreateStatus.Success) return status.ToString();

            return null;
        }

        public static Random Random = new Random();

        public static T SafeCast<T>(object obj)
        {
            try
            {
                return (T)obj;
            }
            catch (Exception x)
            {
                log4net.LogManager.GetLogger(typeof(Common)).Debug($"Could not directly cast: {obj.GetType()} to {typeof(T)}", x);
                try
                {
                    var typeT = typeof(T);
                    var nullable = typeT.IsNullable();
                    if (nullable)
                    {
                        typeT = typeT.NullableOf();
                    }

                    object result = null;
                    if (typeT == typeof(double))
                    {
                        result = Convert.ToDouble(obj);
                        if (nullable)
                        {
                            result = (double?)result;
                        }
                    }
                    else if (typeT == typeof(bool))
                    {
                        result = Convert.ToBoolean(obj);
                        if (nullable)
                        {
                            result = (bool?)result;
                        }
                    }
                    else if (typeT == typeof(DateTime))
                    {
                        result = Convert.ToDateTime(obj);
                        if (nullable)
                        {
                            result = (DateTime?)result;
                        }
                    }
                    else if (typeT == typeof(decimal))
                    {
                        result = Convert.ToDecimal(obj);
                        if (nullable)
                        {
                            result = (decimal?)result;
                        }
                    }
                    else if (typeT == typeof(short))
                    {
                        result = Convert.ToInt16(obj);
                        if (nullable)
                        {
                            result = (short?)result;
                        }
                    }
                    else if (typeT == typeof(int))
                    {
                        result = Convert.ToInt32(obj);
                        if (nullable)
                        {
                            result = (int?)result;
                        }
                    }
                    else if (typeT == typeof(long))
                    {
                        result = Convert.ToInt64(obj);
                        if (nullable)
                        {
                            result = (long?)result;
                        }
                    }
                    else if (typeT == typeof(float))
                    {
                        result = Convert.ToSingle(obj);
                        if (nullable)
                        {
                            result = (float?)result;
                        }
                    }
                    else if (obj is JObject)
                    {
                        var settings = new JsonSerializerSettings
                        {
                            ContractResolver = new JsonContractResolver(new XmlMediaTypeFormatter())
                        };

                        result = JsonConvert.DeserializeObject<T>(obj?.ToString(), settings);
                    }
                    else
                    {
                        var parseResult = default(T);
                        if (!TryParseJson(obj?.ToString(), out parseResult))
                        {
                            if (!TryParseXml(obj?.ToString(), out parseResult))
                            {
                                parseResult = default(T);
                            }
                        }
                        return parseResult;
                    }

                    return (T)result;
                }
                catch (Exception convertException)
                {
                    log4net.LogManager.GetLogger(typeof(Common)).Debug($"Could not Convert : {obj.GetType()} to {typeof(T)}", convertException);
                    return default(T);
                }
            }
        }

        public static bool TryParseXml<T>(string strInput, out T obj)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("<") && strInput.EndsWith(">")))
            {
                try
                {
                    using (var stringReader = new StringReader(strInput))
                    {
                        var serializer = new XmlSerializer(typeof(T));
                        obj = (T)serializer.Deserialize(stringReader);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    log4net.LogManager.GetLogger(typeof(Common)).Debug($"Could not Parse : {strInput} to {typeof(T)}", ex);
                    obj = default(T);
                    return false;
                }
            }
            else
            {
                obj = default(T);
                return false;
            }
        }

        public static bool TryParseJson<T>(string strInput, out T obj)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    obj = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch (Exception ex) //some other exception
                {
                    log4net.LogManager.GetLogger(typeof(Common)).Debug($"Could not Parse : {strInput} to {typeof(T)}", ex);
                    obj = default(T);
                    return false;
                }
            }
            else
            {
                obj = default(T);
                return false;
            }
        }

        public static string DownloadAndExtractZip(string url, string path = null)
        {
            path = path ?? System.Web.HttpContext.MapPath(Path.Combine("~/App_Data/temp", Guid.NewGuid().ToString()));
            if (((System.IO.Directory.Exists(path)) == false))
            {
                System.IO.Directory.CreateDirectory(path);
            }
            var zipPath = System.IO.Path.Combine(path, "downloaded.zip");
            //(new System.Net.WebClient()).DownloadFile(url, zipPath);
            using (var wc = new System.Net.WebClient())
            {
                wc.DownloadFile(url, zipPath);
            }
            var extractPath = System.IO.Path.Combine(path, "extracted_data");

            System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractPath);

            return extractPath;
        }

        public static void MoveFile(string src, string dest, bool overwrite = false)
        {
            var exists = File.Exists(dest);

            if (exists & !overwrite) throw new Exception("Destination file already exists.");

            if (exists & overwrite) File.Delete(dest);

            File.Move(src, dest);
        }

        public static void WriteAllTo(string path, string text)
        {
            System.IO.File.WriteAllText(path, text);
        }

        public static void WriteAllTo(string path, string text, int codepage)
        {
            System.IO.File.WriteAllText(path, text, Encoding.GetEncoding(codepage));
        }

        public static void WriteAllTo(string path, string text, bool withBOM)
        {
            using (var sw = new StreamWriter(File.Open(path, FileMode.Create), new UTF8Encoding(withBOM)))
            {
                sw.WriteLine(text);
            }
        }

        public static void WriteAllTo(string path, List<byte> data)
        {
            File.WriteAllBytes(path, data.ToArray());
        }

        public static void WriteAllTo(string path, byte[] data)
        {
            File.WriteAllBytes(path, data);
        }

        public static void AppendAllTo(string path, string text)
        {
            System.IO.File.AppendAllText(path, text);
        }
        public static void AppendAllTo(string path, string text, int codepage)
        {
            System.IO.File.AppendAllText(path, text, Encoding.GetEncoding(codepage));
        }
        public static void AppendAllTo(string path, string text, bool withBOM)
        {
            using (var sw = new StreamWriter(File.Open(path, FileMode.Append), new UTF8Encoding(withBOM)))
            {
                sw.WriteLine(text);
            }
        }
        public static void AppendAllTo(string path, List<byte> data)
        {
            AppendAllTo(path, data.ToArray());
        }
        public static void AppendAllTo(string path, byte[] data)
        {
            using (var stream = new FileStream(path, FileMode.Append))
            {
                stream.Write(data, 0, data.Length);
            }
        }
        public static void SetLastError(Exception ex)
        {
            var key = $"{HttpContext.Current.Session.Id}LastError";
            AppDevCache.CacheManager.Current.Set(key, new MambaError(ex));
        }

        public static MambaError GetLastError()
        {
            var key = $"{HttpContext.Current.Session.Id}LastError";
            return AppDevCache.CacheManager.Current.Get(key, new MambaError());
        }

        public static string GetConfigurationKey(string key)
        {
            return ConfigurationManager.AppSettings.AllKeys.Contains(key)
                    ? ConfigurationManager.AppSettings[key]
                    : null;
        }

        public static bool IsTypePrimitiveOrSimple(Type type)
        {
            type = GetTypeAsNonNullable(type);

            var simpleTypes = new Type[]
            {
                typeof(String),
                typeof(Decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(Guid)
            };

            return
                type.IsValueType ||
                type.IsPrimitive ||
                simpleTypes.Contains(type);
        }

        public static bool IsTypeCollection(Type type)
        {
            return type.GetInterfaces()
                        .Any(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));
        }

        public static bool IsPropertyPrimitiveOrSimple(MambaRuntimeType prop)
        {
            return IsTypePrimitiveOrSimple(prop.PropertyType);
        }

        public static bool IsPropertyPrimitiveOrSimple(PropertyInfo prop)
        {
            return IsTypePrimitiveOrSimple(prop.PropertyType);
        }

        public static bool IsPropertyCollection(MambaRuntimeType prop)
        {
            return IsTypeCollection(prop.PropertyType);
        }

        public static bool IsPropertyCollection(PropertyInfo prop)
        {
            return IsTypeCollection(prop.PropertyType);
        }

        #region Type Conversions

        public static T ConvertToEnum<T>(string str, bool throwException = true) where T : struct, IConvertible
        {
            try
            {
                return (T)(Enum.Parse(typeof(T), str));
            }
            catch
            {
                if (!throwException) return default(T);

                throw new FormatException();
            }
        }

        public static decimal ConvertToDecimal(string str, bool throwException = true)
        {
            try
            {
                return decimal.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (!throwException) return default(decimal);

                throw new FormatException();
            }
        }

        public static float ConvertToFloat(string str, bool throwException = true)
        {
            try
            {
                return float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (!throwException) return default(float);

                throw new FormatException();
            }
        }

        public static string ConvertMomentFormat(string format)
        {
            var newFormat = format.Replace("a", "tt");

            newFormat = newFormat.Replace("D", "d");
            newFormat = newFormat.Replace("Y", "y");

            return newFormat;
        }

        public static DateTime ConvertToDateTime(string str, bool throwException = true)
        {
            try
            {
                var flags = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal;
                return DateTime.Parse(str, System.Globalization.CultureInfo.InvariantCulture, flags).ToUniversalTime();
            }
            catch
            {
                if (!throwException) return default(DateTime);

                throw new FormatException();
            }
        }

        public static int ConvertToInt(string str, bool throwException = true)
        {
            try
            {
                return int.Parse(str);
            }
            catch
            {
                if (!throwException) return default(int);

                throw new FormatException();
            }
        }

        public static long ConvertToLong(string str, bool throwException = true)
        {
            try
            {
                return long.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (!throwException) return default(long);

                throw new FormatException();
            }
        }

        public static bool ConvertToBool(string str, bool throwException = true)
        {
            try
            {
                return bool.Parse(str);
            }
            catch
            {
                if (!throwException) return default(bool);

                throw new FormatException();
            }
        }

        public static double ConvertToDouble(string str, bool throwException = true)
        {
            try
            {
                return double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                if (!throwException) return default(double);

                throw new FormatException();
            }
        }

        public static byte ConvertToByte(string str, bool throwException = true)
        {
            try
            {
                return byte.Parse(str);
            }
            catch
            {
                if (!throwException) return default(byte);

                throw new FormatException();
            }
        }

        public static Guid ConvertToGuid(string str, bool throwException = true)
        {
            try
            {
                return Guid.Parse(str);
            }
            catch
            {
                if (!throwException) return Guid.Empty;

                throw new FormatException();
            }
        }

        #endregion

        #region Type Conversions (Nullable)        

        public static decimal? ConvertToNullableDecimal(string str)
        {
            try
            {
                return decimal.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        public static float? ConvertToNullableFloat(string str)
        {
            try
            {
                return float.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        public static DateTime? ConvertToNullableDateTime(string str)
        {
            try
            {
                var flags = System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal;
                return DateTime.Parse(str, System.Globalization.CultureInfo.InvariantCulture, flags).ToUniversalTime();
            }
            catch
            {
                return null;
            }
        }

        public static int? ConvertToNullableInt(string str)
        {
            try
            {
                return int.Parse(str);
            }
            catch
            {
                return null;
            }
        }

        public static long? ConvertToNullableLong(string str)
        {
            try
            {
                return long.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        public static bool? ConvertToNullableBool(string str)
        {
            try
            {
                return bool.Parse(str);
            }
            catch
            {
                return null;
            }
        }

        public static double? ConvertToNullableDouble(string str)
        {
            try
            {
                return double.Parse(str, System.Globalization.CultureInfo.InvariantCulture);
            }
            catch
            {
                return null;
            }
        }

        public static byte? ConvertToNullableByte(string str)
        {
            try
            {
                return byte.Parse(str);
            }
            catch
            {
                return null;
            }
        }

        public static Guid? ConvertToNullableGuid(string str)
        {
            try
            {
                return Guid.Parse(str);
            }
            catch
            {
                return null;
            }
        }

        #endregion

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Encode(byte[] inputBytes)
        {
            return System.Convert.ToBase64String(inputBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static System.Collections.Generic.List<byte> Base64DecodeAsByteArray(string base64EncodedData)
        {
            return System.Convert.FromBase64String(base64EncodedData).ToList();
        }

        public static string GetMD5Hash(string str)
        {
            using (var md5Hash = System.Security.Cryptography.MD5.Create())
            {
                var sBuilder = new System.Text.StringBuilder();

                var encodedData = System.Text.Encoding.UTF8.GetBytes(str);
                var hashedData = md5Hash.ComputeHash(encodedData);
                foreach (var b in hashedData)
                {
                    sBuilder.Append(b.ToString("x2"));
                }
                return sBuilder.ToString();
            }
        }

        public static Type GetTypeAsNonNullable(Type type)
        {
            return Nullable.GetUnderlyingType(type) == null
                ? type
                : Nullable.GetUnderlyingType(type);
        }

        public static string GetTypeName(Type type, bool full = false)
        {
            type = GetTypeAsNonNullable(type);

            return full
                ? type.FullName
                : type.Name;
        }

        public static MambaRuntimeType GetProperty(Type type, string name, bool ignoreGetter = false)
        {
            if (ignoreGetter)
            {
                var result = type.GetField(name);

                // try again using Coding Facility naming convention....
                if (result == null)
                {
                    result = type.GetField("_" + name[0].ToString().ToLowerInvariant() + name.Substring(1));
                }

                return result == null
                    ? null
                    : new MambaRuntimeType(result);
            }
            else
            {
                var result = type.GetProperty(name);

                return result == null
                    ? null
                    : new MambaRuntimeType(result);
            }
        }

        public static List<string> ReadLinesFrom(string path, int startAtLine, int numberOfLinesToRead, int codepage)
        {
            var lineNumber = 0;
            var linesRead = new List<string>();

            using (var reader = new StreamReader(path, Encoding.GetEncoding(codepage)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;

                    if (lineNumber < startAtLine)
                        continue;

                    linesRead.Add(line);

                    if (lineNumber >= numberOfLinesToRead)
                        break;
                }
            }

            return linesRead;
        }

        public static List<string> ExtractLinesFrom(string path, int numberOfLinesToRead, int codepage)
        {
            var tempFile = Path.GetTempFileName();
            var lineNumber = 0;
            long numOfRemainingLines = 0;

            var linesRead = new List<string>();

            using (var reader = new StreamReader(path, Encoding.GetEncoding(codepage)))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lineNumber++;
                    linesRead.Add(line);

                    if (lineNumber >= numberOfLinesToRead)
                        break;
                }

                using (var writer = new StreamWriter(File.Open(tempFile, FileMode.Create), Encoding.GetEncoding(codepage)))
                {
                    while ((line = reader.ReadLine()) != null)
                    {
                        numOfRemainingLines++;
                        writer.WriteLine(line);
                    }
                }
            }

            File.Delete(path);

            if (numOfRemainingLines > 0)
            {
                File.Move(tempFile, path);
                log4net.LogManager.GetLogger(typeof(Common)).Info($"File {Path.GetFileName(path)} has {numOfRemainingLines} lines left.");
            }
            else
            {
                log4net.LogManager.GetLogger(typeof(Common)).Info($"File {Path.GetFileName(path)} has no lines left. It was deleted.");
                File.Delete(tempFile);
            }

            return linesRead;
        }

        public static IPromise<T> WrapToPromise<T>(Func<T> cb)
        {
            var promise = new Promise<T>();

            try
            {
                promise.Resolve(cb.Invoke());
            }
            catch (Exception e)
            {
                promise.Reject(e);
                throw;
            }

            return promise;
        }

        public static string RunExecutable(string filename, string arguments = null)
        {
            System.Diagnostics.ProcessStartInfo information;
            information = new System.Diagnostics.ProcessStartInfo();
            information.FileName = filename;
            information.Arguments = arguments;
            information.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            information.CreateNoWindow = true;
            information.RedirectStandardOutput = true;
            information.UseShellExecute = false;

            using (var proc = System.Diagnostics.Process.Start(information))
            {
                proc.WaitForExit();
                return proc.StandardOutput.ReadToEnd();
            }
        }

        public static DateTime GetNextExecutionTime(string cronExpression, DateTime? dt)
        {
            return GetNextExecutionTime(cronExpression, dt ?? DateTime.MinValue);
        }

        public static DateTime GetNextExecutionTime(string cronExpression, DateTime dt)
        {
            var cronSchedule = NCrontab.CrontabSchedule.Parse(cronExpression);
            return cronSchedule.GetNextOccurrence(dt);
        }
    }
}
