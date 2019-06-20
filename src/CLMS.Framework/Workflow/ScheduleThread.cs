using System;
using System.Configuration;
using System.Threading;
using log4net;
using System.Web;
using System.Net;
using NHibernate;
using CLMS.Framework.Utilities;

#if NETFRAMEWORK
using System.Net.Http;
#else
using Microsoft.AspNetCore.Http;
#endif

namespace CLMS.Framework.Workflow
{
    public class ScheduleThread
    {
        private static volatile ScheduleWorker _scheduleWorker = null;
        private static volatile Thread _scheduleThread = null;
        private static ILog _vScheduleLog = null;

        private static ILog ScheduleLog => _vScheduleLog ?? (_vScheduleLog = LogManager.GetLogger(typeof(ScheduleThread)));

        private static volatile bool _scheduleThreadAnalyticDebugOn = false;
        private static volatile int _scheduleThreadHttpRequestTimeOutMin = 0;
        private static volatile bool _scheduleThreadEnabled = true;
        private static volatile int _scheduleThreadWorkIntervalInMsec = 5000;
        private static volatile string _httpRuntimeUrl = "";

        public static ScheduleManager Manager {get; set;}

        public static long NumberOfSessions
        {
            get;
            set;
        } = 0;

        private class ScheduleWorker
        {
            private volatile bool _shouldStop = false;
            private ScheduleManager _scheduler = null;
            private DateTime _start;

            public ScheduleWorker(ScheduleManager manager)
            {
                _scheduler = manager ?? new ScheduleManager();
            }

            public void DoWork()
            {
                try
                {
                    _start = DateTime.UtcNow;
                    while (!_shouldStop)
                    {
                        Thread.Sleep(_scheduleThreadWorkIntervalInMsec);
                        if (_shouldStop) break;
                        _scheduler.ProcessSchedules();
                        if (_shouldStop) break;
                        if (_scheduleThreadHttpRequestTimeOutMin > 0 && _httpRuntimeUrl.Length > 0)
                        {
                            if ((DateTime.UtcNow - _start).Minutes >= _scheduleThreadHttpRequestTimeOutMin)
                            {
                                _start = DateTime.UtcNow;
                                SendHttpRequest(_httpRuntimeUrl);
                            }
                        }
                        if (_scheduleThreadAnalyticDebugOn)
                            ScheduleLog.Debug("_scheduleThread thread: working...");
                    }
                    if (_scheduleThreadAnalyticDebugOn)
                        ScheduleLog.Debug("_scheduleThread thread: terminating gracefully.");
                }
                catch (Exception ex)
                {
                    ScheduleLog.Error("_scheduleThread thread: terminating with expection: [" + ex.Message + "]");
                    _scheduleThread = null;
                }
            }

            private void SendHttpRequest(string url)
            {
                try
                {
                    if (String.IsNullOrEmpty(url))
                    {
                        ScheduleLog.Error("SendHttpRequest wrong url ");
                        return;
                    }
                    ScheduleLog.Debug("Start SendHttpRequest to: [" + url + "]");
                    WebRequest request = WebRequest.Create(url);
                    request.Timeout = 40000;
                    WebResponse response = null;
                    try
                    {
                        response = request.GetResponse();
                    }
                    catch (Exception ex)
                    {
                        if (_scheduleThreadAnalyticDebugOn)
                            ScheduleLog.Debug("SendHttpRequest() error: [" + ex.Message + "]");
                    }
                    finally
                    {
                        if (response != null)
                        {
                            response.Close();
                            response.Dispose();
                        }
                    }
                    ScheduleLog.Debug("Finish SendHttpRequest to: [" + url + "]");
                }
                catch (Exception ex)
                {
                    ScheduleLog.Error("SendHttpRequest exception: " + ex.Message);
                }
            }

            public void RequestStop()
            {
                _shouldStop = true;
            }

            ~ScheduleWorker()
            {
                _scheduler = null;
            }
        }

        public static void CheckScheduleThreadStatus(HttpContext httpContext)
        {
            if (_scheduleThreadAnalyticDebugOn)
                ScheduleLog.Debug("Enter CheckScheduleThreadStatus()");
            if (!_scheduleThreadEnabled)
            {
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("Exit CheckScheduleThreadStatus() !_scheduleThreadEnabled");
                return;
            }
            if (_scheduleThread == null)
            {
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("CheckScheduleThreadStatus _scheduleThread == null, wait..... to check again");
                Thread.Sleep(20000);
                if (_scheduleThread == null)
                {
                    ScheduleLog.Error("CheckScheduleThreadStatus _scheduleThread == null again try to start the thread");
                    StartScheduleThread(httpContext);
                }
            }
            else
            {
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("_scheduleThread is up and running!!!");
            }
            SetHttpRuntimeUrl(httpContext);
            if (_scheduleThreadAnalyticDebugOn)
                ScheduleLog.Debug("Exit CheckScheduleThreadStatus()");
        }

        private static void SetHttpRuntimeUrl(HttpContext httpContext)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_httpRuntimeUrl))
                {
#if NETFRAMEWORK
                    _httpRuntimeUrl = httpContext.Request.Url.GetLeftPart(UriPartial.Authority)
                                      + httpContext.Request.ApplicationPath + "/favicon.ico";
#else
                    _httpRuntimeUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}{httpContext.Request.PathBase}/favicon.ico";
#endif
                    if (_scheduleThreadAnalyticDebugOn)
                    {
                        ScheduleLog.Debug("HttpRuntimeUrl() url: [" + _httpRuntimeUrl + "]");
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private static void GetConfigParams()
        {
            try
            {
                _scheduleThreadAnalyticDebugOn =
                    Convert.ToBoolean((ConfigurationManager.AppSettings["ScheduleThreadAnalyticDebugOn"]));
            }
            catch (Exception)
            {
                _scheduleThreadAnalyticDebugOn = false;
            }
            try
            {
                _scheduleThreadHttpRequestTimeOutMin =
                    Convert.ToInt32((ConfigurationManager.AppSettings["ScheduleThreadHttpRequestTimeOutMin"]));
            }
            catch (Exception)
            {
                _scheduleThreadHttpRequestTimeOutMin = 0;
            }
            try
            {
                _scheduleThreadEnabled = Convert.ToBoolean((ConfigurationManager.AppSettings["ScheduleThreadEnabled"]));
            }
            catch (Exception)
            {
                _scheduleThreadEnabled = true;
            }
            try
            {
                _scheduleThreadWorkIntervalInMsec =
                    Convert.ToInt32((ConfigurationManager.AppSettings["ScheduleThreadWorkIntervalInMsec"]));
            }
            catch (Exception)
            {
                _scheduleThreadWorkIntervalInMsec = 5000;
            }
        }

        public static void StartScheduleThread(HttpContext httpContext)
        {
            try
            {
                GetConfigParams();
                SetHttpRuntimeUrl(httpContext);
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("Enter StartScheduleThread() with User: [" + httpContext?.User +
                                      "], Enabled: [" + _scheduleThreadEnabled + "], " +
                                      "WorkIntervalInMsec: [" + _scheduleThreadWorkIntervalInMsec + "], " +
                                      "HttpRequestTimeOutMin: [" + _scheduleThreadHttpRequestTimeOutMin + "]"
                                     );
                //_log.DebugFormat("Found {0} Active Schedules.", schedules.Count);
                if (!_scheduleThreadEnabled)
                {
                    if (_scheduleThreadAnalyticDebugOn)
                        ScheduleLog.Debug("Exit StartScheduleThread() !_scheduleThreadEnabled");
                    return;
                }
                HttpContext lHttpContext = httpContext ?? Web.GetContext();
                if (_scheduleThread != null)
                {
                    if (_scheduleThreadAnalyticDebugOn)
                        ScheduleLog.Debug("_scheduleThread is already running");
                    return;
                }
                _scheduleWorker = new ScheduleWorker(Manager);
                _scheduleThread = new Thread(new ThreadStart(() =>
                {
#if NETFRAMEWORK
                    HttpContext.Current = lHttpContext;
#endif
                    _scheduleWorker.DoWork();
                }));
                _scheduleThread.Start();
                while (!_scheduleThread.IsAlive)
                {
                    if (_scheduleThreadAnalyticDebugOn)
                        ScheduleLog.Debug("StartScheduleThread !_scheduleThread.IsAlive, wait.....");
                    Thread.Sleep(100);
                }
                if (_scheduleThreadAnalyticDebugOn)
                {
                    ScheduleLog.Debug("Exit StartScheduleThread()");
                }
            }
            catch (Exception ex)
            {
                ScheduleLog.Error("Exit StartScheduleThread()() with Exception error: " + ex.Message);
            }
        }

        public static void StopScheduleThread()
        {
            try
            {
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("Enter StopScheduleThread()");
                if (_scheduleThread != null)
                {
                    if (_scheduleThreadAnalyticDebugOn)
                        ScheduleLog.Debug("Try to stop the _scheduleThread");
                    _scheduleWorker.RequestStop();
                    _scheduleThread.Join(20000);
                    _scheduleWorker = null;
                    _scheduleThread = null;
                }
                else
                {
                    if (_scheduleThreadAnalyticDebugOn)
                    {
                        ScheduleLog.Debug("_scheduleThread is not running");
                    }
                }
                if (_scheduleThreadAnalyticDebugOn)
                    ScheduleLog.Debug("Exit StopScheduleThread()");
            }
            catch (Exception ex)
            {
                ScheduleLog.Error("Exit StopScheduleThread() with Exception error: " + ex.Message);
            }
        }
    }
}