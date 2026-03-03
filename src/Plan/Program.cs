var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddDbContext<PlanDbContext>((sp, options) =>{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<PlanDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);


builder.Services.AddFudieJwksProvider();

var attributeAssemblyName = typeof(InjectableAttribute).Assembly.GetName().Name;

builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

app.UseFudieOpenApi();

app.MapFeatures(builder=>builder.UseFudieAuthorization());

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
