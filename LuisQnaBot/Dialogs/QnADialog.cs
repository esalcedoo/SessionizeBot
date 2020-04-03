using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.Dialogs
{
    public class QnADialog : ComponentDialog
    {
        private readonly QnAMaker _qnAMaker;
        private string _initialDialog = nameof(WaterfallDialog) + nameof(QnADialog);

        public QnADialog(QnAMaker qnAMaker) : base(nameof(QnADialog))
        {
            _qnAMaker = qnAMaker;

            AddDialog(new AttachmentPrompt(nameof(AttachmentPrompt)));
            AddDialog(new WaterfallDialog(_initialDialog)
               .AddStep(GetQnAAnswer)
               .AddStep(PromptQnAAnswer));

            // The initial child Dialog to run.
            InitialDialogId = _initialDialog;
        }

        private async Task<DialogTurnResult> GetQnAAnswer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            QueryResult[] responses = await _qnAMaker.GetAnswersAsync(stepContext.Context);
            QueryResult response = responses.FirstOrDefault();
            return await stepContext.NextAsync(response?.Answer ?? "Mejor pregunta por twitter: @netcoreconf");
        }

        private async Task<DialogTurnResult> PromptQnAAnswer(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            string answer = stepContext.Result.ToString();
            var messageActivity = MessageFactory.Text(answer);
            await stepContext.Context.SendActivityAsync(messageActivity);
            return await stepContext.EndDialogAsync();
        }
    }
}
