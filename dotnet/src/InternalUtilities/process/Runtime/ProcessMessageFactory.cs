﻿// Copyright (c) Microsoft. All rights reserved.

using System.Collections.Generic;

namespace Microsoft.SemanticKernel.Process.Runtime;

/// <summary>
/// A factory class for creating <see cref="ProcessMessage"/> instances.
/// </summary>
internal static class ProcessMessageFactory
{
    /// <summary>
    /// Creates a new <see cref="ProcessMessage"/> instance from a <see cref="KernelProcessEdge"/> and a data object.
    /// </summary>
    /// <param name="edge">An instance of <see cref="KernelProcessEdge"/></param>
    /// <param name="sourceEventId">id of the source steps generating the event</param>
    /// <param name="data">A data object.</param>
    /// <param name="writtenToThread">Optional thread id where the event was written</param>
    /// <returns>An instance of <see cref="ProcessMessage"/></returns>
    internal static ProcessMessage CreateFromEdge(KernelProcessEdge edge, string sourceEventId, object? data, string? writtenToThread = null)
    {
        KernelProcessFunctionTarget target = (edge.OutputTarget as KernelProcessFunctionTarget)!;
        Dictionary<string, object?> parameterValue = [];
        if (!string.IsNullOrWhiteSpace(target.ParameterName))
        {
            parameterValue.Add(target.ParameterName!, data);
        }

        ProcessMessage newMessage = new(edge.SourceStepId, target.StepId, target.FunctionName, parameterValue)
        {
            SourceEventId = sourceEventId,
            TargetEventId = target.TargetEventId,
            TargetEventData = data,
            GroupId = edge.GroupId,
            writtenToThread = writtenToThread
        };

        return newMessage;
    }
}
