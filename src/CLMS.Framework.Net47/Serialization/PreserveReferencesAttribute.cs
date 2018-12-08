#if NETFRAMEWORK
using System;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace CLMS.Framework.Serialization
{
	[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
	public class PreserveReferencesAttribute : Attribute, IContractBehavior, IOperationBehavior
	{
		private readonly bool _on = true;

		public PreserveReferencesAttribute(bool on)
		{
			_on = on;
		}

		public bool On
		{
			get { return (_on); }
		}

		#region IContractBehavior Members

		void IContractBehavior.AddBindingParameters(
			ContractDescription contractDescription,
			ServiceEndpoint endpoint,
			BindingParameterCollection bindingParameters)
		{
		}

		void IContractBehavior.ApplyClientBehavior(
			ContractDescription contractDescription,
			ServiceEndpoint endpoint,
			ClientRuntime clientRuntime)
		{
			ReferencePreservingContractBehavior.ReplaceDataContractSerializerOperationBehaviors(
				contractDescription, On);
		}

		void IContractBehavior.ApplyDispatchBehavior(
			ContractDescription contractDescription,
			ServiceEndpoint endpoint,
			DispatchRuntime dispatchRuntime)
		{
			ReferencePreservingContractBehavior.ReplaceDataContractSerializerOperationBehaviors(
				contractDescription, On);
		}

		void IContractBehavior.Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
		{
		}

		#endregion

		#region IOperationBehavior Members

		void IOperationBehavior.AddBindingParameters(
			OperationDescription operationDescription,
			BindingParameterCollection bindingParameters)
		{
		}

		void IOperationBehavior.ApplyClientBehavior(
			OperationDescription operationDescription,
			ClientOperation clientOperation)
		{
			ReferencePreservingContractBehavior.ReplaceDataContractSerializerOperationBehavior(
				operationDescription, On);
		}

		void IOperationBehavior.ApplyDispatchBehavior(
			OperationDescription operationDescription,
			DispatchOperation dispatchOperation)
		{
			ReferencePreservingContractBehavior.ReplaceDataContractSerializerOperationBehavior(
				operationDescription, On);
		}

		void IOperationBehavior.Validate(OperationDescription operationDescription)
		{
		}

		#endregion
	}
}
#endif