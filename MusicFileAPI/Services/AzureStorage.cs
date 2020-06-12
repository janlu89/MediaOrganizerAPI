using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using MusicFileAPI.Interfaces;
using MusicFileAPI.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MusicFileAPI.Services
{
    public class AzureStorage : ICloudStorage
    {
        private readonly IStorageConnectionFactory _storageConnectionFactory;

        public AzureStorage(IStorageConnectionFactory storageConnectionFactory)
        {
            _storageConnectionFactory = storageConnectionFactory;
        }

        public async Task<List<MusicStream>> GetAll()
        {
            List<MusicStream> allBlobs = new List<MusicStream>();
            var blobContainer = await _storageConnectionFactory.GetContainer();
            foreach (CloudBlockBlob item in blobContainer.ListBlobs(null, true, BlobListingDetails.Metadata))
            {
                allBlobs.Add(new MusicStream
                {
                    uri = item.Uri,
                    artist = item.Metadata.FirstOrDefault(x => x.Key == "artist").Value,
                    title = item.Metadata.FirstOrDefault(x => x.Key == "title").Value
                });
            }
            return allBlobs;
        }

        public async Task UploadAsync(UploadMusicFileRequest request)
        {
            var blobContainer = await _storageConnectionFactory.GetContainer();
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(GetRandomBlobName(request.musicFile.FileName));
            using (var stream = request.musicFile.OpenReadStream())
            {
                blob.Metadata.Add("title", request.title);
                blob.Metadata.Add("artist", request.artist);
                await blob.UploadFromStreamAsync(stream);
            }
        }

        public async Task EditMusicInfo(MusicStream musicStream)
        {
            var blobContainer = await _storageConnectionFactory.GetContainer();
            string filename = Path.GetFileName(musicStream.uri.LocalPath);

            var blob = blobContainer.GetBlockBlobReference(filename);
            //
            var metadataKeys = new string[] { "title", "artist" };
            foreach (string key in metadataKeys)
            {
                if (blob.Metadata.ContainsKey(key))
                {
                    blob.Metadata[key] = key == "title" ? musicStream.title : musicStream.artist;
                }
                else
                {
                    blob.Metadata.Add(key, key == "title" ? musicStream.title : musicStream.artist);
                }
            }
            //
            blob.SetMetadata();

        }

        public async Task DeleteFile(string fileName)
        {
            Uri uri = new Uri(fileName);
            string filename = Path.GetFileName(uri.LocalPath);

            var blobContainer = await _storageConnectionFactory.GetContainer();
            var blob = blobContainer.GetBlockBlobReference(filename);
            await blob.DeleteIfExistsAsync();
        }

        public async Task DeleteAll()
        {
            var blobContainer = await _storageConnectionFactory.GetContainer();
            foreach (var blob in blobContainer.ListBlobs())
            {
                if (blob.GetType() == typeof(CloudBlockBlob))
                {
                    await ((CloudBlockBlob)blob).DeleteIfExistsAsync();
                }
            }
        }

        /// <summary> 
        /// string GetRandomBlobName(string filename): Generates a unique random file name to be uploaded  
        /// </summary> 
        private string GetRandomBlobName(string filename)
        {
            string ext = Path.GetExtension(filename);
            return string.Format("{0:10}_{1}{2}", DateTime.Now.Ticks, Guid.NewGuid(), ext);
        }
    }

}