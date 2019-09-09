// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Runtime.Serialization;

namespace zAppDev.DotNet.Framework.Workflow
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
