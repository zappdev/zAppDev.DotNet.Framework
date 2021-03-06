﻿// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace zAppDev.DotNet.Framework.Workflow
{
    public interface IWorkflowContext
    {
        string Error { get; set; }
        DateTime? ExpirationDateTime { get; set; }
        bool Expires { get; set; }
        Guid? Id { get; set; }
        string Name { get; set; }
        string PendingJobCreatedBy { get; set; }
        DateTime? PendingSince { get; set; }
        string PendingStep { get; set; }
        WorkflowExecutionResult Result { get; set; }
        WorkflowStatus? Status { get; set; }
        Guid TransientId { get; set; }

        WorkflowContextBase Copy(bool deep = false, Hashtable copiedObjects = null, bool asNew = false, bool reuseNestedObjects = false, WorkflowContextBase copy = null);
        bool Equals(object obj);
        int GetHashCode();
        PropertyInfo GetPrimaryKey();
        bool IsTransient();
        int _GetUniqueIdentifier();
        List<string> _Validate(bool throwException = true);
    }
}