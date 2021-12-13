using GnuOne.Data;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Welcome_Settings;

/// <summary>
/// 
/// </summary>



//N�r vi k�r welcome och startar gnuone.exe. Funkar inta kontakten med databasen. Det funkar om connectionstringen �r h�rdkordad.
/// Vill vi kanske att GnuOne startar Welcome settings/mail loopen. ist�llet f�r tv�rt om?
/// 
/// Mer att g�ra. G�ra klart contoller.
/// Frontend - l�gga in sin app i denna.

//async Mailloop();
//Process process = new Process();


//// Configure the process using the StartInfo properties.
//process.StartInfo.FileName = "process.exe";
//process.StartInfo.Arguments = "-n";
//process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
//process.Start();
//process.WaitForExit();// Waits here for the process to exit.


var builder = WebApplication.CreateBuilder(args);

string _connectionstring = builder.Configuration.GetConnectionString("DefaultConnection");

// Add services to the container.
builder.Services.AddDbContext<ApiContext>(Options =>
            Options
                .UseMySql(_connectionstring, ServerVersion.AutoDetect(_connectionstring))
            );

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(
        builder =>
        {
            ///f�r att till�ta kommunikation fr�n/till FrontEND
            builder.AllowAnyHeader();
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();

            string[] array = { "https://localhost:5001", "http://localhost:5000", "http://localhost:7261", "http://localhost:5261" };
            builder.WithOrigins(array);
        });
});
builder.Services.AddControllersWithViews();






var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html"); ;

app.Run();


/// Flytta �ver controllers hit med mailfunktionen
///     AddDbcontext.
///         Skriva in connectionstr�ngen in i Json fil. fr�n Welcome settings. Anv�nda appsettings f�r att skapa dbcontexten
///        
/// 
/// Fr�gor. Va h�nder n�r man g�ra en Process.Start fr�n Welcomesettings.