using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SampleSite.GridFs
{
    public interface IGridFileSystem
    {
        Task<string> Upload(string fileName, Stream source);
        Task<Stream> Download(string gridName);

        Task<bool> IsAllowed(string gridName, ClaimsPrincipal user);

        Task SetOwner(string gridName, string owner);

        Task<GridFile> GetMetadata(string gridName);

        Task MakePublic(string gridName);
        Task MakeNotPublic(string gridName);

        Task AllowUser(string gridName, string user);
        Task DenyUser(string gridName, string user);

        Task AllowRole(string gridName, string role);
        Task DenyRole(string gridName, string role);
    }
}