﻿using LuisQnaBot.Dialogs;
using LuisQnaBot.IntentHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Services.LUIS
{
    public class LuisNoneHandler : ILUISeIntentHandler
    {
        private readonly ConversationState _conversationState;
        private readonly QnAMakerBaseDialog _qnADialog;

        public LuisNoneHandler(ConversationState conversationState, QnAMakerBaseDialog qnADialog)
        {
            _conversationState = conversationState;
            _qnADialog = qnADialog;
        }

        public async Task<DialogTurnResult> Handle(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return await dialogContext.BeginDialogAsync(_qnADialog.Id, cancellationToken: cancellationToken);
        }

        public async Task Handle(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await _qnADialog.RunAsync(turnContext,
                            _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        public bool IsValid(string intent) => intent == "None";
    }
}
