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

        public class DialogNames
        {
            public const string WaterfallWantToPlay = "WaterfallWantToPlay";
            public const string PromptConfirmPlayISpy = "PromptConfirmPlayISpy";
            public const string ProcessConfirmPlay = "ProcessConfirmPlay";
            public const string WaterfallPlayGame = "WaterfallPlayGame";
            public const string PromptForAnswer = "PromptForAnswer";
        }

        public ISpyBotDialogSet(ISpyBotAccessors accessors) : base(accessors.DialogState)
        {
            _accessors = accessors;

            Add(new WaterfallDialog(DialogNames.WaterfallWantToPlay, new WaterfallStep[]
            {
                StepConfirmPlayISpyAsync,
                StepProcessConfirmPlayAsync
            }));

            Add(new ConfirmPrompt(DialogNames.PromptConfirmPlayISpy));

            Add(new WaterfallDialog(DialogNames.WaterfallPlayGame, new WaterfallStep[]
            {
                StepPromptForAnswerAsync,
                StepConfirmAnswerAsync
            }));

            Add(new TextPrompt(DialogNames.PromptForAnswer));
        }

        /// <summary>
        /// Prompt the user whether they want to play I Spy.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StepConfirmPlayISpyAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(DialogNames.PromptConfirmPlayISpy, new PromptOptions { Prompt = MessageFactory.Text(Constants.Messages.PromptPlayISpy, inputHint: Constants.InputHint.ExpectingInput) }, cancellationToken);
        }

        /// <summary>
        /// Checks whether or not the user wants to play.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StepProcessConfirmPlayAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            // Yes, wants to play.
            if ((bool)stepContext.Result)
            {
                // Set bot state to indicate we're waiting for tags.
                var botState = await _accessors.ISpyBotState.GetAsync(stepContext.Context, () => new ISpyBotState());
                botState.WaitingForTagsFromVision = true;
                await _accessors.ISpyBotState.SetAsync(stepContext.Context, botState);
                await _accessors.ConversationState.SaveChangesAsync(stepContext.Context);

                // Send a message.
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.StartingCamera), cancellationToken);

                // Send an event so the page fires up the camera.
                var eventAction = stepContext.Context.Activity.CreateReply();
                eventAction.Type = ActivityTypes.Event;
                eventAction.Name = Constants.BotEvents.ReadyForCamera;
                await stepContext.Context.SendActivityAsync(eventAction);

            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.DontPlay), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// User has started the game, prompt for an answer.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StepPromptForAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var botState = await _accessors.ISpyBotState.GetAsync(stepContext.Context, () => new ISpyBotState());

            var firstLetter = botState.ObjectChosenByBot.ToUpper()[0];

            return await stepContext.PromptAsync(DialogNames.PromptForAnswer, new PromptOptions { Prompt = MessageFactory.Text(string.Format(Constants.Messages.ISpy, firstLetter), inputHint: Constants.InputHint.ExpectingInput) }, cancellationToken);
        }

        /// <summary>
        /// Checks the answer the user gave.
        /// </summary>
        /// <param name="stepContext">The step context.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns></returns>
        private async Task<DialogTurnResult> StepConfirmAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var guess = (string)stepContext.Result;
            guess = guess.Trim().ToLower();

            var botState = await _accessors.ISpyBotState.GetAsync(stepContext.Context, () => new ISpyBotState());

            var objectChosenByBotTrimmedToLower = botState.ObjectChosenByBot.Trim().ToLower();

            if (objectChosenByBotTrimmedToLower.StartsWith(guess) || guess.StartsWith(objectChosenByBotTrimmedToLower))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(string.Format(Constants.Messages.Correct, botState.ObjectChosenByBot.Trim())), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(string.Format(Constants.Messages.Incorrect, botState.ObjectChosenByBot.Trim())), cancellationToken);
            }

            await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.PlayAgain), cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
