namespace CrewTasks.Api.OpenApi;

public class OpenApiVersioningTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "Crew Tasks API",
            Version = "v1",
            Description = "API for managing crew tasks.",
            Contact = new OpenApiContact
            {
                Name = "Gabriel Abreu",
                Url = new Uri("https://grabreu.dev")
            }
        };

        return Task.CompletedTask;
    }
}
