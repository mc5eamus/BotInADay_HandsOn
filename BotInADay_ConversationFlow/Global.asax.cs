using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;

namespace BotInADay_ConversationFlow
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var stateManager = ConfigurationManager.AppSettings["BotStateManager"];

            IBotDataStore<BotData> store = null;

            if ("DocumentDb".Equals(stateManager))
            {
                var uri = new Uri(ConfigurationManager.AppSettings["DocumentDbUrl"]);
                var key = ConfigurationManager.AppSettings["DocumentDbKey"];
                store = new DocumentDbBotDataStore(uri, key);
            }


            Conversation.UpdateContainer(
                builder =>
                {
                    if (store != null)
                    {
                        builder.Register(c => store)
                            .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                            .AsSelf()
                            .SingleInstance();

                        builder.Register(c => new CachingBotDataStore(store, CachingBotDataStoreConsistencyPolicy.ETagBasedConsistency))
                            .As<IBotDataStore<BotData>>()
                            .AsSelf()
                            .InstancePerLifetimeScope();
                    }
                });

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
