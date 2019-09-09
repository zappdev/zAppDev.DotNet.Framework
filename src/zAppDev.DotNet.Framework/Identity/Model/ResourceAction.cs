// Copyright (c) 2017 CLMS. All rights reserved.
// Licensed under the AGPL-3.0 license. See LICENSE file in the project root for full license information.
using System;

namespace zAppDev.DotNet.Framework.Identity.Model
{
    internal class ResourceAction
    {
        public readonly string Action;
        public readonly string ActionType;

        public readonly string Resource;
        public readonly string ResourceType;

        public ResourceAction(string resource, string action)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }
            ResourceType = ClaimTypes.ResourceType;
            Resource = resource;
            ActionType = ClaimTypes.ActionType;
            Action = action;
        }

        public ResourceAction(string resourceType, string resource, string actionType, string action)
        {
            if (string.IsNullOrEmpty(resourceType))
            {
                throw new ArgumentNullException(nameof(resourceType));
            }
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentNullException(nameof(resource));
            }
            if (string.IsNullOrEmpty(actionType))
            {
                throw new ArgumentNullException(nameof(actionType));
            }
            if (string.IsNullOrEmpty(action))
            {
                throw new ArgumentNullException(nameof(action));
            }
            ResourceType = resourceType;
            Resource = resource;
            ActionType = actionType;
            Action = action;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is ResourceAction other))
            {
                return obj != null && obj.Equals(this);
            }
            return ((string.CompareOrdinal(other.ResourceType, ResourceType) == 0) &&
                    (string.CompareOrdinal(other.Resource, Resource) == 0) &&
                    (string.CompareOrdinal(other.ActionType, ActionType) == 0) &&
                    (string.CompareOrdinal(other.Action, Action) == 0));
        }

        public override int GetHashCode()
        {
            return (ResourceType.GetHashCode() ^ Resource.GetHashCode() ^
                    ActionType.GetHashCode() ^ Action.GetHashCode());
        }
    }
}