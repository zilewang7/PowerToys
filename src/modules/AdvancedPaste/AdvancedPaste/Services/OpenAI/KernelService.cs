﻿// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

using AdvancedPaste.Models;
using Azure.AI.OpenAI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace AdvancedPaste.Services.OpenAI;

public sealed class KernelService(IKernelQueryCacheService queryCacheService, IAICredentialsProvider aiCredentialsProvider, IPromptModerationService promptModerationService, ICustomTextTransformService customTextTransformService) :
    KernelServiceBase(queryCacheService, promptModerationService, customTextTransformService)
{
    private readonly IAICredentialsProvider _aiCredentialsProvider = aiCredentialsProvider;

    protected override string ModelName => "gpt-4o";

    protected override PromptExecutionSettings PromptExecutionSettings =>
        new OpenAIPromptExecutionSettings()
        {
            ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
            Temperature = 0.01,
        };

    protected override void AddChatCompletionService(IKernelBuilder kernelBuilder) => kernelBuilder.AddOpenAIChatCompletion(
        modelId: ModelName,
        apiKey: _aiCredentialsProvider.Key,
        endpoint: new Uri("https://chatapi.nloli.xyz/v1")
    );

    protected override AIServiceUsage GetAIServiceUsage(ChatMessageContent chatMessage) =>
        chatMessage.Metadata?.GetValueOrDefault("Usage") is CompletionsUsage completionsUsage
            ? new(PromptTokens: completionsUsage.PromptTokens, CompletionTokens: completionsUsage.CompletionTokens)
            : AIServiceUsage.None;
}
