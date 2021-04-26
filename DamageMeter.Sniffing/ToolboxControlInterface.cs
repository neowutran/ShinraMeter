using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Tera.Toolbox;

namespace DamageMeter.Sniffing
{
    public class ToolboxControlInterface
    {
        private readonly ToolboxHttpClient _client;

        public ToolboxControlInterface(string address)
        {
            _client = new ToolboxHttpClient(address);
        }

        /// <summary>
        /// Requests <c>ttb-interface-control</c> to return current server id.
        /// </summary>
        /// <returns>server id</returns>
        public async Task<uint> GetServerId()
        {
            var resp = await _client.CallAsync("getServer");
            return resp?.Result?.Value<uint>() ?? 0;
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to return current release version.
        /// </summary>
        /// <returns>release version</returns>
        public async Task<int> GetReleaseVersion()
        {
            var resp = await _client.CallAsync("getReleaseVersion");
            return resp?.Result?.Value<int>() ?? 0;
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to dump a map to file.
        /// </summary>
        /// <param name="path">path the map will be dumped to</param>
        /// <param name="mapType">type of the map:
        ///     <list type="bullet">
        ///         
        ///         <item>
        ///             <term>"protocol"</term>
        ///             <description>opcode map</description>
        ///         </item>
        ///         <item>
        ///             <term>"sysmsg"</term>
        ///             <description>system messages map</description>
        ///         </item>
        ///     </list>
        /// </param>
        /// <returns>true if successful</returns>
        public async Task<bool> DumpMap(string path, string mapType)
        {
            var resp = await _client.CallAsync("dumpMap", new JObject
            {
                { "path", path},
                { "mapType", mapType }
            });
            return resp?.Result != null && resp.Result.Value<bool>();
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to install hooks for the provided list of opcodes.
        /// </summary>
        /// <param name="opcodes">list of opcode names</param>
        /// <returns>true if successful</returns>
        public async Task<bool> AddHooks(List<string> opcodes)
        {
            var jArray = new JArray();
            opcodes.ForEach(opc => jArray.Add(opc));
            var resp = await _client.CallAsync("addHooks", new JObject
            {
                {"hooks", jArray}
            });
            return resp?.Result != null && resp.Result.Value<bool>();
        }
        public async Task<uint> GetToolboxPID()
        {
            var resp = await _client.CallAsync("getToolboxPID");
            return resp?.Result?.Value<uint>() ?? 0;
        }

        public async Task<bool> AddDefinition(string opcodeName, uint version, string def)
        {
            var resp = await _client.CallAsync("addDefinition", new JObject
            {
                {"opcodeName", opcodeName},
                {"version", version},
                {"def", def}
            });
            return resp?.Result != null && resp.Result.Value<bool>();
        }
        public async Task<bool> AddOpcode(string opcodeName, ushort opcode)
        {
            var resp = await _client.CallAsync("addOpcode", new JObject
            {
                {"opcodeName", opcodeName},
                {"opcode", opcode}
            });
            return resp?.Result != null && resp.Result.Value<bool>();
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to uninstall hooks for the provided list of opcodes.
        /// </summary>
        /// <param name="opcodes">list of opcode names</param>
        /// <returns>true if successful</returns>
        public async Task<bool> RemoveHooks(List<string> opcodes)
        {
            var jArray = new JArray();
            opcodes.ForEach(opc => jArray.Add(opc));
            var resp = await _client.CallAsync("removeHooks", new JObject
            {
                {"hooks", jArray}
            });
            return resp?.Result != null && resp.Result.Value<bool>();
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to perform a DC query.
        /// </summary>
        /// <param name="query">query string</param>
        /// <returns>json object containing query result</returns>
        public async Task<JObject?> Query(string query)
        {
            var resp = await _client.CallAsync("query", new JObject
            {
                { "query" , query }
            });

            return resp?.Result?.Value<JObject>();
        }
        /// <summary>
        /// Requests <c>ttb-interface-control</c> to return current protocol version.
        /// </summary>
        /// <returns>protocol version</returns>
        public async Task<uint> GetProtocolVersion()
        {
            var resp = await _client.CallAsync("getProtocolVersion");
            return resp?.Result?.Value<uint>() ?? 0;
        }

    }
}