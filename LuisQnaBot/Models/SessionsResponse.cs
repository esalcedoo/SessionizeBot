using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LuisQnaBot.Models
{
    public class SessionResponse
    {
        public object GroupId { get; set; }
        public string GroupName { get; set; }
        public List<Session> Sessions { get; set; }
    }
}
