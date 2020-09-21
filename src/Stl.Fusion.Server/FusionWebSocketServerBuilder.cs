using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Stl.Fusion.Server
{
    public readonly struct FusionWebSocketServerBuilder
    {
        private class AddedTag { }
        private static readonly ServiceDescriptor AddedTagDescriptor =
            new ServiceDescriptor(typeof(AddedTag), new AddedTag());

        public FusionBuilder Fusion { get; }
        public IServiceCollection Services => Fusion.Services;

        internal FusionWebSocketServerBuilder(FusionBuilder fusion)
        {
            Fusion = fusion;
            if (Services.Contains(AddedTagDescriptor))
                return;
            // We want above Contains call to run in O(1), so...
            Services.Insert(0, AddedTagDescriptor);

            Fusion.AddPublisher();
            Services.TryAddSingleton<WebSocketServer.Options>();
            Services.TryAddSingleton<WebSocketServer>();
        }

        public FusionBuilder BackToFusion() => Fusion;

        // ConfigureXxx

        public FusionWebSocketServerBuilder ConfigureWebSocketServer(
            WebSocketServer.Options options)
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(WebSocketServer.Options),
                options);
            Services.Replace(serviceDescriptor);
            return this;
        }

        public FusionWebSocketServerBuilder ConfigureWebSocketServer(
            Action<IServiceProvider, WebSocketServer.Options> optionsBuilder)
        {
            var serviceDescriptor = new ServiceDescriptor(
                typeof(WebSocketServer.Options),
                c => {
                    var options = new WebSocketServer.Options();
                    optionsBuilder.Invoke(c, options);
                    return options;
                },
                ServiceLifetime.Singleton);
            Services.Replace(serviceDescriptor);
            return this;
        }
    }
}