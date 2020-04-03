using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using System.Threading;
using System.Threading.Tasks;

namespace LuisQnaBot.IntentHandlers
{
    public interface ILUISeIntentHandler
    {
        //internal Task Handle(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
        bool IsValid(string intent);
        Task<DialogTurnResult> Handle(DialogContext dialogContext, CancellationToken cancellationToken);
        Task Handle(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken);
    }
}
