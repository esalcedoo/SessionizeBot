using Microsoft.Bot.Schema;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LuisQnaBot.Models
{
    public class Session
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartsAt { get; set; }
        public DateTime EndsAt { get; set; }
        public List<Speaker> Speakers { get; set; }
        public int RoomId { get; set; }
        public string Room { get; set; }
        public Speaker Speaker => Speakers.FirstOrDefault();

        public HeroCard ToCard()
        {
            var hero = new HeroCard(
                title: Title,
                subtitle: Room,
                text: string.Join("\n", Description));

            if (Speakers?.Any() ?? false)
                hero.Images = new List<CardImage>()
                {
                    new CardImage()
                    {
                        Url = Speaker.ProfilePicture,
                        Alt = Speaker.TagLine
                    }
                };

            return hero;
        }

    }
}
