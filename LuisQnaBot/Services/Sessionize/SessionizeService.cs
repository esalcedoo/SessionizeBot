using LuisQnaBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LuisQnaBot.Services.Sessionize
{
    public class SessionizeService
    {
        private readonly HttpClient _client;

        public SessionizeService(HttpClient client)
        {
            _client = client;
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

        public async Task<IEnumerable<Speaker>> WhoIsSHeAsync(string name = default, DateTime dateTime = default, string track = default)
        {
            IEnumerable<Speaker> speakers = await GetSpeakersAsync();

            if (!string.IsNullOrEmpty(name))
                speakers = speakers
                       .Where(speaker => speaker.FullName.Contains(name,StringComparison.InvariantCultureIgnoreCase));

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
            List<Speaker> speakers = new List<Speaker>();
            string route = "Speakers";

            HttpResponseMessage response = await _client.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                speakers = await response.Content.ReadAsAsync<List<Speaker>>();
                foreach (Speaker speaker in speakers)
                {

                    var sessionIds = speaker.Sessions.Select(s => s.Id).ToList();
                    speaker.Sessions.Clear();

                    foreach (var sessionId in sessionIds)
                    {
                        speaker.Sessions.Add(await GetSessionByIdAsync(sessionId));
                    }
                }
            }
            return speakers;
        }

        internal async Task<IEnumerable<Session>> GetSessionsAsync()
        {
            IEnumerable<Session> sessions = new List<Session>();
            string route = "Sessions";

            HttpResponseMessage response = await _client.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                var sessionsResponse = await response.Content.ReadAsAsync<List<SessionResponse>>();
                sessions = sessionsResponse.FirstOrDefault()?.Sessions;
                foreach (Session session in sessions)
                {

                    var speakerIds = session.Speakers.Select(s => s.Id);
                    session.Speakers.Clear();

                    foreach (var speakerId in speakerIds)
                    {
                        session.Speakers.Add(await GetSpeakerByIdAsync(speakerId));
                    }
                }
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
                List<Speaker> speakers = await response.Content.ReadAsAsync<List<Speaker>>();
                speaker = speakers.FirstOrDefault(s => s.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            }
            return speaker;
        }

        internal async Task<Session> GetSessionByIdAsync(int id)
        {
            Session session = null;
            string route = "Sessions";

            HttpResponseMessage response = await _client.GetAsync(route);

            if (response.IsSuccessStatusCode)
            {
                List<Session> sessions = await response.Content.ReadAsAsync<List<Session>>();
                session = sessions.FirstOrDefault(s => s.Id == id);
            }
            return session;
        }
    }
}
