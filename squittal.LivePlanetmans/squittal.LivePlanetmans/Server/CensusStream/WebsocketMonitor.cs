using DaybreakGames.Census.Stream;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusStream
{
    public class WebsocketMonitor : IWebsocketMonitor, IDisposable
    {
        private readonly ICensusStreamClient _client;
        private readonly IWebsocketEventHandler _handler;
        private readonly ILogger<WebsocketMonitor> _logger;

        public WebsocketMonitor(ICensusStreamClient censusStreamClient, IWebsocketEventHandler handler, ILogger<WebsocketMonitor> logger)
        {
            _client = censusStreamClient;
            _handler = handler;
            _logger = logger;

            var subscription = new CensusStreamSubscription
            {
                Characters = new[] { "all" },
                Worlds = new[] { "all" },
                EventNames = new[] { "Death", "PlayerLogin", "PlayerLogout" }
            };

            _client.Subscribe(subscription)
                .OnMessage(OnMessage)
                .OnDisconnect(OnDisconnect);
        }

        private async Task OnMessage(string message)
        {
            if (message == null)
            {
                return;
            }

            JToken jMsg;

            try
            {
                jMsg = JToken.Parse(message);
            }
            catch (Exception)
            {
                _logger.LogError(91097, "Failed to parse message: {0}", message);
                return;
            }

            if (jMsg.Value<string>("type") == "heartbeat")
            {
                return;
            }

            await _handler.Process(jMsg);
        }

        public Task OnApplicationShutdown(CancellationToken cancellationToken)
        {
            return _client.DisconnectAsync();
        }

        public Task OnApplicationStartup(CancellationToken cancellationToken)
        {
            return _client.ConnectAsync();
        }

        private Task OnDisconnect(string error)
        {
            _logger.LogInformation("Websocket Client Disconnected!");

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
