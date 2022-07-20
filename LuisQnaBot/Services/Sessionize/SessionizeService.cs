using LuisQnaBot.Models;
using LuisQnaBot.Utils;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace LuisQnaBot.Services.Sessionize
{
    public class SessionizeService
    {
        private readonly HttpClient _client;
        private readonly IMemoryCache _memoryCache;

        public SessionizeService(HttpClient client, IMemoryCache memoryCache)
        {
            _client = client;
            _memoryCache = memoryCache;
        }

        public async Task<IEnumerable<Session>> WhatToWatchAsync(DateTime? dateTime = null, string track = null)
        {
            IEnumerable<Session> sessions = await GetSessionsAsync();

            if (dateTime is DateTime datetime)
            {
                if (datetime.Hour != 0)
                {
                    sessions = sessions
                    .Where(session => (session.StartsAt <= datetime
                                        && datetime < session.EndsAt) || session.StartsAt <= datetime.AddMinutes(10));
                }
                else
                {
                    sessions = sessions
                           .Where(session => session.StartsAt.Day == datetime.Day);
                }
            }
            if (track is not null)
            {
                sessions = sessions.Where(session => string.Equals(session.Room, track, StringComparison.CurrentCultureIgnoreCase));
            }

            return sessions;
        }

        public async Task<IEnumerable<Session>> WhatToWatchAsync(DateTime dateTime)
        {
            IEnumerable<Session> sessions = await GetSessionsAsync();

            if (dateTime.Minute < 30)
            {
                return sessions
                .Where(session => session.StartsAt <= dateTime
                                    && dateTime < session.EndsAt);
            }
            else
            {
                return sessions
                   .Where(session => session.StartsAt <= dateTime.AddMinutes(55)
                                       && dateTime.AddMinutes(55) < session.EndsAt);
            }
        }

        public async Task<IEnumerable<Session>> WhatToWatchAsync(string track)
        {
            IEnumerable<Session> sessions = await GetSessionsAsync();

            return sessions
            .Where(session => session.Room == track);
        }

        public async Task<IEnumerable<Speaker>> WhoIsSHeAsync(string fullName, string name = default, DateTime? dateTime = default, string track = default)
        {
            IEnumerable<Speaker> speakers = await GetSpeakersAsync();

            if (!string.IsNullOrEmpty(name))
            {
                if (speakers.Any(speaker => string.Equals(speaker.FullName, fullName)))
                {
                    speakers = speakers.Where(speaker => string.Equals(speaker.FullName, fullName)).ToList();
                }
                else
                {
                    speakers = speakers
                           .Where(speaker =>
                           {
                               speaker.FullName = speaker.FullName.Count() > 19 ? speaker.FullName.Remove(speaker.FullName.LastIndexOf(' ')).TrimEnd() : speaker.FullName;
                               var speakerWithOutAccentMarks = speaker.FullName.RemoveAccentMark();
                               return speakerWithOutAccentMarks.Contains(name.RemoveAccentMark());
                           }).ToList();
                }
            }

            if (dateTime > DateTime.Now.AddDays(-1))
                speakers = speakers
                   .Where(speaker => speaker.Sessions.Any(s => s.StartsAt <= dateTime && dateTime < s.EndsAt));

            if (!string.IsNullOrEmpty(track))
                speakers = speakers
                   .Where(speaker => speaker.Sessions.FirstOrDefault()?.Room.Equals(track, StringComparison.InvariantCultureIgnoreCase) ?? false);
            return speakers;
        }

        public async Task<Session> KeyNoteAsync()
        {
            IEnumerable<Session> sessions = await GetSessionsAsync();
            return sessions.OrderBy(session => session.StartsAt).FirstOrDefault();
        }

        internal async Task<IEnumerable<Speaker>> GetSpeakersAsync(DateTime time = default)
        {
            string route = "Speakers";

            List<Speaker> speakers = await _memoryCache.GetOrCreateAsync(route, async entry =>
            {
                HttpResponseMessage response = await _client.GetAsync(route);

                if (response.IsSuccessStatusCode)
                {
                    //entry.SetOptions(new MemoryCacheEntryOptions() { } )
                    speakers = await response.Content.ReadFromJsonAsync<List<Speaker>>();

                    foreach (Speaker speaker in speakers)
                    {
                        var sessionIds = speaker.Sessions.Select(s => s.Id).ToList();
                        speaker.Sessions.Clear();

                        foreach (var sessionId in sessionIds)
                        {
                            speaker.Sessions.Add(await GetSessionByIdAsync(sessionId));
                        }
                    }
                    return speakers;
                }
                return null;
            });

            if (speakers is null)
            {
                _memoryCache.Remove(route);
            }

            return speakers;
        }

        internal async Task<IEnumerable<Session>> GetSessionsAsync(bool getSpeakers = true)
        {
            string route = "Sessions";
            IEnumerable<Session> sessions = await _memoryCache.GetOrCreateAsync(route, async entry =>
            {

                HttpResponseMessage response = await _client.GetAsync(route);

                if (response.IsSuccessStatusCode)
                {
                    var sessionsResponse = await response.Content.ReadFromJsonAsync<List<SessionResponse>>();
                    sessions = sessionsResponse.FirstOrDefault()?.Sessions;

                    if (getSpeakers)
                    {

                        foreach (Session session in sessions)
                        {
                            var speakerIds = session.Speakers.Select(s => s.Id).ToList();
                            session.Speakers.Clear();

                            foreach (var speakerId in speakerIds)
                            {
                                session.Speakers.Add(await GetSpeakerByIdAsync(speakerId));
                            }
                        }
                    }
                    return sessions;
                }
                return null;
            });

            if (sessions is null)
            {
                _memoryCache.Remove(route);
            }

            return sessions;

        }

        internal async Task<Speaker> GetSpeakerByIdAsync(string id)
        {
            Speaker speaker = null;
            string route = "Speakers";

            HttpResponseMessage response = await _client.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                List<Speaker> speakers = await response.Content.ReadFromJsonAsync<List<Speaker>>();
                speaker = speakers.FirstOrDefault(s => s.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            }
            return speaker;
        }

        internal async Task<Session> GetSessionByIdAsync(int id)
        {
            IEnumerable<Session> sessions = await GetSessionsAsync(false);
            Session session = sessions.FirstOrDefault(s => s.Id == id);

            return session;
        }
    }
}
