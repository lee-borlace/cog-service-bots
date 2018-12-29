// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace ISpyBot
{
    /// <summary>
    /// This class is created as a Singleton and passed into the IBot-derived constructor.
    ///  - See <see cref="EchoWithCounterBot"/> constructor for how that is injected.
    ///  - See the Startup.cs file for more details on creating the Singleton that gets
    ///    injected into the constructor.
    /// </summary>
    public class ISpyBotAccessors
    {
        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public ISpyBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }
      
        public static string ConversationDataName { get; } = $"{nameof(ISpyBotAccessors)}.ConversationData";
       
        public IStatePropertyAccessor<ConversationData> ConversationData { get; set; }

        public static string DialogStateName { get; } = $"{nameof(ISpyBotAccessors)}.DialogState";

        public IStatePropertyAccessor<DialogState> DialogState { get; set; }



    }
}
