// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using Microsoft.Extensions.DependencyInjection;
using Autofac;

#if NETFRAMEWORK
using CommonServiceLocator;
#else
using Autofac.Extensions.DependencyInjection;
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

        public static bool ServiceProviderIsLoaded() => _serviceProvider != null;

#if NETFRAMEWORK
#else
        public object GetInstance(Type serviceType, string name = null)
        {
            if (string.IsNullOrEmpty(name))
                return _currentServiceProvider.GetService(serviceType);

            if (_currentServiceProvider is AutofacServiceProvider autofac)
            {
                return autofac.LifetimeScope.ResolveNamed(name, serviceType);
            }

            throw new Exception("Instance not found");
        }

        public TService GetInstance<TService>(string name = null)
        {
            if (string.IsNullOrEmpty(name))
                return _currentServiceProvider.GetService<TService>();

            try
            {
                if (_currentServiceProvider is AutofacServiceProvider autofac)
                {
                    return autofac.LifetimeScope.ResolveNamed<TService>(name);
                }
                else
                {
                    return default;
                }
            }
            catch (ArgumentException)
            {
                return default;
            }
        }
#endif
    }
}