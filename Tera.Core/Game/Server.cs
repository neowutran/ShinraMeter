// Copyright (c) Gothos
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tera.Game
{
    public class Server
    {
        public string Ip { get; private set; }
        public string Name { get; private set; }
        public string Region { get; private set; }

        public Server(string name, string region, string ip)
        {
            Ip = ip;
            Name = name;
            Region = region;
        }
    }
}
