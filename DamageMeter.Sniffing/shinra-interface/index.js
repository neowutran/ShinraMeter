const { spawn } = require('child_process');
const Path = require('path');

class ShinraMeter {
    constructor(m) {
        const meterPath = Path.join(__dirname, 'ShinraMeter.exe');

        m.clientInterface.once('ready', () => {
            m.log('Starting Shinra Meter...');
            const shinra = spawn(meterPath, ['--toolbox'], { stdio: 'ignore' });
            shinra.on('exit', () => m.log('Shinra exited because it closed or it is already running.'));
        });
    }
}
exports.ClientMod = ShinraMeter;
