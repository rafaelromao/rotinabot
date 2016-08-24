using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Lime.Protocol;
using Takenet.MessagingHub.Client.Extensions.Bucket;

namespace RotinaBot.Tests.AcceptanceTests.Mocks
{
    internal class FakeBucketExtension : IBucketExtension
    {
        private Dictionary<string, Document> Bucket = new Dictionary<string, Document>();

        public void Clear()
        {
            Bucket = new Dictionary<string, Document>();
        }

        public Task<T> GetAsync<T>(string id, CancellationToken cancellationToken = new CancellationToken()) where T : Document
        {
            if (!Bucket.ContainsKey(id))
                return Task.FromResult<T>(null);

            return ((T)Bucket[id]).AsCompletedTask();
        }

        public Task SetAsync<T>(string id, T document, TimeSpan expiration = new TimeSpan(),
            CancellationToken cancellationToken = new CancellationToken()) where T : Document
        {
            Bucket[id] = document;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(string id, CancellationToken cancellationToken = new CancellationToken())
        {
            Bucket.Remove(id);
            return Task.CompletedTask;
        }
    }
}