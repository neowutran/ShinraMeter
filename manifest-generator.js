////////////////////
//  CONFIG        //
////////////////////

const forceUnixLineEndings = true //force \n instead of \r\n (default: true)

const removeOldDefs = false // remove undetected defs (default: false)

const addAutoUpdateDisableBool = true // add ("disableAutoUpdate": false) to module.json if missing (default: true)

////////////////////
//  LISTS         //
////////////////////

// ignore these files for making manifest
const IGNORED_FILES = [
    'manifest.json',
    'manifest-generator.js',
    'manifest-generator.bat',
    'manifest-generator.exe',
	'.gitignore',
    'node.exe',
    'README.md',
    'GongSolutions.WPF.DragDrop.pdb',
    'HtmlAgilityPack.pdb',
    'Newtonsoft.Json.pdb',
    'TCC.exe.manifest',
    "TCCupdater.exe",
    "TCCupdater.dll",
    "TCCupdater.exe.config",
    "TCCupdater.dll.config",
    "TCCupdater.pdb",
    "TCCupdater.deps.json",
    "TCCupdater.runtimeconfig.dev.json",
    "TCCupdater.runtimeconfig.json",
    
    ".runtimeconfig.dev.json"

]

// ignore files/folders which start with these characters: i.e. ".git" or "_old"
const IGNORED_CHARACTERS = [
    '.',
    '_'
]

// filetypes to force unix line ending if enabled
const FORCE_UNIX_FILE_TYPES = [
    '.txt',
    '.text',
    '.js',
    '.json',
    '.jsn',
    '.xml',
    '.md',
    '.htm',
    '.html',
    '.css',
    '.csv',
    '.php',
    '.cfg',
    '.ini',
    '.list',
    '.lst'
]

////////////////////
//  CODE          //
////////////////////

const crypto = require('crypto'),
    fs = require('fs'),
	path = require('path')

// set directory to launch argument or local directory
let directory = __dirname
if (process.argv[2]) {
    directory = path.join(__dirname, process.argv[2])
    // check if valid directory
    try {
        fs.readdirSync(directory, 'utf8')
    }
    catch (err) {
        console.log(`"${directory}" is not a valid folder.`)
        return
    }
}

// read existing module.json
let modulejson
if (addAutoUpdateDisableBool) {
    try {
        // sanitize input
        modulejson = require(path.join(directory, 'module.json'))
        if (modulejson && typeof modulejson === 'object') {
            if (modulejson.disableAutoUpdate === undefined) {
                let newModule = {disableAutoUpdate: false}
                Object.assign(newModule, modulejson)
                fs.writeFileSync(path.join(directory, 'module.json'), jsonify(newModule), 'utf8')
            }
        }
    }
    catch (error) {
        // make new module
        console.log(error);
        let newModule = {disableAutoUpdate: false}
        fs.writeFileSync(path.join(directory, 'module.json'), jsonify(newModule), 'utf8')
    }
}

// read existing manifest.json
let manifest
try {
    // sanitize input
    manifest = require(path.join(directory, 'manifest.json'))
    if (manifest && typeof manifest === 'object') {
        if (!manifest.files) manifest.files = {}
        if (removeOldDefs) delete manifest.defs
    }
    else {
        manifest = {
            files: {}
        }
    }
}
catch (error) {
    // make new manifest
    manifest = {
        files: {}
    }
}

// delete removed file entries
let checking = 0
for (let entry of Object.keys(manifest.files)) {
    // check if file exists
    checking  += 1
    fs.access(path.join(directory, entry), fs.constants.F_OK, (err) => {
        checking -= 1
        if (err) delete manifest.files[entry]
        checkProg()
        return
    })
}

let reading = 0
getFiles()

// get all files in folder and subfolder
function getFiles(relativePath = '', files) {
    let dir = path.join(directory, relativePath)
    if (!files) files = fs.readdirSync(dir, 'utf8')
    for (let file of files) {
        // if not ignored file or begins with ignored character
        if (!IGNORED_FILES.includes(file) && !IGNORED_CHARACTERS.includes(file[0])) {
            reading += 1
            fs.readdir(path.join(dir, file), 'utf8', (err, moreFiles) => {
                if (moreFiles) {
                    getFiles(path.join(relativePath, file), moreFiles)
                }
                else {
                    getHash(path.join(relativePath, file))
                }
                reading -= 1
                checkProg()
            })
        }
    }
}

// get sha256 hash
function getHash(file, type = 'sha256') {
    file = file.replace(/\\/g, '/')
    // force unix line endings
    if (forceUnixLineEndings) forceUnix(file)
    // get defs
    getDefs(file)
    if (manifest.files[file] && typeof manifest.files[file] === 'object') {
        manifest.files[file].hash = crypto.createHash(type).update(fs.readFileSync(path.join(directory, file))).digest('hex')
    }
    else {
        manifest.files[file] = crypto.createHash(type).update(fs.readFileSync(path.join(directory, file))).digest('hex')
    }
}

// get defs
function getDefs(file) {
    if (file.slice(-4).includes('.js')) {
        let data = fs.readFileSync(path.join(directory, file), 'utf8')
        // ignore comments
        data = data.replace(/\/\/.*\/\*(?!.*\*\/)/g,'') // ignore: // ... /* ...
        data = data.replace(/\/\*[^]*?\*\//gm,'') // ignore: /* ... */
        data = data.replace(/\/\/.*/g,'') // ignore: // ...
        let packets = data.match(/['"`][CS]_[A-Z_]+['"`],[ \t\n]*(\d+|['"`]raw['"`]|.+[ \t\n]*\d+[ \t\n]*[:][ \t\n]*\d+)/igm) //[ \t\n]*\d+[ \t\n]*[:][ \t\n]*\d+
        //console.log(packets)
        if (packets) {
            if (!Array.isArray(packets)) packets = [packets]
            for (let packet of packets) {
                // make defs object for manifest
                if (!manifest.defs) manifest.defs = {}
                // formatting
                packet = packet.replace(/['"` \t\n]/igm, '')
                packet = packet.split(',')
                // if 'raw' or (patchVer > X ? Y : Z)
                if (isNaN(packet[1])) {
                    packet[1] = packet[1].toLowerCase()
                    // get 'raw'
                    if (packet[1] == 'raw') packet[1] = ['raw']
                    // get Y and Z
                    else {
                        packet[1] = packet[1].match(/\d+[:]\d+/)[0]
                        if (packet[1]) {
                            packet[1] = packet[1].split(':')
                            for (let index in packet[1]) {
                                packet[1][index] = Number(packet[1][index])
                            }
                        }
                        else packet[1] = []
                    }
                }
                else packet[1] = [Number(packet[1])]
                for (let packetVer of packet[1]) {
                    // if in manifest
                    if (manifest.defs[packet[0]]) {
                        // add to list
                        if (Array.isArray(manifest.defs[packet[0]])) {
                            if (!manifest.defs[packet[0]].includes(packetVer)) {
                                manifest.defs[packet[0]].push(packetVer)
                                manifest.defs[packet[0]].sort((a,b)=>{return a-b})
                            }
                        }
                        // change to list
                        else if (['number', 'string'].includes(typeof manifest.defs[packet[0]]) && manifest.defs[packet[0]] != packetVer) {
                            manifest.defs[packet[0]] = [manifest.defs[packet[0]], packetVer]
                            manifest.defs[packet[0]].sort((a,b)=>{return a-b})
                        }
                    }
                    else {
                        manifest.defs[packet[0]] = packetVer
                    }
                }
            }
        }
    }
}

// force unix line endings
function forceUnix(file) {
    // check if read and writable
    for (let type of FORCE_UNIX_FILE_TYPES) {
        if (file.slice(-6).includes(type)) {
            try {
                let data = fs.readFileSync(path.join(directory, file), 'utf8')
                data = data.replace(/\r\n/g, '\n')
                fs.writeFileSync(path.join(directory, file), data, 'utf8')
            }
            catch (err) {
                //console.log(err)
                console.log('Cannot edit protected file: ' + file)
            }
            return
        }
    }
}

// alphabetize object keys
function alphabetizeObject(obj) {
    let keys = Object.keys(obj)
    keys.sort()
    let newObj = {}
    for (let key of keys) {
        newObj[key] = obj[key]
    }
    return newObj
}

// JSON.stringify but make lists single line
function jsonify(obj) {
    obj = JSON.stringify(obj, null, '\t')
    let lists = obj.match(/\[[^]+?\].*/igm)
    if (lists) for (let list of lists) {
        obj = obj.substring(0,obj.indexOf(list)) + list.replace(/[ \n\t]*/igm, '') + obj.substring(obj.indexOf(list) + list.length)
    }
    return obj
}

// check if process completed
function checkProg() {
    if (reading === 0 && checking === 0) {
        manifest.files = alphabetizeObject(manifest.files)
        if (manifest.defs) manifest.defs = alphabetizeObject(manifest.defs)
        fs.writeFileSync(path.join(directory, 'manifest.json'), jsonify(manifest), 'utf8')
        console.log('"manifest.json" generation complete.')
    }
}