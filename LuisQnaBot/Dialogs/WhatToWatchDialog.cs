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

        public WhatToWatchDialog(SessionizeService sessionizeService) : base(nameof(WhatToWatchDialog))
        {
            _sessionizeService = sessionizeService;
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog),
                new List<WaterfallStep>()
                {
                    GetTime,
                    PrintTracks,
                    GetSessions,
                    PrintSessions
                }));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            InitialDialogId = nameof(WaterfallDialog);
        }


        private async Task<DialogTurnResult> GetTime(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            RecognizerResult luisResult = stepContext.Context.TurnState.Get<RecognizerResult>("LuisRecognizerResult");
            DateTime? time = luisResult.GetDateTime();
            return await stepContext.NextAsync(time, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PrintTracks(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is not DateTime)
            {
                string text = "¿De qué track te lo muestro?";

                PromptOptions promptOptions = new()
                {
                    Prompt = MessageFactory.Text(text),
                    Choices = ChoiceFactory.ToChoices(_tracks)
                };
                return await stepContext.PromptAsync(nameof(ChoicePrompt), promptOptions, cancellationToken);
            }
            return await stepContext.NextAsync(stepContext.Result, cancellationToken: cancellationToken);

        }
        private async Task<DialogTurnResult> GetSessions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IEnumerable<Session> sessions = default;
            if (stepContext.Result is DateTime time)
            {
                sessions = await _sessionizeService.WhatToWatchAsync(time);
            }
            if (stepContext.Result is FoundChoice track)
            {
                sessions = await _sessionizeService.WhatToWatchAsync(track.Value);
            }
            return await stepContext.NextAsync(sessions, cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> PrintSessions(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IMessageActivity activityMessage = BuildActivityMessage(stepContext.Result as IEnumerable<Session>);
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
