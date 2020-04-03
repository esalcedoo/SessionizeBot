using LuisQnaBot.IntentHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        private readonly IEnumerable<ILUISeIntentHandler> _intentHandlers;
        private readonly WhoIsSHeDialog _whoIsSHeDialog;
        private readonly WhatToWatchDialog _whatToWatchDialog;
        private readonly QnADialog _qnADialog;

        public MainDialog(IEnumerable<ILUISeIntentHandler> intentHandlers, WhoIsSHeDialog whoIsSHeDialog, WhatToWatchDialog whatToWatchDialog, QnADialog qnADialog):base(nameof(MainDialog))
        {
            _intentHandlers = intentHandlers;
            _whoIsSHeDialog = whoIsSHeDialog;
            _whatToWatchDialog = whatToWatchDialog;
            _qnADialog = qnADialog;

            AddDialog(whoIsSHeDialog);
            AddDialog(whatToWatchDialog);
            AddDialog(qnADialog);
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            string luisIntentKey = GetTopIntentKey(dc.Context);

            var intentHandler = _intentHandlers.FirstOrDefault(handler =>
                handler.IsValid(luisIntentKey));

            if (intentHandler is null)
            {
                await dc.Context.SendActivityAsync(MessageFactory.Text(
                    $"Intención {luisIntentKey} no soportada."));

                return await dc.EndDialogAsync();
            }
            return await intentHandler.Handle(dc, cancellationToken);
        }

        private static string GetTopIntentKey(ITurnContext turnContext)
        {
            RecognizerResult luisResult = turnContext.TurnState
                            .Get<RecognizerResult>("LuisRecognizerResult");
            return luisResult.Intents.FirstOrDefault().Key;
        }
    }
}
