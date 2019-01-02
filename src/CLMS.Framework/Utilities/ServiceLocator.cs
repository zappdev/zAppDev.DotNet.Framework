#if NETSTANDARD
using Microsoft.Extensions.DependencyInjection;
#endif
using System;

namespace CLMS.Framework.Utilities
{
    /// <summary>
    /// ServiceLocator Shim For .NET Core
    /// </summary>
    public class ServiceLocator
    {
#if NETSTANDARD
        private readonly ServiceProvider _currentServiceProvider;

        [ThreadStatic]
        private static ServiceProvider _serviceProvider;

        public ServiceLocator(ServiceProvider currentServiceProvider)
        {
            _currentServiceProvider = currentServiceProvider;
        }

        public static ServiceLocator Current => new ServiceLocator(_serviceProvider);

        public static void SetLocatorProvider(ServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public object GetInstance(Type serviceType)
        {
            return _currentServiceProvider.GetService(serviceType);
        }

        public TService GetInstance<TService>()
        {
            return _currentServiceProvider.GetService<TService>();
        }
#endif
    }
}