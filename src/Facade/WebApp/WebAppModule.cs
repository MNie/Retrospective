namespace WebApp
{
    using Autofac;
    using Configuration;
    using Controllers;
    using IO.Ably;

    public class WebAppModule : Module
    {
        private readonly AblyConfig _config;

        public WebAppModule(AblyConfig config) => _config = config;

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.Register(_ => _config).AsSelf().SingleInstance();
            builder.Register(_ => new AblyRealtime(_config.ApiKey)).AsSelf().SingleInstance();
        }
    }
}