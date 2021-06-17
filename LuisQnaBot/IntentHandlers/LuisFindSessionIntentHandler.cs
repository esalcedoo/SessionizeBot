using LuisQnaBot.Dialogs;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.IntentHandlers
{
    public class LuisFindSessionIntentHandler : ILUISeIntentHandler
    {
        private readonly ConversationState _conversationState;
        private readonly WhoIsSHeDialog _whoIsSHeDialog;

        public LuisFindSessionIntentHandler(ConversationState conversationState, WhoIsSHeDialog whoIsSHeDialog)
        {
            _conversationState = conversationState;
            _whoIsSHeDialog = whoIsSHeDialog;
        }

        public async Task<DialogTurnResult> Handle(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            //dialogContext.Dialogs.Add(dia);
            return await dialogContext.BeginDialogAsync(_whoIsSHeDialog.Id, cancellationToken: cancellationToken);
        }

        public async Task Handle(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await _whoIsSHeDialog.RunAsync(turnContext,
                            _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        //TODO - NOT IMPLEMENTED FindSessionDialog YET
        public bool IsValid(string intent) => intent == "FindSession";

    }
}
