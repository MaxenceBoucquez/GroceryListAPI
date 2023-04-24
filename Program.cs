using APIAzure.Models;
using APIAzure.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddScoped<IStorageService<Article>, ArticleStorageService>();
builder.Services.AddScoped<IStorageService<User>, UserStorageService>();
builder.Services.AddScoped<IStorageService<ArticleUser>, ArticleUserStorageService>();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI();


app.UseStaticFiles();

app.UseCors(options =>
     options.AllowAnyOrigin()
     .AllowAnyMethod() 
     .AllowAnyHeader());

app.UseHttpsRedirection();

app.UseRouting();


app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
