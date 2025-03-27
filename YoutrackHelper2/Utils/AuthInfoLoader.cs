using System;
using System.IO;
using System.Threading.Tasks;

namespace YoutrackHelper2.Utils
{
    public class AuthInfoLoader
    {
        public static async Task<(string Uri, string Perm)> GetAuthInfoAsync()
        {
            (string Uri, string Perm) authInfo = (string.Empty, string.Empty);

            authInfo.Uri = (await File.ReadAllTextAsync(
                    $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\uri.txt"))
            .Replace("\n", string.Empty);

            authInfo.Perm = (await File.ReadAllTextAsync(
                    $@"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\youtrackInfo\perm.txt"))
            .Replace("\n", string.Empty);

            return authInfo;
        }
    }
}