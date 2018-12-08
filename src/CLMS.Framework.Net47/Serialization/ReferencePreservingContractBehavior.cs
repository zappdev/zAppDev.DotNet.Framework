#if NETFRAMEWORK
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CLMS.Framework.Serialization
{
	public class ReferencePreservingContractBehavior : IContractBehavior
	{
		private const bool ignoreExtensionDataObject = false;
		private const Int32 maxItemsInObjectGraph = 0xFFFF;

		private readonly bool _on;

		public ReferencePreservingContractBehavior(bool on)
		{
			_on = on;
		}

		#region IContractBehavior Members

		public void AddBindingParameters(ContractDescription contractDescription,
		                                 ServiceEndpoint endpoint,
		                                 BindingParameterCollection bindingParameters)
		{
		}

		public void ApplyClientBehavior(ContractDescription contractDescription,
		                                ServiceEndpoint endpoint,
		                                ClientRuntime clientRuntime)
		{
			ReplaceDataContractSerializerOperationBehaviors(contractDescription, _on);
		}

		public void ApplyDispatchBehavior(ContractDescription contractDescription,
		                                  ServiceEndpoint endpoint,
		                                  DispatchRuntime dispatchRuntime)
		{
			ReplaceDataContractSerializerOperationBehaviors(contractDescription, _on);
		}

		public void Validate(
			ContractDescription contractDescription,
			ServiceEndpoint endpoint)
		{
		}

		#endregion

		internal static void ReplaceDataContractSerializerOperationBehaviors(
			ContractDescription contractDescription,
			bool on)
		{
			foreach (OperationDescription operation in contractDescription.Operations)
			{
				ReplaceDataContractSerializerOperationBehavior(operation, on);
			}
		}

		internal static void ReplaceDataContractSerializerOperationBehavior(
			OperationDescription operation,
			bool on)
		{
			if (operation.Behaviors.Remove(typeof (DataContractSerializerOperationBehavior))
			    || operation.Behaviors.Remove(typeof (ReferencePreservingDataContractSerializerOperationBehavior)))
			{
				operation.Behaviors.Add(new ReferencePreservingDataContractSerializerOperationBehavior(
				                        	operation,
				                        	maxItemsInObjectGraph,
				                        	ignoreExtensionDataObject,
				                        	on));
			}
		}
	}
}
#endif