using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ISpyBot.Dialogs
{
    public class ISpyBotDialog
    {
        public class DialogNames
        {
            public const string WaterfallMain = "WaterfallMain";
            public const string PromptConfirmPlayISpy = "PromptConfirmPlayISpy";
            public const string ProcessConfirmPlay = "ProcessConfirmPlay";
        }


        public static void InitialiseDialogSet(DialogSet dialogSet)
        {
            dialogSet.Add(new WaterfallDialog(DialogNames.WaterfallMain, new WaterfallStep[]
            {
                StepConfirmPlayISpyAsync,
                StepProcessConfirmPlayAsync
            }));

            dialogSet.Add(new ConfirmPrompt(DialogNames.PromptConfirmPlayISpy));
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
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.StartingCamera), cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text(Constants.Messages.DontPlay), cancellationToken);
            }

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }
    }
}
