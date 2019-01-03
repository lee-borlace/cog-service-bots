// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;

namespace WatcherBot
{
  
    public class WatcherBotAccessors
    {
        public ConversationState ConversationState { get; }

        public UserState UserState { get; }

        public WatcherBotAccessors(ConversationState conversationState, UserState userState)
        {
            ConversationState = conversationState ?? throw new ArgumentNullException(nameof(conversationState));
            UserState = userState ?? throw new ArgumentNullException(nameof(userState));
        }

        public static string WatcherBotStateName { get; } = $"{nameof(WatcherBotAccessors)}.WatcherBotState";

        public IStatePropertyAccessor<WatcherBotState> WatcherBotState { get; set; }

        public static string DialogStateName { get; } = $"{nameof(WatcherBotAccessors)}.DialogState";

        public IStatePropertyAccessor<DialogState> DialogState { get; set; }



    }
}
