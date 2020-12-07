const { RpcServer } = require("./rpc-server");
// TODO: move these to settings ------
const listenAddress = '127.0.0.61';
const listenPort = 5300;
// -----------------------------------

class ControlInterface
{
    constructor(mod)
    {
        this.mod = mod;
        this.server = new RpcServer(this.mod, listenAddress, listenPort);
        this.server.start();
    }

    destructor()
    {
        this.server.stop();
    }
}

exports.ControlInterface = ControlInterface;
