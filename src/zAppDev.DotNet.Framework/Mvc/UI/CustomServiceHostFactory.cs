#if NETFRAMEWORK
using System;
using System.Linq;
using System.Xml;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.ServiceModel.Activation;
using System.Xml.Schema;
using System.Collections.Generic;
using System.Configuration;

namespace zAppDev.DotNet.Framework.Web.UI
{
	public class CustomServiceHostFactory : ServiceHostFactory
	{
		protected override ServiceHost CreateServiceHost(Type serviceType, Uri[] baseAddresses)
		{
			string newHost = ConfigurationManager.AppSettings["ServerExternalIP"];

			if (string.IsNullOrEmpty(newHost))
			{
				return base.CreateServiceHost(serviceType, baseAddresses);
			}

			List<Uri> newAddressses = new List<Uri>();
			foreach (var uri in baseAddresses)
			{
				if (uri.Port == 80 && uri.Scheme.ToUpper() == "HTTP")
				{
					newAddressses.Add(new Uri(String.Format("{0}://{1}{2}", uri.Scheme, newHost, uri.AbsolutePath)));
				}
				else
				{
					newAddressses.Add(new Uri(String.Format("{0}://{1}:{2}{3}", uri.Scheme, newHost, uri.Port, uri.AbsolutePath)));
				}
			}

			ServiceHost serviceHost = new ServiceHost(serviceType, newAddressses.ToArray());
			return serviceHost;
		}
	}
}
#endif
