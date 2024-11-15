using Contrib.SiteMessage;
using Data.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using RobotsTxt;
using SixLabors.ImageSharp.Web.DependencyInjection;
using Web.Extensions;
using Web.Filters;
using Web.Middlewares;
using Web.Services;

var builder = WebApplication.CreateBuilder(args);

var mvcBuilder = builder.Services.AddControllersWithViews(
    options => { options.Filters.Add<ResponseWrapperFilter>(); }
);

// Enable Razor page dynamic compilation in development mode
if (builder.Environment.IsDevelopment()) mvcBuilder.AddRazorRuntimeCompilation();

builder.Services.AddAutoMapper(typeof(Program));
builder.Services.AddFreeSql(builder.Configuration);
builder.Services.AddHttpClient();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policyBuilder =>
    {
        policyBuilder.AllowCredentials();
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        // policyBuilder.AllowAnyOrigin();
        policyBuilder.WithOrigins("http://localhost:8080");
    });
});
builder.Services.AddStaticRobotsTxt(builder => builder
    .AddSection(section => section.AddUserAgent("Googlebot").Allow("/"))
    .AddSection(section => section.AddUserAgent("bingbot").Allow("/"))
    .AddSection(section => section.AddUserAgent("Bytespider").Allow("/"))
    .AddSection(section => section.AddUserAgent("Sogou web spider").Allow("/"))
    .AddSection(section => section.AddUserAgent("*").Disallow("/"))
);
builder.Services.AddSwagger();
builder.Services.AddSettings(builder.Configuration);
builder.Services.AddAuth(builder.Configuration);
builder.Services.AddHttpContextAccessor();
// Register IHttpClientFactory, reference: https://docs.microsoft.com/zh-cn/dotnet/core/extensions/http-client
builder.Services.AddHttpClient();
builder.Services.AddImageSharp();

// Register custom services
builder.Services.AddScoped<BlogService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ConfigService>();
builder.Services.AddScoped<LinkExchangeService>();
builder.Services.AddScoped<LinkService>();
builder.Services.AddScoped<PhotoService>();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<VisitRecordService>();
builder.Services.AddSingleton<CommonService>();
builder.Services.AddSingleton<CrawlService>();
builder.Services.AddSingleton<Messages>();
builder.Services.AddSingleton<ThemeService>();
builder.Services.AddSingleton<PicLibService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseImageSharp();
// app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ServeUnknownFileTypes = true
});

app.UseMiddleware<VisitRecordMiddleware>();

app.UseRobotsTxt();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api-docs/swagger";
    options.SwaggerEndpoint("/swagger/admin/swagger.json", "Admin");
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