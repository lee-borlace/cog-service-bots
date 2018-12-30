// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq;
using ISpyBot.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Integration;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Serialization;

namespace ISpyBot
{
    /// <summary>
    /// The Startup class configures services and the request pipeline.
    /// </summary>
    public class Startup
    {
        private ILoggerFactory _loggerFactory;
        private readonly bool _isProduction;

        public Startup(IHostingEnvironment env)
        {
            _isProduction = env.IsProduction();
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            if (env.IsDevelopment())
            {
                builder.AddUserSecrets<Startup>();
            }

            Configuration = builder.Build();
        }

        /// <summary>
        /// Gets the configuration that represents a set of key/value application configuration properties.
        /// </summary>
        /// <value>
        /// The <see cref="IConfiguration"/> that represents a set of key/value application configuration properties.
        /// </value>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> specifies the contract for a collection of service descriptors.</param>
        /// <seealso cref="IStatePropertyAccessor{T}"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/aspnet/web-api/overview/advanced/dependency-injection"/>
        /// <seealso cref="https://docs.microsoft.com/en-us/azure/bot-service/bot-service-manage-channels?view=azure-bot-service-4.0"/>
        public void ConfigureServices(IServiceCollection services)
        {
            var secretKey = Configuration.GetSection("botFileSecret")?.Value;
            var botFilePath = Configuration.GetSection("botFilePath")?.Value;
            var botConfig = BotConfiguration.Load(botFilePath ?? @".\ISpyBot.bot", secretKey);
            services.AddSingleton(sp => botConfig ?? throw new InvalidOperationException($"The .bot config file could not be loaded. ({botConfig})"));

            services.AddOptions();
            services.Configure<BotAuthConfig>(Configuration.GetSection("BotAuth"));
            services.Configure<SpeechConfig>(Configuration.GetSection("Speech"));
            services.Configure<VisionConfig>(Configuration.GetSection("Vision"));

            var dataStore = GetBlobDataStore(botConfig);

            var conversationState = GetBlobConversationState(dataStore);
            var userState = GetBlobUserState(dataStore);

            services.AddSingleton(conversationState);
            services.AddSingleton(userState);

            services.AddSingleton(new BotStateSet(conversationState, userState));

            services.AddBot<ISpyBotBot>(options =>
            {
                // Retrieve current endpoint.
                var environment = _isProduction ? "production" : "development";
                var service = botConfig.Services.FirstOrDefault(s => s.Type == "endpoint" && s.Name == environment);
                if (!(service is EndpointService endpointService))
                {
                    throw new InvalidOperationException($"The .bot file does not contain an endpoint with name '{environment}'.");
                }

                options.CredentialProvider = new SimpleCredentialProvider(endpointService.AppId, endpointService.AppPassword);

                // Creates a logger for the application to use.
                ILogger logger = _loggerFactory.CreateLogger<ISpyBotBot>();

                // Catches any errors that occur during a conversation turn and logs them.
                options.OnTurnError = async (context, exception) =>
                {
                    logger.LogError($"Exception caught : {exception}");
                    await context.SendActivityAsync("Sorry, it looks like something went wrong.");
                };

                options.Middleware.Add(new AutoSaveStateMiddleware(conversationState, userState));
            });

            services.AddSingleton<ISpyBotAccessors>(sp =>
            {
                var accessors = new ISpyBotAccessors(conversationState, userState)
                {
                    ISpyBotState = conversationState.CreateProperty<ISpyBotState>(ISpyBotAccessors.ConversationDataName),
                    DialogState = conversationState.CreateProperty<DialogState>(ISpyBotAccessors.DialogStateName),
                };

                return accessors;
            });

            services.AddMvc()
                 .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                 .AddJsonOptions(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());
        }

        private static ConversationState GetBlobConversationState(IStorage dataStore)
        {
            var conversationState = new ConversationState(dataStore);
            return conversationState;
        }

        private static UserState GetBlobUserState(IStorage dataStore)
        {
            var userState = new UserState(dataStore);
            return userState;
        }

        private static IStorage GetBlobDataStore(BotConfiguration botConfig)
        {
            const string StorageConfigurationId = "BotStateBlob";
            var blobConfig = botConfig.FindServiceByNameOrId(StorageConfigurationId);
            if (!(blobConfig is BlobStorageService blobStorageConfig))
            {
                throw new InvalidOperationException($"The .bot file does not contain an blob storage with name '{StorageConfigurationId}'.");
            }
            // Default container name.
            const string DefaultBotContainer = "bot-state-ispybot";
            var storageContainer = string.IsNullOrWhiteSpace(blobStorageConfig.Container) ? DefaultBotContainer : blobStorageConfig.Container;
            IStorage dataStore = new Microsoft.Bot.Builder.Azure.AzureBlobStorage(blobStorageConfig.ConnectionString, storageContainer);
            return dataStore;
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseBotFramework();

            app.UseHttpsRedirection();

            app.UseMvc();
        }
    }
}
