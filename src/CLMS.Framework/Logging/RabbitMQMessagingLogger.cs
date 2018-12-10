using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
#if NETFRAMEWORK
using System.Web.Http.Controllers;
#endif
using CLMS.Framework.Services;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace CLMS.Framework.Logging
{
    public class RabbitMQMessagingLogger : IAPILogger, IDisposable
    {
        private IConnection _connection;
        private IModel _channel;
#if NETFRAMEWORK
        private readonly string _exchange;
        private bool _initialized;
        private ConnectionFactory _connectionFactory;

        public RabbitMQMessagingLogger(string host, int port, string vhost, string username, string password, string exchange = "")
        {
            _exchange = exchange;
            _connectionFactory = new ConnectionFactory
            {
                HostName = host,
                Port = port,
                VirtualHost = vhost,
                UserName = username,
                Password = password,
                AutomaticRecoveryEnabled = true
            };
            Initialize();
        }

        private void Initialize(bool throwOnError = false)
        {
            if(_initialized) return;
            if (string.IsNullOrWhiteSpace(_connectionFactory.HostName))
            {
                LogManager.GetLogger(typeof(RabbitMQMessagingLogger)).Debug("No host specified for RabbitMQ. The RabbitMQ Messaging Publisher cannot initialize and will not push messages.");
                if(throwOnError)
                    throw new ApplicationException("Could not initialize the RabbitMQ Messaging Publisher! Check your configuration. Make sure you have provided RabbitMQ connection details.");

                return;
            }

            if (string.IsNullOrWhiteSpace(_exchange))
            {
                LogManager.GetLogger(typeof(RabbitMQMessagingLogger)).Debug("No exchange specified for RabbitMQ. The RabbitMQ Messaging Publisher cannot initialize and will not push messages.");
                if (throwOnError)
                    throw new ApplicationException("Could not initialize the RabbitMQ Messaging Publisher! Check your configuration. Make sure you have provided RabbitMQ connection details.");

                return;
            }

            try
            {
                _connection = _connectionFactory.CreateConnection();
                _channel = _connection.CreateModel();

                if (!string.IsNullOrWhiteSpace(_exchange))
                {
                    _channel.ExchangeDeclare(_exchange, 
                        ExchangeType.Topic, 
                        durable:true);
                
                    _channel.QueueDeclare(
                        queue: "api.log.exposed",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _channel.QueueBind("api.log.exposed", _exchange, "api.log.exposed.#");
                
                    _channel.QueueDeclare(
                        queue: "api.log.external",
                        durable: true,
                        exclusive: false,
                        autoDelete: false,
                        arguments: null);

                    _channel.QueueBind("api.log.external", _exchange, "api.log.external.#");
                }

                _initialized = true;
            }
            catch(Exception e)
            {
                LogManager.GetLogger(typeof(RabbitMQMessagingLogger)).Error("Error initializing RabbitMQ Messaging Publisher!", e);
                if(throwOnError) throw;
            }
        }

        public static IAPILogger FromConfiguration()
        {
            var host = ConfigurationManager.AppSettings["rabbitmq:host"];
            var portRaw = ConfigurationManager.AppSettings["rabbitmq:port"];
            int port;
            if (!int.TryParse(portRaw, out port))
            {
                port = 5672;
            }
            var vhost = ConfigurationManager.AppSettings["rabbitmq:vhost"];
            if (string.IsNullOrWhiteSpace(vhost))
            {
                vhost = "/";
            }
            var username = ConfigurationManager.AppSettings["rabbitmq:username"];
            var password = ConfigurationManager.AppSettings["rabbitmq:password"];
            var exchange = ConfigurationManager.AppSettings["rabbitmq:exchange"];
            return new RabbitMQMessagingLogger(host, port, vhost, username, password, exchange);
        }

        public void Log(string apiType, string apiTitle, LogMessage message, bool throwOnError)
        {
            new Task(() =>
                {
                    var logger = LogManager.GetLogger(GetType());
                    try
                    {
                        // push to Log
                        logger.Debug(message.ToFullString());
                        // push to RabbitMQ
                        var topic = $@"api.log.{apiType}.{apiTitle}";

                        Publish(topic, message);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"Error while trying to log API {apiTitle}", e);
                        if (throwOnError) throw;
                    }
                })
                .Start();
        }

        public void LogExternalAPIAccess(Guid requestId, string service, string operation,
            ServiceConsumptionOptions options, object response, HttpStatusCode status,
            TimeSpan processingTime, bool throwOnError = false, bool cachedResponse = false)
        {
            var message = LogMessage.CreateMessage(requestId, options, service, operation, response, status, processingTime, cachedResponse);
            Log("external", $"{service}.{operation}", message, throwOnError);
        }

        public void LogExposedAPIAccess(Guid requestId, HttpActionContext actionContext, TimeSpan processingTime, bool cacheHit)
        {
            LogExposedAPIAccess(requestId,
                $@"{actionContext.ControllerContext.ControllerDescriptor.ControllerName}.{actionContext.ActionDescriptor.ActionName}",
                actionContext.Request, 
                actionContext.Response, 
                processingTime, 
                false,
                cacheHit);
        }

        public void LogExposedAPIAccess(Guid requestId, string apiTitle, HttpRequestMessage request, HttpResponseMessage response, TimeSpan processingTime, bool throwOnError, bool cacheHit)
        {
            var message = LogMessage.CreateMessage(requestId, request, response, processingTime, cacheHit);
            Log("exposed", apiTitle, message, throwOnError);
        }

        public void Publish(string topic, object message)
        {
            if (!EnsureInitialized())
            {
                LogManager.GetLogger(GetType()).DebugFormat("Could not publish to RabbitMQ log. Not initialized. ([{0}] Sent {1})", topic, message);
                return;
            }

            // serialize to json
            var json = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, json.SerializerSettings));

            _channel.BasicPublish(
                exchange: _exchange,
                routingKey: topic,
                basicProperties: null,
                body: body);

            LogManager.GetLogger(GetType()).DebugFormat("[{0}] Sent {1}", topic, message);
        }

        private bool EnsureInitialized()
        {
            try
            {
                Initialize(true);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

#endif
        public void Dispose()
        {
            _connection?.Dispose();
            _channel?.Dispose();
        }
    }
}