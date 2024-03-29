﻿using LuisQnaBot.BotBuilderMiddlewares;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuisQnaBot
{
    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        public AdapterWithErrorHandler(IConfiguration configuration, LuisRecognizerMiddleware luisRecognizerMiddleware, ILogger<BotFrameworkHttpAdapter> logger)
            : base(configuration, logger)
        {
            MiddlewareSet.Use(luisRecognizerMiddleware);

            OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync("Se ha producido un error.");
            };
        }
    }
}
