// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ISpyBot.Dialogs;
using ISpyBot.Model;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace ISpyBot
{
    /// <summary>
    /// Represents a bot that processes incoming activities.
    /// For each user interaction, an instance of this class is created and the OnTurnAsync method is called.
    /// This is a Transient lifetime service.  Transient lifetime services are created
    /// each time they're requested. For each Activity received, a new instance of this
    /// class is created. Objects that are expensive to construct, or have a lifetime
    /// beyond the single turn, should be carefully managed.
    /// For example, the <see cref="MemoryStorage"/> object and associated
    /// <see cref="IStatePropertyAccessor{T}"/> object are created with a singleton lifetime.
    /// </summary>
    /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-2.1"/>
    public class ISpyBotBot : IBot
    {
        private readonly ILogger _logger;
        private readonly ISpyBotAccessors _accessors;
        private DialogSet _dialogs;

        public ISpyBotBot(ISpyBotAccessors accessors, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
            {
                throw new System.ArgumentNullException(nameof(loggerFactory));
            }

            _accessors = accessors ?? throw new ArgumentNullException(nameof(_accessors));
            _logger = loggerFactory.CreateLogger<ISpyBotBot>();

            _dialogs = new DialogSet(_accessors.DialogState);
            ISpyBotDialog.InitialiseDialogSet(_dialogs);
        }

        public async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dialogContext = await _dialogs.CreateContextAsync(turnContext, cancellationToken);
            var results = await dialogContext.ContinueDialogAsync(cancellationToken);

            // Bot added to conversation. Kick off the main dialog.
            if (turnContext.Activity.Type == ActivityTypes.ConversationUpdate)
            {
                if (turnContext.Activity.MembersAdded.Any())
                {
                    if (turnContext.Activity.MembersAdded.First().Id.Contains("bot", StringComparison.OrdinalIgnoreCase))
                    {
                        await dialogContext.BeginDialogAsync(ISpyBotDialog.DialogNames.WaterfallMain, null, cancellationToken);
                    }
                }
            }
            // Message.
            else if (turnContext.Activity.Type == ActivityTypes.Message )
            {
                // If the DialogTurnStatus is Empty we should start a new dialog.
                if (results.Status == DialogTurnStatus.Empty)
                {
                    await dialogContext.BeginDialogAsync(ISpyBotDialog.DialogNames.WaterfallMain, null, cancellationToken);
                }
            }
            // Event
            else if (turnContext.Activity.Type == ActivityTypes.Event)
            {
                // Clear out some not very useful tags for this game.
                var tagsToIgnore = new List<string>() { "indoor", "outdoor" };

                var tags = (turnContext.Activity.Value as JArray)?.ToObject<List<Tag>>();

                tags = tags.Where(t => !tagsToIgnore.Contains(t.Name)).ToList();
            }
        }
    }
}
