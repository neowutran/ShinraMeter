const { Helpers } = require("./helpers");
const { RpcHandler } = require("./rpc-handler");
const http = require('http');

class RpcServer
{
    constructor(mod, address, port)
    {
        this.handler = new RpcHandler(mod);
        this.address = address;
        this.port = port;
        this.server = http.createServer((req, res) =>
        {
            if (req.method !== 'POST') return;
            let body = '';
            req.on('data', chunk => body += chunk.toString());
            req.on('error', err =>
            {
                if (err.errno === 'ECONNREFUSED')
                {
                    console.log(`[control-interface] Failed to send data (error:${err.errno}).`);
                }
                else
                {
                    console.log(`[control-interface] Failed to send data(error:${err.errno}).`)
                }
            });
            req.on('end', () =>
            {
                let rpcRequest = JSON.parse(body);
                var rpcResult = null;
                var respType = 'result';
                try
                {
                    rpcResult = this.handler.handle(rpcRequest);
                }
                catch (error)
                {
                    respType = 'error';
                    rpcResult = {
                        'code': -1,
                        'message': error.toString()
                    };
                }
                let jsonResponse = Helpers.buildResponse(rpcResult, rpcRequest.id, respType);
                let stringResponse = JSON.stringify(jsonResponse);
                //console.log(`\n------REQUEST------\n\t${JSON.stringify(rpcRequest)}\n------RESPONSE------\n\t${stringResponse}`);
                res.writeHead(200, Helpers.buildHeaders(stringResponse.length));
                res.write(stringResponse);
                res.end();
            });
        });
        this.server.on('error', err =>
        {
            if (err.errno == 'EADDRINUSE')
            {
                console.log(`[control-interface] Cannot connect (${err}). This might be caused by multi-clienting.`);
            }
            else if (err.errno == 'ECONNREFUSED')
            {
                console.log(`[control-interface] Cannot connect (${err}).`);
            }
        });

    }

    start()
    {
        this.server.listen(this.port, this.address, () => { });
    }
    stop()
    {
        this.server.removeAllListeners();
        this.server.close();
    }
}
exports.RpcServer = RpcServer;
