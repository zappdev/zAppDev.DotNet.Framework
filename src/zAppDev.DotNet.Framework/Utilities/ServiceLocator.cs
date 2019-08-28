using System;
using Microsoft.Extensions.DependencyInjection;

#if NETFRAMEWORK
using CommonServiceLocator;
#endif

namespace zAppDev.DotNet.Framework.Utilities
{
    /// <summary>
    /// ServiceLocator Shim For .NET Core
    /// </summary>
    public class ServiceLocator
    {
        private readonly IServiceProvider _currentServiceProvider;

        [ThreadStatic]
        private static IServiceProvider _serviceProvider;

        public IServiceProvider CurrentServiceProvider { get => _currentServiceProvider; private set {} }

        public ServiceLocator(IServiceProvider currentServiceProvider)
        {
            _currentServiceProvider = currentServiceProvider;
        }

#if NETFRAMEWORK
        public static IServiceLocator Current => CommonServiceLocator.ServiceLocator.Current;
#else
        public static ServiceLocator Current => new ServiceLocator(_serviceProvider);
#endif

        public static void SetLocatorProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetInstance(Type serviceType)
        {
            return _currentServiceProvider.GetService(serviceType);
        }

        public TService GetInstance<TService>()
        {
            try
            {
                return _currentServiceProvider.GetService<TService>();            
            }
            catch (ArgumentException)
            {
                return default(TService);
            }
        }
    }
}