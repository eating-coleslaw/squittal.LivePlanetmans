﻿using System;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;

namespace squittal.LivePlanetmans.Server.CensusStream
{
    public interface IWebsocketEventHandler : IDisposable
    {
        Task Process(JToken jPayload);
    }
}