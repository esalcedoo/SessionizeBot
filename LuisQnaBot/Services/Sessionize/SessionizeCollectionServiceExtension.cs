using LuisQnaBot.Services.Sessionize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class SessionizeCollectionServiceExtension
    {
        public static IServiceCollection AddSessionize(this IServiceCollection services)
        {
            services.AddHttpClient<SessionizeService>(client =>
            {
                client.BaseAddress = new Uri("https://sessionize.com/api/v2/*******/view/");
                client.DefaultRequestHeaders.Add(System.Net.HttpRequestHeader.Accept.ToString(), "application/json");
            }
            );
            return services;
        }
    }
}
