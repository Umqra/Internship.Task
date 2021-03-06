using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;

namespace DataCore
{
    public class ServerInfo
    {
        public class ServerInfoId : IComparable<ServerInfoId>
        {
            public string Id { get; set; }
            public int CompareTo(ServerInfoId other)
            {
                return String.Compare(Id, other.Id, StringComparison.Ordinal);
            }
        }

        public ServerInfoId GetIndex() => new ServerInfoId {Id = Id};

        [JsonIgnore]
        public string Id { get; set; }

        public string Name { get; set; }
        public IList<string> GameModes { get; set; }

        private bool Equals(ServerInfo other)
        {
            return string.Equals(Id, other.Id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServerInfo)obj);
        }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }
    }
}