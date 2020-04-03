using LuisQnaBot.Dialogs;
using LuisQnaBot.IntentHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Services.LUIS
{
    public class LuisWhatToWatchIntentHandler : ILUISeIntentHandler
    {
        private readonly ConversationState _conversationState;
        private readonly WhatToWatchDialog _whatToWatchNowDialog;

        public LuisWhatToWatchIntentHandler(ConversationState conversationState, WhatToWatchDialog whatToWatchNowDialog)
        {
            _conversationState = conversationState;
            _whatToWatchNowDialog = whatToWatchNowDialog;
        }

        public async Task<DialogTurnResult> Handle(DialogContext dialogContext, CancellationToken cancellationToken)
        {
            return await dialogContext.BeginDialogAsync(_whatToWatchNowDialog.Id, cancellationToken: cancellationToken);
        }

        public async Task Handle(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            await _whatToWatchNowDialog.RunAsync(turnContext,
                            _conversationState.CreateProperty<DialogState>(nameof(DialogState)), cancellationToken);
        }

        public bool IsValid(string intent) => intent == "WhatToWatch";
    }
}
