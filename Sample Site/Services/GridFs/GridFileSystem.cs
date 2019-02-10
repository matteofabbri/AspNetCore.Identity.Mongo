using System;
using System.IO;
using System.Security.Claims;
using System.Threading.Tasks;
using SampleSite.Mongo;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace SampleSite.GridFs
{
    public class GridFileSystem : IGridFileSystem
    {
        private readonly GridFSBucket _bucket;
        private readonly IMongoCollection<GridFile> _files;

        public GridFileSystem(string connectionString, string collectionName)
        {
            _files = MongoUtil.FromConnectionString<GridFile>(connectionString, collectionName);
            _bucket = new GridFSBucket(_files.Database);
        }

        public async Task<string> Upload(string fileName, Stream source)
        {
            var gf = new GridFile
            {
                Acl = new Acl(),
                FileName = fileName,
                MimeType = MimeMapping.MimeUtility.GetMimeMapping(fileName),
                LastModified = DateTimeOffset.Now,
            };

            await _files.InsertOneAsync(gf);
            gf.GridName = $"{gf.Id}-{gf.FileName}";
            await _files.ReplaceOneAsync(x=>x.Id == gf.Id, gf);

            _bucket.UploadFromStream(gf.GridName, source);

            return gf.GridName;
        }

        public async Task<bool> IsAllowed(string gridName, ClaimsPrincipal user)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);
            return gf?.Acl?.IsAllowed(user) ?? false;
        }

        public async Task<GridFile> GetMetadata(string gridName)
        {
            return await _files.FirstOrDefaultAsync(x => x.GridName == gridName);
        }

        public async Task SetOwner(string gridName, string owner)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.Owner = owner;

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task MakePublic(string gridName)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);
            
            if(gf == null) return;

            gf.Acl.Public = true;

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task MakeNotPublic(string gridName)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.Public = false;

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task AllowUser(string gridName, string user)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.AllowUsers.Add(user);
            gf.Acl.DenyUsers.Remove(user);

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task DenyUser(string gridName, string user)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.DenyUsers.Add(user);
            gf.Acl.AllowUsers.Remove(user);

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task AllowRole(string gridName, string role)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.AllowRoles.Add(role);
            gf.Acl.DenyRoles.Remove(role);

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public async Task DenyRole(string gridName, string role)
        {
            var gf = await _files.FirstOrDefaultAsync(x => x.GridName == gridName);

            if (gf == null) return;

            gf.Acl.DenyRoles.Add(role);
            gf.Acl.AllowRoles.Remove(role);

            await _files.ReplaceOneAsync(x => x.Id == gf.Id, gf);
        }

        public Task<Stream> Download(string gridName)
        {
            return Task.FromResult(_bucket.OpenDownloadStreamByName(gridName) as Stream);
        }
    }
}
