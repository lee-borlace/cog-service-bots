using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ISpyBot.Dialogs
{
    public class ISpyBotDialogSet : DialogSet
    {
        private ISpyBotAccessors _accessors;

        public ISpyBotDialogSet(ISpyBotAccessors accessors) : base(accessors.DialogState)
        {
            _accessors = accessors;

            Add(new WaterfallDialog(DialogNames.WaterfallMain, new WaterfallStep[]
            {
                StepConfirmPlayISpyAsync,
                StepProcessConfirmPlayAsync
            }));

            Add(new ConfirmPrompt(DialogNames.PromptConfirmPlayISpy));
        }

        public class DialogNames
        {
            public const string WaterfallMain = "WaterfallMain";
            public const string PromptConfirmPlayISpy = "PromptConfirmPlayISpy";
            public const string ProcessConfirmPlay = "ProcessConfirmPlay";
        }
        

        /// <summary>
        /// Prompt the user whether they want to play I Spy.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<DialogTurnResult> StepConfirmPlayISpyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(DialogNames.PromptConfirmPlayISpy, new PromptOptions { Prompt = MessageFactory.Text(Constants.Messages.PromptPlayISpy, inputHint:Constants.InputHint.ExpectingInput) }, cancellationToken);
        }

        /// <summary>
        /// Checks whether or not the user wants to play.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private static async Task<DialogTurnResult> StepProcessConfirmPlayAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Yes, wants to play.
            if ((bool)stepContext.Result)
            {
                // Send a message.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.StartingCamera), cancellationToken);

                // Send an event so the page fires up the camera.
                var eventAction = stepContext.Context.Activity.CreateReply();
                eventAction.Type = ActivityTypes.Event;
                eventAction.Name = Constants.BotEvents.ReadyForCamera;
                await stepContext.Context.SendActivityAsync(eventAction);

                // Set bot state to indicate we're waiting for tags.
                
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.DontPlay), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
