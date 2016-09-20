using System;
using System.Threading;
using AutoMapper;
using Microsoft.Owin.Hosting;
using Nancy;
using Nancy.Bootstrapper;
using Owin;

namespace DcaFx
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = 8586;
            if (args != null && args.Length > 0)
            {
                port = int.Parse(args[0]);
            }
            var startOptions = new StartOptions {Port = port};

            using (WebApp.Start<Startup>(startOptions))
            {
                Console.WriteLine("Running on port {0}", startOptions.Port);

                while (true) Thread.Sleep(500);
            }
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseNancy();
        }
    }

    public class FooBeforeAllRequests : IApplicationStartup
    {

        public void Initialize(IPipelines pipelines)
        {
            pipelines.OnError.AddItemToStartOfPipeline((context, exception) =>
            {
                Console.WriteLine(exception.ToString());
                return null;
            });
        }
    }

    public class QueryConverter
    {
        protected T Conv<T>(object input)
        {
            return (T)Convert.ChangeType(input, typeof(T));
        }

        public T Query<T>(object input)
        {
            return (T)Convert.ChangeType(Conv<string>(input), typeof(T));
        }
    }

    public class Parameters
    {
        private readonly IMapper _mapper;

        public Parameters(){}
        public Parameters(params dynamic[] values)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Parameters, Parameters>();
            });
            _mapper = config.CreateMapper();
            foreach (var value in values) Map(value);
        }

        public void Map(dynamic query)
        {
            var newValues = _mapper.Map<Parameters>(query);
            _mapper.Map(newValues, this);
        }

        public string type { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string format { get; set; }
        public string callback { get; set; }
        public int? limit { get; set; }
    }

    public class DefaultModule : NancyModule
    {
        private readonly Adapter _adapter;

        public DefaultModule()
        {
            Get["/"] = DefaultRoute;
            Get["/delault"] = DefaultRoute;
            Get["/type/{type}"] = DefaultRoute;
            Get["/city/{city}"] = DefaultRoute;
            Get["/country/{country}"] = DefaultRoute;

            _adapter = new Adapter();
        }

        private dynamic DefaultRoute(dynamic routeParameters)
        {
            var parameters = new Parameters(routeParameters, Request.Query);
            var json = _adapter.Process(
                parameters.limit,
                parameters.type?.Split(','),
                parameters.city?.Split(','),
                parameters.country?.Split(','),
                parameters.format
            );
            Response response;
            if (string.IsNullOrWhiteSpace(parameters.callback))
            {
                response = (Response) json;
                response.ContentType = "application/json";
            }
            else
            {
                response = (Response) String.Format("{0}({1});", parameters.callback, json);
                response.ContentType = "application/javascript";
            }
            return response;

        }
    }
}
