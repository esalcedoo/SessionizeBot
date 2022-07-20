using Luis;
using Microsoft.Azure.CognitiveServices.Language.LUIS.Runtime.Models;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Recognizers.Text.DataTypes.TimexExpression;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuisQnaBot.Services.LUIS
{
    public static class LuisEntitiesExtension
    {
        public static DateTime? GetDateTime(this RecognizerResult recognizerResult)
        {
            var luisResponse = new SessionizeLuisModel();
            luisResponse.Convert(recognizerResult);

            DateTime dateTime;
            if (TryFindDateTime(luisResponse, out dateTime))
            {
                return dateTime;
            }
            return null;
        }

        public static bool TryFindDateTime(SessionizeLuisModel luisResponse, out DateTime dateTime)
        {
            DateTimeSpec dateTimeSpec = luisResponse.Entities.datetime?.FirstOrDefault();

            dateTime = default;
            if (dateTimeSpec != null)
                foreach (string expresion in dateTimeSpec?.Expressions)
                {
                    TimexProperty parsed = new TimexProperty(expresion);
                    if (parsed.TimexValue.Equals("PRESENT_REF", StringComparison.CurrentCultureIgnoreCase))
                    {
                        dateTime = DateTime.UtcNow.AddHours(2);
                        return true;
                    }
                    else
                    {
                        
                        dateTime = new DateTime(
                            year: parsed.Year ?? DateTime.UtcNow.Year,
                            month: parsed.Month ?? DateTime.UtcNow.Month,
                            day: (int)(parsed.DayOfWeek != null ? DateTime.UtcNow.AddDays(GetDaysToAdd(parsed.DayOfWeek)).Day : parsed.DayOfMonth),
                            hour: parsed.Hour ?? 0,
                            minute: parsed.Minute ?? 0,
                            second: parsed.Second ?? 0
                            );
                        return true;
                    }
                }
            return false;
        }

        private static double GetDaysToAdd(int? dayOfWeek)
        {
            int add = 0;
            if (dayOfWeek > ((int)DateTime.Now.DayOfWeek))
            {
                add = (int)(dayOfWeek - (DateTime.UtcNow.Day % 7));
            }
            else if (dayOfWeek < ((int)DateTime.Now.DayOfWeek))
            {
                add = (int)(7 + dayOfWeek - (int)DateTime.Now.DayOfWeek);
            }
            return add;
        }

        //public static bool TryFindTimeAdverPhrase(SessionizeLuisModel luisResponse, out DateTime dateTime)
        //{
        //    dateTime = default;
        //    if (int.TryParse(luisResponse.Entities.TimeAdverbPhrase?[0][0], out int hour))
        //    {
        //        dateTime = new DateTime(
        //                year: DateTime.UtcNow.Year,
        //                month: DateTime.UtcNow.Month,
        //                day: DateTime.UtcNow.Day,
        //                hour: hour,
        //                minute: 0,
        //                second: 0
        //                );

        //        return true;
        //    }
        //    return false;
        //}

        public static string GetSpeakerName(this RecognizerResult recognizerResult)
        {
            var luisResponse = new SessionizeLuisModel();
            luisResponse.Convert(recognizerResult);

            string name = luisResponse.Entities.Speaker?[0][0];

            return name;
        }

        public static string GetTrack(this RecognizerResult recognizerResult)
        {
            var luisResponse = new SessionizeLuisModel();
            luisResponse.Convert(recognizerResult);

            string name = luisResponse.Entities.track?[0][0];

            return name;
        }
    }
}
