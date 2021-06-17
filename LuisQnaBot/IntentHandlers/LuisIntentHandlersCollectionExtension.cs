using LuisQnaBot.IntentHandlers;
using LuisQnaBot.Services.LUIS;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class LuisIntentHandlersCollectionExtension
    {
        public static IServiceCollection AddLuisIntentHandlers(this IServiceCollection services)
        {
            services.AddTransient<ILUISeIntentHandler, LuisWhatToWatchIntentHandler>();
            services.AddTransient<ILUISeIntentHandler, LuisNoneHandler>();
            services.AddTransient<ILUISeIntentHandler, LuisWhoIsSHeIntentHandler>();
            services.AddTransient<ILUISeIntentHandler, LuisFindSessionIntentHandler>();
            return services;
        }
    }
}
