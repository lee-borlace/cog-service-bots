using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.Documents.Linq;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Watcher.Config;
using Watcher.Model;

namespace Watcher.Data
{
    /// <summary>
    /// Data repo for the storage of observations.
    /// </summary>
    public class CosmosDataRepo
    {
        protected DocumentClient _cosmosClient;
        private WatcherConfig _config;

        public CosmosDataRepo(IOptions<WatcherConfig> options)
        {
            _config = options.Value;

            _cosmosClient = new DocumentClient(new Uri(_config.CosmosConfig.Endpoint), _config.CosmosConfig.Key);

            CreateDatabaseIfNotExistsAsync().Wait();
            CreateCollectionIfNotExistsAsync().Wait();
        }

        /// <summary>
        /// Inserts an observation.
        /// </summary>
        /// <param name="observation">The observation.</param>
        /// <returns></returns>
        public async Task InsertObservation(Observation observation)
        {
            await _cosmosClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(_config.CosmosConfig.DatabaseId, _config.CosmosConfig.CollectionId), observation);
        }

        /// <summary>
        /// Gets observations matching predicate.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task<IEnumerable<Observation>> GetObservations(Expression<Func<Observation, bool>> predicate)
        {
            return await GetItemsAsync(predicate);
        }

        /// <summary>
        /// Gets items of given type based on predicate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="predicate">The predicate.</param>
        /// <returns></returns>
        public async Task<IEnumerable<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate)
        {
            IDocumentQuery<T> query = _cosmosClient.CreateDocumentQuery<T>(
                UriFactory.CreateDocumentCollectionUri(_config.CosmosConfig.DatabaseId, _config.CosmosConfig.CollectionId),
                new FeedOptions { MaxItemCount = -1 })
                .Where(predicate)
                .AsDocumentQuery();

            List<T> results = new List<T>();
            while (query.HasMoreResults)
            {
                results.AddRange(await query.ExecuteNextAsync<T>());
            }

            return results;
        }

        private async Task CreateDatabaseIfNotExistsAsync()
        {
            try
            {
                await _cosmosClient.ReadDatabaseAsync(UriFactory.CreateDatabaseUri(_config.CosmosConfig.DatabaseId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _cosmosClient.CreateDatabaseAsync(new Database { Id = _config.CosmosConfig.DatabaseId });
                }
                else
                {
                    throw;
                }
            }
        }

        private async Task CreateCollectionIfNotExistsAsync()
        {
            try
            {
                await _cosmosClient.ReadDocumentCollectionAsync(UriFactory.CreateDocumentCollectionUri(_config.CosmosConfig.DatabaseId, _config.CosmosConfig.CollectionId));
            }
            catch (DocumentClientException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await _cosmosClient.CreateDocumentCollectionAsync(
                        UriFactory.CreateDatabaseUri(_config.CosmosConfig.DatabaseId),
                        new DocumentCollection { Id = _config.CosmosConfig.CollectionId },
                        new RequestOptions { OfferThroughput = 1000 });
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
