TypeDescriptor.AddAttributes(typeof(DateOnly), new TypeConverterAttribute(typeof(DateOnlyTypeConverter)));

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    options.SerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
});

builder.Services.AddScoped(typeof(Guid), sp =>
    sp.GetRequiredService<IFudieUser>().TenantId
    ?? throw new InvalidOperationException("TenantId is not available in the current request"));

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddDbContext<SchedulersDbContext>((sp, options) =>{
    options.UseFirestore(sp);
    options.LogTo(Console.WriteLine, LogLevel.Information, DbContextLoggerOptions.None);
}).AddInterfacesFor<SchedulersDbContext>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(), ServiceLifetime.Singleton);

builder.Services.AddFudieJwksProvider();

builder.Services.AddInjectables();

var app = builder.Build();

app.UseExceptionHandler();

app.UseFudieOpenApi();

app.MapFeatures(builder => builder.UseFudieAuthorization());

app.UseHttpsRedirection();

app.Run();

public partial class Program { }
