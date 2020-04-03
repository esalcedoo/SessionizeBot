using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using System.Collections.Generic;

namespace LuisQnaBot.Models
{
    public class Speaker
    {
        public string Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public string TagLine { get; set; }
        public string ProfilePicture { get; set; }
        public bool IsTopSpeaker { get; set; }
        public List<Session> Sessions { get; set; }
        public string FullName { get; set; }

        public HeroCard ToCard()
        {
            var hero = new HeroCard(
                title: FullName,
                subtitle: TagLine,
                text: string.Join("\n", Bio),
                images: new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = ProfilePicture,
                        Alt = nameof(ProfilePicture)
                    }
                });

            return hero;
        }

        public Choice ToChoice()
        {
            var cardAction = new Choice(value: FullName);

            return cardAction;
        }
    }
}
