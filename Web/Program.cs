using Contrib.SiteMessage;
using Data.Extensions;
using Web.Extensions;
using Web.Filters;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options => { options.Filters.Add<ResponseWrapperFilter>(); });
builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.WithOrigins("http://localhost:8080");
    });
});
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHttpContextAccessor();

// Custom services
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<Messages>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api-docs/swagger";
    options.SwaggerEndpoint("/swagger/blog/swagger.json", "Blog");
    options.SwaggerEndpoint("/swagger/auth/swagger.json", "Auth");
    options.SwaggerEndpoint("/swagger/common/swagger.json", "Common");
    options.SwaggerEndpoint("/swagger/test/swagger.json", "Test");
});

app.UseReDoc(options =>
{
    options.RoutePrefix = "api-docs/redoc";
    options.SpecUrl = "/swagger/blog/swagger.json";
});

app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");
app.Run();