﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;

namespace Microsoft.SemanticKernel;

/// <summary>
/// Builder class for defining targets to listen for in a process.
/// </summary>
public sealed partial class ListenForTargetBuilder : ProcessStepEdgeBuilder
{
    private readonly ProcessBuilder _processBuilder;
    private readonly List<MessageSourceBuilder> _messageSources = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ListenForTargetBuilder"/> class.
    /// </summary>
    /// <param name="messageSources">The list of message sources.</param>
    /// <param name="processBuilder">The process builder.</param>
    /// <param name="edgeGroup">The group ID for the message sources.</param>
    public ListenForTargetBuilder(List<MessageSourceBuilder> messageSources, ProcessBuilder processBuilder, KernelProcessEdgeGroupBuilder? edgeGroup = null) : base(processBuilder, "Aggregate", "Aggregate", edgeGroupBuilder: edgeGroup)
    {
        Verify.NotNullOrEmpty(messageSources, nameof(messageSources));
        this._messageSources = messageSources;
        this._processBuilder = processBuilder;
    }

    /// <summary>
    /// Signals that the output of the source step should be sent to the specified target when the associated event fires.
    /// </summary>
    /// <param name="target">The output target.</param>
    /// <returns>A fresh builder instance for fluid definition</returns>
    public ProcessStepEdgeBuilder SendEventTo(ProcessStepTargetBuilder target)
    {
        return this.SendEventTo_Internal(target);
    }

    /// <summary>
    /// Signals that the output of the source step should be sent to the specified target when the associated event fires.
    /// </summary>
    /// <param name="target">The output target.</param>
    /// <param name="inputs"> The inputs to the target.</param>
    /// <param name="messagesIn"> The messages to be sent to the target.</param>
    /// <param name="thread"> The thread to send the event to.</param>
    /// <returns>A fresh builder instance for fluid definition</returns>
    public ProcessStepEdgeBuilder SendEventToAgent(ProcessAgentBuilder target, Dictionary<string, string>? inputs = null, string? messagesIn = null, string? thread = null)
    {
        // TODO: Move this method to agent builder
        return this.SendEventTo_Internal(new ProcessFunctionTargetBuilder(target));
    }

    /// <summary>
    /// Signals the specified variable update to be performed.
    /// </summary>
    /// <param name="variableUpdate"></param>
    /// <returns></returns>
    public ListenForTargetBuilder Update(VariableUpdate variableUpdate)
    {
        Verify.NotNull(variableUpdate, nameof(variableUpdate));
        this.VariableUpdate = variableUpdate;
        this.SendEventTo_Internal(new ProcessStateTargetBuilder(this.VariableUpdate), this.Metadata);

        return new ListenForTargetBuilder(this._messageSources, this._processBuilder, this.EdgeGroupBuilder);
    }

    /// <summary>
    /// Signals that the specified event should be emitted.
    /// </summary>
    /// <param name="eventName"></param>
    /// <param name="payload"></param>
    /// <returns></returns>
    public ListenForTargetBuilder EmitEvent(string eventName, Dictionary<string, string>? payload = null)
    {
        Verify.NotNullOrWhiteSpace(eventName, nameof(eventName));
        this.SendEventTo_Internal(new ProcessEmitTargetBuilder(eventName, payload), this.Metadata);
        return new ListenForTargetBuilder(this._messageSources, this._processBuilder, this.EdgeGroupBuilder);
    }

    /// <summary>
    /// Signals that the output of the source step should be sent to the specified target when the associated event fires.
    /// </summary>
    /// <param name="target">The output target.</param>
    /// <param name="inputs"> The inputs to the target.</param>
    /// <param name="messagesIn"> The messages to be sent to the target.</param>
    /// <param name="thread"> The thread to send the event to.</param>
    /// <returns>A fresh builder instance for fluid definition</returns>
    public ProcessStepEdgeBuilder SendEventToAgent<TProcessState>(ProcessAgentBuilder<TProcessState> target, Dictionary<string, string>? inputs = null, string? messagesIn = null, string? thread = null) where TProcessState : class, new()
    {
        // TODO: Move this method to agent builder
        var metaData = new Dictionary<string, object?>()
        {
            { "foundryAgent.inputs", inputs },
            { "foundryAgent.messagesIn", messagesIn },
            { "foundryAgent.thread", thread }
        };

        return this.SendEventTo_Internal(new ProcessFunctionTargetBuilder(target), metaData);
    }

    /// <summary>
    /// Sends the event to the specified target.
    /// </summary>
    /// <param name="target">The target to send the event to.</param>
    /// <param name="metadata">Optional metadata to include with the event.</param>
    /// <returns>A new instance of <see cref="ListenForTargetBuilder"/>.</returns>
    internal override ProcessStepEdgeBuilder SendEventTo_Internal(ProcessTargetBuilder target, Dictionary<string, object?>? metadata = null)
    {
        foreach (var messageSource in this._messageSources)
        {
            if (messageSource.Source == null)
            {
                throw new InvalidOperationException("Source step cannot be null.");
            }

            // Link all the source steps to the event listener
            ProcessStepEdgeBuilder? onEventBuilder = null;
            // TODO: seems on function result is not properly supported
            //if (messageSource.MessageType == "default")
            //{
            //    onEventBuilder = messageSource.Source.OnFunctionResult();
            //}
            //else
            //{
            //    onEventBuilder = messageSource.Source.OnEvent(messageSource.MessageType);
            //}

            if (messageSource.Source is ProcessBuilder processSource)
            {
                onEventBuilder = processSource.OnInputEvent(messageSource.MessageType);
            }
            else
            {
                onEventBuilder = messageSource.Source.OnEvent(messageSource.MessageType);
            }

            onEventBuilder.EdgeGroupBuilder = this.EdgeGroupBuilder;
            onEventBuilder.Metadata = metadata ?? [];

            if (messageSource.Condition != null)
            {
                onEventBuilder.Condition = messageSource.Condition;
            }
            onEventBuilder.SendEventTo(target, metadata);
        }

        return new ListenForTargetBuilder(this._messageSources, this._processBuilder, edgeGroup: this.EdgeGroupBuilder);
    }

    /// <summary>
    /// Signals that the process should be stopped.
    /// </summary>
    public override void StopProcess()
    {
        var target = new ProcessFunctionTargetBuilder(EndStep.Instance);

        foreach (var messageSource in this._messageSources)
        {
            if (messageSource.Source == null)
            {
                throw new InvalidOperationException("Source step cannot be null.");
            }

            // Link all the source steps to the event listener
            var onEventBuilder = messageSource.Source.OnEvent(messageSource.MessageType);
            onEventBuilder.EdgeGroupBuilder = this.EdgeGroupBuilder;
            onEventBuilder.SendEventTo(target);
        }
    }
}
