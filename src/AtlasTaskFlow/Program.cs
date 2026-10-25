using AtlasTaskFlow.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews(options =>
{
    // Every POST form must carry an anti-forgery token.
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddHttpContextAccessor();

// Sprint 4 keeps data in memory; one shared store for the whole app, seeded with sample teams.
builder.Services.AddSingleton(_ =>
{
    var store = new AtlasStore();
    SeedData.Populate(store, DateTime.Today);
    return store;
});
builder.Services.AddSingleton<TaskWorkflowService>();
builder.Services.AddScoped<CurrentUserContext>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Friendly pages for 404 and other error status codes instead of a blank browser error.
app.UseStatusCodePagesWithReExecute("/Home/Status/{0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Lets the integration tests start the app with WebApplicationFactory<Program>.
public partial class Program { }
