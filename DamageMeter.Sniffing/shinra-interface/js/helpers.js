class Helpers
{
    static buildHeaders(length)
    {
        return {
            'Content-Type': 'application/json',
            'Content-Length': length,
            'User-Agent': 'shinrameter',
            'Connection': 'Keep-Alive',
            'Keep-Alive': 'timeout=1, max=100'
        };
    }
    static buildResponse(ret, id, type)
    {
        return {
            'jsonrpc': '2.0',
            [type]: ret,
            'id': id
        };
    }
}
exports.Helpers = Helpers;
