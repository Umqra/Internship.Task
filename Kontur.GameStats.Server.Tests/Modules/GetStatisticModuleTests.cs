﻿using System.Net;
using System.Threading.Tasks;
using DataCore;
using FluentAssertions;
using HttpServerCore;
using Kontur.GameStats.Server.Modules;
using NUnit.Framework;

namespace Kontur.GameStats.Server.Tests.Modules
{
    [TestFixture]
    public class GetStatisticModuleTests : BaseModuleTests
    {
        [SetUp]
        public override void Setup()
        {
            base.Setup();
            Module = new GetStatisticModule(StatisticStorage);
        }

        [Test]
        public async Task ModuleReturnsEmptyArray_WhenNoServersRegistred()
        {
            var response = await Module.ProcessRequest(CreateRequest("", "/servers/info"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, new ServerInfo[] {}));
        }

        [Test]
        public async Task ModuleReturnsAllServerInfos()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateServer(Server2.GetIndex(), Server2);

            var response = await Module.ProcessRequest(CreateRequest("", "/servers/info"));

            response.Response.ShouldBeEquivalentTo(new JsonHttpResponse(HttpStatusCode.OK, new[]
            {
                new
                {
                    endpoint = Server1.Id,
                    info = Server1
                },
                new
                {
                    endpoint = Server2.Id,
                    info = Server2
                }
            }));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenServerNotRegistred()
        {
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/info"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsServerInfo()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);

            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/info"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Server1));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoMatchesFound()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);

            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Server1.Id}/matches/{DateTime1.ToUtcFormat()}"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsNotFound_WhenNoServersFound()
        {
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Host1}/matches/{DateTime1.ToUtcFormat()}"));

            response.Response.Should().Be(new HttpResponse(HttpStatusCode.NotFound));
        }

        [Test]
        public async Task ModuleReturnsMatchInfo()
        {
            await StatisticStorage.UpdateServer(Server1.GetIndex(), Server1);
            await StatisticStorage.UpdateMatch(Match1.GetIndex(), Match1.InitPlayers(Match1.EndTime));
            
            var response = await Module.ProcessRequest(CreateRequest("", $"/servers/{Match1.HostServer.Id}/matches/{Match1.EndTime.ToUtcFormat()}"));

            response.Response.Should().Be(new JsonHttpResponse(HttpStatusCode.OK, Match1));
        }
    }
}
