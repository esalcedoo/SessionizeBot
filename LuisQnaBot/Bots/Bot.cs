using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LuisQnaBot.Dialogs;
using LuisQnaBot.IntentHandlers;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;

namespace LuisQnaBot.Bots
{
    public class Bot<T> : ActivityHandler where T : Dialog
    {
        private ConversationState _conversationState;
        private readonly IEnumerable<ILUISeIntentHandler> _intentHandlers;
        private readonly T _dialog;
        private readonly QnAMakerBaseDialog _qnADialog;
        private readonly WhatToWatchDialog _whatToWatchDialog;
        private readonly WhoIsSHeDialog _whoIsSHeDialog;

        public Bot(ConversationState conversationState, IEnumerable<ILUISeIntentHandler> intentHandlers, T dialog, QnAMakerBaseDialog qnADialog, WhatToWatchDialog whatToWatchDialog, WhoIsSHeDialog whoIsSHeDialog)
        {
            _conversationState = conversationState;
            _intentHandlers = intentHandlers;
            _dialog = dialog;
            _qnADialog = qnADialog;
            _whatToWatchDialog = whatToWatchDialog;
            _whoIsSHeDialog = whoIsSHeDialog;
        }

        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default)
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            // Save any state changes that might have occured during the turn.
            await _conversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            DialogSet dialogSet = new DialogSet(_conversationState.CreateProperty<DialogState>("DialogState"));

            dialogSet.Add(_qnADialog); 
            dialogSet.Add(_whatToWatchDialog);
            dialogSet.Add(_whoIsSHeDialog);

            DialogContext dialogContext = await dialogSet.CreateContextAsync(turnContext, cancellationToken);
            DialogTurnResult results = await dialogContext.ContinueDialogAsync(cancellationToken);

            if (results.Status == DialogTurnStatus.Empty)
            {
                string luisIntentKey = GetTopIntentKey(turnContext);

                var intentHandler = _intentHandlers.FirstOrDefault(handler =>
                    handler.IsValid(luisIntentKey));

                if (intentHandler is null)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(
                        $"Intención {luisIntentKey} no soportada."));
                    return;
                }

                await intentHandler.Handle(dialogContext, cancellationToken);
            }
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hola anfitriona! Puedes preguntarme por qué sesión ver; o sobre la biografía de cierta speaker."), cancellationToken);
                }
            }
        }

        private static string GetTopIntentKey(ITurnContext turnContext)
        {
            RecognizerResult luisResult = turnContext.TurnState
                            .Get<RecognizerResult>("LuisRecognizerResult");
            return luisResult.Intents.FirstOrDefault().Key;
        }
    }
}
