
using LuisQnaBot.Dialogs;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DialogsCollectionExtension
    {
        public static IServiceCollection AddDialogs(this IServiceCollection services)
        {
            services.AddTransient<MainDialog>();
            services.AddSingleton<QnADialog>();
            services.AddSingleton<WhatToWatchDialog>();
            services.AddSingleton<WhoIsSHeDialog>();
            return services;
        }
    }
}