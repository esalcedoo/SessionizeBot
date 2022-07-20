using LuisQnaBot.Dialogs.StateModels;
using LuisQnaBot.Models;
using LuisQnaBot.Services;
using LuisQnaBot.Services.LUIS;
using LuisQnaBot.Services.Sessionize;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Dialogs
{
    /// <summary>
    /// Prints sessions in time range; if datetime not found, ask for a track and prints that track's sessions.
    /// </summary>
    public class WhatToWatchDialog : ComponentDialog
    {
        static readonly string[] _tracks = new[] { "Desarrollo", "Power Platform", "Economía", "Power Bi", "Datos", "Motivación" };

        private readonly SessionizeService _sessionizeService;
        private readonly IStatePropertyAccessor<WhatToWatchUserState> _WhatToWatchUserStatePropertyAccessor;

        public WhatToWatchDialog(SessionizeService sessionizeService, ConversationState conversationState) : base(nameof(WhatToWatchDialog))
        {
            _sessionizeService = sessionizeService;
            _WhatToWatchUserStatePropertyAccessor = conversationState.CreateProperty<WhatToWatchUserState>(nameof(WhatToWatchUserState));

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),
                new List<WaterfallStep>()
                {
                    GetDateTime,
                    PrintTracks,
                    GetTrack,
                    PrintSessions
                }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> GetDateTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            RecognizerResult luisResult = stepContext.Context.TurnState.Get<RecognizerResult>("LuisRecognizerResult");

            WhatToWatchUserState whatToWatchUserState = new WhatToWatchUserState() { DateTime = luisResult.GetDateTime() };
            await _WhatToWatchUserStatePropertyAccessor.SetAsync(stepContext.Context, whatToWatchUserState, cancellationToken);
            return await stepContext.NextAsync(whatToWatchUserState, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PrintTracks(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            WhatToWatchUserState whatToWatchUserState = stepContext.Result as WhatToWatchUserState;

            RecognizerResult luisResult = stepContext.Context.TurnState.Get<RecognizerResult>("LuisRecognizerResult");
            string track = luisResult.GetTrack();

            if (track is null && whatToWatchUserState.DateTime == null)
            {
                string text = "¿De qué track te lo muestro?";

                PromptOptions promptOptions = new()
                {
                    Prompt = MessageFactory.Text(text),
                    Choices = ChoiceFactory.ToChoices(_tracks)
                };
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
            return await stepContext.NextAsync(track, cancellationToken: cancellationToken);

        }

        private async Task<DialogTurnResult> GetTrack(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string track = null;
            if (stepContext.Result is FoundChoice foundChoice)
            {
                track = foundChoice.Value;
            }
            if (stepContext.Result is string trackResult)
            {
                track = trackResult;
            }
            WhatToWatchUserState whatToWatchUserState = await _WhatToWatchUserStatePropertyAccessor.GetAsync(stepContext.Context, null, cancellationToken);
            whatToWatchUserState.Track = track;
            return await stepContext.NextAsync(whatToWatchUserState, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PrintSessions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            WhatToWatchUserState whatToWatchUserState = stepContext.Result as WhatToWatchUserState;

            IEnumerable<Session> sessions = await _sessionizeService.WhatToWatchAsync(whatToWatchUserState.DateTime, whatToWatchUserState.Track);
            
            IMessageActivity activityMessage = BuildActivityMessage(sessions);
            await stepContext.Context.SendActivityAsync(activityMessage, cancellationToken);

            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        private static IMessageActivity BuildActivityMessage(IEnumerable<Session> sessions)
        {
            IMessageActivity activityMessage;
            if (sessions is not null && sessions.Any())
            {
                List<Attachment> attachments = new List<Attachment>();

                foreach (Session session in sessions.OrderBy(s => s.Room))
                {
                    attachments.Add(session.ToCard().ToAttachment());
                }

                activityMessage = MessageFactory.Carousel(attachments, "Aquí tienes:");
            }
            else
            {
                activityMessage = MessageFactory.Text("Descansa la mente. ¡¡en breve nos tienes ahí!!");
            }

            return activityMessage;
        }
    }
}
