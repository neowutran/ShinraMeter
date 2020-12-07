const { DataInterface } = require("./data-interface");
const { ControlInterface } = require("./control-interface");

class ShinraInterface
{
    constructor(mod)
    {
        this.controlInterface = new ControlInterface(mod);
        this.dataInterface = new DataInterface(mod);
    }

    destructor()
    {
        this.controlInterface.stop();
        this.dataInterface.stop();
    }
}

exports.ShinraInterface = ShinraInterface;