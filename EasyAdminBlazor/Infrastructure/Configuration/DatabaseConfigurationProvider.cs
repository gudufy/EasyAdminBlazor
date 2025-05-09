namespace EasyAdminBlazor.Infrastructure.Configuration
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly IFreeSql _fsql;

        public DatabaseConfigurationProvider(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        public override void Load()
        {
            LoadFromDatabase();
        }

        private void LoadFromDatabase()
        {
            var settings = _fsql.Select<SysConfig>().ToList();
            var data = new Dictionary<string, string>();
            foreach (var setting in settings)
            {
                data[setting.Code] = setting.Content;
            }
            Data = data;
        }

        public void Reload()
        {
            LoadFromDatabase();
            OnReload();
        }
    }

    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private readonly IFreeSql _fsql;

        public DatabaseConfigurationSource(IFreeSql fsql)
        {
            _fsql = fsql;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return new DatabaseConfigurationProvider(_fsql);
        }
    }

    public static class DatabaseConfigurationExtensions
    {
        public static IConfigurationBuilder AddDatabaseConfiguration(this IConfigurationBuilder builder, IFreeSql fsql)
        {
            builder.Add(new DatabaseConfigurationSource(fsql));
            return builder;
        }
    }
}
