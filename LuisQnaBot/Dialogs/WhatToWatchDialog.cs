using LuisQnaBot.Models;
using LuisQnaBot.Services;
using LuisQnaBot.Services.LUIS;
using LuisQnaBot.Services.Sessionize;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Dialogs
{
    public class WhatToWatchDialog : AttachmentPrompt
    {
        private readonly SessionizeService _sessionizeService;

        public WhatToWatchDialog(SessionizeService sessionizeService):base(nameof(WhatToWatchDialog))
        {
            _sessionizeService = sessionizeService;
        }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            RecognizerResult luisResult = dc.Context.TurnState.Get<RecognizerResult>("LuisRecognizerResult");
            DateTime time = luisResult.GetDateTime();
            if (time == default) time = DateTime.UtcNow.AddHours(2);

            //TODO QUITAR AUMENTAR TIEMPO
            IEnumerable<Session> sessions = await _sessionizeService.WhatToWatchAsync(time.AddDays(1));

            IMessageActivity activityMessage = BuildActivityMessage(sessions);

            await dc.Context.SendActivityAsync(activityMessage, cancellationToken);
            await base.EndDialogAsync(dc.Context, dc.ActiveDialog, DialogReason.EndCalled);

            return null;
        }

        private static IMessageActivity BuildActivityMessage(IEnumerable<Session> sessions)
        {
            IMessageActivity activityMessage;
            if (sessions.Any())
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
