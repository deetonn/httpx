using System.Net;
using Http.Server;
using Http.Server.Requests;

using var server = new Server();

server.AddEndpoint<WeatherApiHttpEndpoint>();
server.AddEndpoint<TimingJsEndpoint>();
server.AddEndpoint<TimingHtmlEndpoint>();

await server.Begin();

public class JsonEndpoint : HttpEndpoint
{
    public override string Endpoint => "/api/json";

    public override async Task<HttpResponse> Execute(HttpListenerRequest request)
    {
        var response = await base.Execute(request);

        response.ContentType = "application/json";
        response.Body.Append("{ \"data\": 123 }");

        return response;
    }
}

public class WeatherApiHttpEndpoint : HttpEndpoint
{
    public override string Endpoint =>  "/api/weather";

    public override async Task<HttpResponse> Execute(HttpListenerRequest request)
    {
        var response = await base.Execute(request);

        response.ContentType = "application/json";
        response.Body.Append("{ \"data\": 123 }");

        return response;
    }
}

public class TimingJsEndpoint : HttpEndpoint
{
    public override string Endpoint => "/assets/js/timing.js";

    public override async Task<HttpResponse> Execute(HttpListenerRequest request)
    {
        var response = await base.Execute(request);

        response.SetBodyToFile("./assets/script.js");

        return response;
    }
}

public class TimingHtmlEndpoint : HttpEndpoint
{
    public override string Endpoint => "/";

    public override async Task<HttpResponse> Execute(HttpListenerRequest request)
    {
        var response = await base.Execute(request);

        response.SetBodyToFile("assets/index.html");

        return response;
    }
}
