﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HttpServerCore;
using Kontur.GameStats.Server.Storage;
using NLog;

namespace Kontur.GameStats.Server.Modules
{
    public class ReportsModule : BaseModule
    {
        private Logger logger;
        protected override Logger Logger => logger ?? (logger = LogManager.GetCurrentClassLogger());

        private readonly IReportStorage reportStorage;
        private readonly IGlobalServerStatisticStorage globalStatisticStorage;
        private readonly int DefaultCountParameter = 5;
        private int MinCountParameter = 0;
        private int MaxCountParameter = 50;
        
        public ReportsModule(IReportStorage reportStorage, IGlobalServerStatisticStorage globalStatisticStorage)
        {
            this.reportStorage = reportStorage;
            this.globalStatisticStorage = globalStatisticStorage;
        }

        protected override IEnumerable<RequestFilter> Filters => new[]
        {
            new RequestFilter(HttpMethodEnum.Get, 
                new Regex(@"^/reports/recent-matches(/(?<count>.+)?)?$", RegexOptions.Compiled), 
                HandleRecentMatchesQuery), 
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex(@"^/reports/best-players(/(?<count>.+)?)?$", RegexOptions.Compiled), 
                HandleBestPlayersQuery), 
            new RequestFilter(
                HttpMethodEnum.Get, 
                new Regex(@"^/reports/popular-servers(/(?<count>.+)?)?$", RegexOptions.Compiled), 
                HandlePopularServersQuery), 
        };

        private int ParseIntegerGroupOrDefault(Group matchGroup, int defaultValue)
        {
            var result = defaultValue;
            if (matchGroup.Success)
                int.TryParse(matchGroup.Value, out result);
            return result;
        }

        private int FitInBound(int value, int lowerBound, int upperBound)
        {
            if (value < lowerBound)
                return lowerBound;
            if (value > upperBound)
                return upperBound;
            return value;
        }

        private int FilterCountParameter(Group group)
        {
            return FitInBound(
                ParseIntegerGroupOrDefault(group, DefaultCountParameter), 
                MinCountParameter,
                MaxCountParameter);
        }

        private Task<IResponse> HandlePopularServersQuery(IRequest request, Match match)
        {
            var serversCount = FilterCountParameter(match.Groups["count"]);
            return GetPopularServers(serversCount);
        }

        private Task<IResponse> GetPopularServers(int serversCount)
        {
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, reportStorage.PopularServers(serversCount).Select(server => new
            {
                endpoint = server.Server.Id,
                name = server.Server.Name,
                averageMatchesPerDay = server.AverageMatchesPerDay(globalStatisticStorage)
            }));
            return Task.FromResult(response);
        }

        private Task<IResponse> HandleBestPlayersQuery(IRequest request, Match match)
        {
            var playersCount = FilterCountParameter(match.Groups["count"]);
            return GetBestPlayers(playersCount);
        }

        private Task<IResponse> GetBestPlayers(int playersCount)
        {
            var bestPlayers = reportStorage.BestPlayers(playersCount).Select(player => new
            {
                name = player.Player.Name,
                killToDeathRatio = player.KillToDeathRatio
            });
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, bestPlayers);
            return Task.FromResult(response);
        }

        private Task<IResponse> HandleRecentMatchesQuery(IRequest request, Match match)
        {
            var matchCount = FilterCountParameter(match.Groups["count"]);
            return GetRecentMatches(matchCount);
        }

        private Task<IResponse> GetRecentMatches(int matchCount)
        {
            var recentMatches = reportStorage.RecentMatches(matchCount).Select(match => new
            {
                server = match.HostServer.Id,
                timestamp = match.EndTime,
                results = match
            });
            IResponse response = new JsonHttpResponse(HttpStatusCode.OK, recentMatches);
            return Task.FromResult(response);
        }
    }
}
