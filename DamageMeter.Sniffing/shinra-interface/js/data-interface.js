const net = require('net');
// TODO: move these to settings ------
const address = '127.0.0.60';
const port = 5300;
// -----------------------------------

class DataInterface
{
    installedHooks = 0;
    installRawHook(opcode)
    {
        let options = { order: -Infinity };
        this.installedHooks++;
        try
        {
            this.mod.hook(opcode, 'raw', options, (code, data) =>
            {
                this.send(this.build(data));
            });

        } catch (err)
        {
            //this.mod.log('Failed to install hook for ' + opcode + ' ' + err);
        }
    }
    removeRawHook(opcode)
    {
        let options = { order: -Infinity };

        this.mod.unhook(opcode, 'raw', options, (code, data) =>
        {
            this.send(this.build(data));
        })
    }
    installHooks(mod)
    {
        let opcodes = ['C_CHECK_VERSION', 'C_LOGIN_ARBITER'];
        opcodes.forEach(o =>
        {
            this.installedHooks++;
            mod.hook(o, 'raw', { order: -Infinity }, (code, data) =>
            {
                this.send(this.build(data));
            })
        });
    }
    printInfo()
    {
        this.mod.log('Installed hooks: ' + this.installedHooks);
    }


    constructor(mod)
    {
        this.mod = mod;
        mod.command.add('shinra', (cmd, arg) =>
        {
            switch (cmd)
            {
                case 'add':
                    this.installRawHook(arg);
                    break;
                case 'rem':
                    this.removeRawHook(arg);
                    break;
                case 'print':
                    this.printInfo();
                    break;
            }
        });
        this.interface = new net.Socket();
        this.interface.connect(port, address);
        this.interface.on('error', (err) =>
        {
            this.interface.end();
            this.mod.error("[shinra-interface] " + err);
        });
        this.interface.on('connect', () => { this.mod.log("[shinra-interface] Connected!") });

        this.installHooks(mod);
    }

    build(payload)
    {
        return Buffer.from(payload);
    }

    send(data)
    {
        if (!this.interface.writable) return;
        this.interface.write(this.build(data));
    }

    stop()
    {
        this.interface.end();
        this.interface.destroy();
    }
}

exports.DataInterface = DataInterface;
