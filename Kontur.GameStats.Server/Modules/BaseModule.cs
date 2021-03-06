﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HttpServerCore;
using NLog;

namespace Kontur.GameStats.Server.Modules
{
    public class InvalidQueryException : Exception
    {
        public InvalidQueryException(string message, Exception innerException = null) :
            base(message, innerException)
        {
        }
    }
    public abstract class BaseModule : IServerModule
    {
        protected abstract Logger Logger { get; }

        private List<RequestFilter> filters { get; set; }
        protected abstract IEnumerable<RequestFilter> Filters { get; }

        public async Task<IRequest> ProcessRequest(IRequest request)
        {
            Logger.ConditionalTrace("Get request: {0}", request);
            if (filters == null)
                filters = Filters.ToList();
            try
            {
                return await filters.Aggregate(Task.FromResult(request),
                    async (task, filter) => await filter.FilterRequest(await task));
            }
            catch (InvalidQueryException e)
            {
                Logger.Info("Invalid query: {0}", e);
            }
            catch (Exception e)
            {
                Logger.Error(e, "Unhandled module exception");
            }
            return await Task.FromResult(request);
        }
    }
}
