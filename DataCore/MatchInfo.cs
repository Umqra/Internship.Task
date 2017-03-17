using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DataCore
{
    public class MatchInfo
    {
        public string Id { get; set; }
        public ServerInfo HostServer { get; set; }
        public string Map { get; set; }
        public string GameMode { get; set; }
        public int FragLimit { get; set; }
        public int TimeLimit { get; set; }
        public double ElapsedTime { get; set; }
        public DateTime EndTime { get; set; }
        public IList<PlayerInfo> Scoreboard { get; set; }

        public MatchInfo() { }

        public virtual MatchInfo InitPlayers(DateTime endTime)
        {
            EndTime = endTime;
            for (int i = 0; i < Scoreboard.Count; i++)
            {
                Scoreboard[i].BaseMatch = this;
                Scoreboard[i].AreWinner = i == 0;
                Scoreboard[i].ScoreboardPercent = (Scoreboard.Count == 1 ? 100 : 100.0 * i / (Scoreboard.Count - 1));
            }
            return this;
        }

        private bool Equals(MatchInfo other)
        {
            return Equals(HostServer.Id, other.HostServer.Id) && EndTime.Equals(other.EndTime);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((MatchInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = HostServer?.Id.GetHashCode() ?? 0;
                hashCode = (hashCode * 397) ^ EndTime.GetHashCode();
                return hashCode;
            }
        }
    }
}