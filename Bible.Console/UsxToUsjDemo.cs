using Bible.Usx.Services;

namespace Bible.Console;

public static class UsxToUsjDemo
{
    public static async Task<string> ConvertAsync(Stream usxStream)
    {
        var converter = new UsxToUsjConverter();
        return await converter.ConvertUsxStreamToUsjJsonAsync(usxStream);
    }
}