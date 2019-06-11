using System;
using System.Runtime.Serialization;

namespace CLMS.Framework.Workflow
{
    /// <summary>
    /// The WorkflowStatus class
    ///
    /// </summary>
    [Serializable]
    [DataContract]
    public enum WorkflowStatus
    {
        [DataMember(Name = "None")]
        None,
        [DataMember(Name = "Completed")]
        Completed,
        [DataMember(Name = "Pending")]
        Pending,
        [DataMember(Name = "Expired")]
        Expired,
        [DataMember(Name = "Cancelled")]
        Cancelled,
        [DataMember(Name = "Failed")]
        Failed,
        [DataMember(Name = "StepToContinueNotFound")]
        StepToContinueNotFound,
    }
}
