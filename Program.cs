
using OllamaSharp;

namespace WebApi_LLM_kiss
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthorization();

            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            // set up the client with local LLM
            var uri = new Uri("http://localhost:11434");//tbd: remmember to change to the docker compose service name
            var ollama = new OllamaApiClient(uri);

            // select a model which should be used for further operations
            ollama.SelectedModel = "llama3.2:1b";

            //Add the ollama client to the dependency container (For DI)
            builder.Services.AddSingleton<OllamaApiClient>(ollama);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();

                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", app.Environment.ApplicationName);
                });
            }

            //app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapGet("/ollama/ask/{question}", async (string question, OllamaApiClient ollamaClient) => {
                string? answer = null;
                var chat = new Chat(ollamaClient);
                await foreach (var reply in chat.SendAsync(question, null, null, null))
                    answer += reply;

                return Results.Ok(answer);
            });
          

            app.Run();
        }
    }
}
