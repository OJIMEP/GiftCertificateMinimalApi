using System.Reflection;

namespace GiftCertificateMinimalApi.Endpoints.Internal
{
    public static class EndpointExtentions
    {
        public static void AddEndpoints<TMarker>(this IServiceCollection services, IConfiguration configuration)
        {
            AddEndpoints(services, typeof(TMarker), configuration);
        }

        public static void AddEndpoints(this IServiceCollection services, Type typeMarker, IConfiguration configuration)
        {
            var endpointsType = GetEndpointTypesFromAssembly(typeMarker);

            foreach (var endpoint in endpointsType)
            {
                endpoint.GetMethod(nameof(IEndpoints.AddServices))!
                    .Invoke(null, new object[] { services, configuration });
            }
        }

        public static void UseEndpoints<TMarker>(this IApplicationBuilder app)
        {
            UseEndpoints(app, typeof(TMarker));
        }

        public static void UseEndpoints(this IApplicationBuilder app, Type typeMarker)
        {
            var endpointsType = GetEndpointTypesFromAssembly(typeMarker);

            foreach (var endpoint in endpointsType)
            {
                endpoint.GetMethod(nameof(IEndpoints.DefineEndpoints))!
                    .Invoke(null, new object[] { app });
            }
        }

        private static IEnumerable<TypeInfo> GetEndpointTypesFromAssembly(Type typeMarker)
        {
            // ищем в проекте все классы, имплементирующие инерфейс IEndpoints
            // и так как это абстрактный интерфейс, не надо создавать объекты
            return typeMarker.Assembly.DefinedTypes
                            .Where(x => !x.IsAbstract && !x.IsInterface && typeof(IEndpoints).IsAssignableFrom(x));
        }

    }
}
