﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Represents the target for an edge in a Process
/// </summary>
public record KernelProcessTarget
{
    /// <summary>
    /// Creates an instance of the <see cref="KernelProcessTarget"/> class.
    /// </summary>
    /// <param name="type"></param>
    public KernelProcessTarget(ProcessTargetType type)
    {
        this.Type = type;
    }

    ///// <summary>
    ///// Initializes a new instance of the <see cref="KernelProcessTarget"/> class.
    ///// </summary>
    ///// <param name="functionTarget"></param>
    //public KernelProcessTarget(KernelProcessFunctionTarget functionTarget)
    //{
    //    Verify.NotNull(functionTarget, nameof(functionTarget));
    //    this.Type = ProcessTargetType.KernelFunction;
    //    this.FunctionTarget = functionTarget;
    //}

    ///// <summary>
    ///// Initializes a new instance of the <see cref="KernelProcessTarget"/> class.
    ///// </summary>
    ///// <param name="variableUpdate"></param>
    //public KernelProcessTarget(VariableUpdate variableUpdate)
    //{
    //    Verify.NotNull(variableUpdate, nameof(variableUpdate));
    //    this.Type = ProcessTargetType.StateUpdate;
    //    this.VariableUpdate = variableUpdate;
    //}

    /// <summary>
    /// The type of target.
    /// </summary>
    public ProcessTargetType Type { get; init; } = ProcessTargetType.Invocation;

    ///// <summary>
    ///// The associated <see cref="KernelProcessFunctionTarget"/>. Null when <see cref="Type"/> is not <see cref="ProcessTargetType.KernelFunction"/>."/>
    ///// </summary>
    //public KernelProcessFunctionTarget? FunctionTarget { get; init; }

    ///// <summary>
    ///// The associated <see cref="VariableUpdate"/>. Null when <see cref="Type"/> is not <see cref="ProcessTargetType.StateUpdate"/>."/>
    ///// </summary>
    //public VariableUpdate? VariableUpdate { get; init; }
}

/// <summary>
/// Represents a state operations target for an edge in a Process
/// </summary>
public record KernelProcessStateTarget : KernelProcessTarget
{
    /// <summary>
    /// Creates an instance of the <see cref="KernelProcessStateTarget"/> class.
    /// </summary>
    public KernelProcessStateTarget(VariableUpdate variableUpdate) : base(ProcessTargetType.StateUpdate)
    {
        this.VariableUpdate = variableUpdate;
    }

    /// <summary>
    /// The associated <see cref="VariableUpdate"/>.
    /// </summary>
    public VariableUpdate VariableUpdate { get; init; }
}

/// <summary>
/// Represents a state operations target for an edge in a Process
/// </summary>
public record KernelProcessEmitTarget : KernelProcessTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KernelProcessEmitTarget"/> class.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="payload"></param>
    public KernelProcessEmitTarget(string eventName, Dictionary<string, string>? payload = null) : base(ProcessTargetType.StateUpdate)
    {
        Verify.NotNullOrWhiteSpace(eventName, nameof(eventName));
        this.EventName = eventName;
        this.Payload = payload;
    }

    /// <summary>
    /// The name or type of the event to be emitted.
    /// </summary>
    public string EventName { get; init; }

    /// <summary>
    /// /// The payload to be sent with the event.
    /// </summary>
    public Dictionary<string, string>? Payload { get; init; }
}

/// <summary>
/// A serializable representation of a specific parameter of a specific function of a specific Step.
/// </summary>
[DataContract]
public record KernelProcessFunctionTarget : KernelProcessTarget
{
    /// <summary>
    /// Creates an instance of the <see cref="KernelProcessFunctionTarget"/> class.
    /// </summary>
    public KernelProcessFunctionTarget(string stepId, string functionName, string? parameterName = null, string? targetEventId = null, Func<Dictionary<string, object?>, Dictionary<string, object?>>? inputMapping = null) : base(ProcessTargetType.KernelFunction)
    {
        Verify.NotNullOrWhiteSpace(stepId);
        Verify.NotNullOrWhiteSpace(functionName);

        this.StepId = stepId;
        this.FunctionName = functionName;
        this.ParameterName = parameterName;
        this.TargetEventId = targetEventId;
        this.InputMapping = inputMapping;
    }

    /// <summary>
    /// The unique identifier of the Step being targeted.
    /// </summary>
    [DataMember]
    public string StepId { get; init; }

    /// <summary>
    /// The name if the Kernel Function to target.
    /// </summary>
    [DataMember]
    public string FunctionName { get; init; }

    /// <summary>
    /// The name of the parameter to target. This may be null if the function has no parameters.
    /// </summary>
    [DataMember]
    public string? ParameterName { get; init; }

    /// <summary>
    /// The unique identifier for the event to target. This may be null if the target is not a sub-process.
    /// </summary>
    [DataMember]
    public string? TargetEventId { get; init; }

    /// <summary>
    /// The mapping function to apply to the input data before passing it to the function.
    /// </summary>
    public Func<Dictionary<string, object?>, Dictionary<string, object?>>? InputMapping { get; init; }
}
