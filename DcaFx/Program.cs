using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Owin.Hosting;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.Owin;
using Newtonsoft.Json;
using Owin;

namespace DcaFx
{
    class Program
    {
        static void Main(string[] args)
        {
            var startOptions = new StartOptions {Port = 8586};

            using (WebApp.Start<Startup>(startOptions))
            {
                Console.WriteLine("Running on port {0}", startOptions.Port);
                while (true)
                {
                    System.Threading.Thread.Sleep(500);
                }
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

        public Parameters(params dynamic[] values)
        {
            var config = new MapperConfiguration(cfg => { });
            _mapper = config.CreateMapper();
            foreach (var value in values) Map(value);
        }

        public void Map(dynamic query)
        {
            _mapper.Map<dynamic, Parameters>(query, this);
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
        private Adapter _adapter;

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
                response = (Response) $"{parameters.callback}({json});";
                response.ContentType = "application/javascript";
            }
            return response;

        }
    }
}
