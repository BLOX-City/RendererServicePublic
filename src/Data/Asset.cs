using System.Security.Cryptography;
using System.Text;

public class Asset
{
    private static HttpClient _httpClient = new HttpClient();
    public static async Task<string> Load(Uri uri)
    {
        bool isFile = uri.Scheme == "file";

        if (isFile)
        {
            return uri.AbsolutePath.Substring(1);
        }


        string hashedPath = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(uri.ToString())));

        string cacheDirHashPath = $"cache/{hashedPath}";

        bool doesExist = File.Exists(cacheDirHashPath);

        if (!doesExist)
        {
            await File.WriteAllBytesAsync(cacheDirHashPath, await _httpClient.GetByteArrayAsync(uri));

            return cacheDirHashPath;

        }

        return cacheDirHashPath;
    }
}