namespace CatalogoApi.AppServicesExtensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        return app;
    }

    public static IApplicationBuilder UserAppCors(this IApplicationBuilder app)
    {
        app.UseCors(p =>
        {
            p.AllowAnyOrigin();
            p.WithMethods("GET");
            p.AllowAnyMethod();
        });
        return app;
    }

    public static IApplicationBuilder UseSwaggerMiddleware(this IApplicationBuilder app) 
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        return app;
    }

}