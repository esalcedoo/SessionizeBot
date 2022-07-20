using LuisQnaBot.Models;
using LuisQnaBot.Services.LUIS;
using LuisQnaBot.Services.Sessionize;
using LuisQnaBot.Utils;
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
    public class WhoIsSHeDialog : ComponentDialog
    {
        private readonly SessionizeService _sessionizeService;
        private string _initialDialog = nameof(WaterfallDialog) + nameof(WhoIsSHeDialog);

        public WhoIsSHeDialog(SessionizeService sessionizeService) : base(nameof(WhoIsSHeDialog))
        {
            _sessionizeService = sessionizeService;

            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(_initialDialog)
               .AddStep(CheckSpeakers)
               .AddStep(GetSpeaker)
               .AddStep(PrintSpeaker)
               .AddStep(EndDialog));

            // The initial child Dialog to run.
            InitialDialogId = _initialDialog;
        }

        private async Task<DialogTurnResult> CheckSpeakers(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            IEnumerable<Speaker> speakers = await GetSpeakers(stepContext);
            if (speakers.Count() > 1)
                return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions()
                {
                    Prompt = MessageFactory.Text("¿A cuál de ellas te refieres?", inputHint: InputHints.ExpectingInput),
                    Choices = ChoiceFactory.ToChoices(speakers.Select(s=>s.FullName).ToList())
                });
            else
                return await stepContext.NextAsync(speakers.FirstOrDefault(), cancellationToken);
        }

        private async Task<DialogTurnResult> GetSpeaker(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            Speaker speaker = null;

            if (stepContext.Result is FoundChoice speakerName)
            {
                IEnumerable<Speaker> speakers = await GetSpeakers(stepContext);
                speaker = speakers.FirstOrDefault(s => s.FullName.Contains(speakerName.Value));
            }

            speaker ??= stepContext.Result as Speaker;

            return await stepContext.NextAsync(speaker, cancellationToken);
        }

        private async Task<DialogTurnResult> PrintSpeaker(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (stepContext.Result is Speaker speaker)
            {
                await stepContext.Context.SendActivityAsync(BuildActivityMessage(speaker), cancellationToken);
            }
            return await stepContext.NextAsync(cancellationToken: cancellationToken);
        }

        private async Task<DialogTurnResult> EndDialog(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        }

        private static IMessageActivity BuildActivityMessage(Speaker speaker)
        {
            IMessageActivity activityMessage;
            if (speaker != null)
            {
                activityMessage = MessageFactory.Attachment(speaker.ToCard().ToAttachment(), "Con ese nombre he encontrado a esta ponente:");
            }
            else
            {
                activityMessage = MessageFactory.Text("No he visto a nadie con ese nombre por aquí...");
            }

            return activityMessage;
        }


        private async Task<IEnumerable<Speaker>> GetSpeakers(WaterfallStepContext stepContext)
        {
            RecognizerResult luisResult = stepContext.Context.TurnState.Get<RecognizerResult>("LuisRecognizerResult");

            string name = luisResult.GetSpeakerName();
            DateTime? time = luisResult.GetDateTime();
            string track = luisResult.GetTrack();

            IEnumerable<Speaker> speakers = await _sessionizeService.WhoIsSHeAsync(
                stepContext.Context.Activity.Text,
                name: name,
                time,
                track: track); //TODO QUITAR AUMENTAR TIEMPO
            return speakers;
        }

    }
}
