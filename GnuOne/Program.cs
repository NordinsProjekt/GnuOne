using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Library.Models;
using MailKit.Net.Pop3;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using Welcome_Settings;

string[] empty = { string.Empty };
bool keepGoing = true;
while (keepGoing)
{
    Console.Clear();
    Meny.DefaultWindow2("");
    Meny.Draw(Meny.EnterCredMenu(""), 38, 15, ConsoleColor.White);
    Global.ConnectionString = EnterCredMenuInput();

    //kolla hur consolen blir när man gör en ny DB

    MariaContext context = new MariaContext(Global.ConnectionString);
    MariaContext DbContext = new MariaContext(Global.CompleteConnectionString);
    WriteToJson("ConnectionStrings:Defaultconnection", Global.CompleteConnectionString);

    //kollar om det är rätt inlog med att skicka någonting till db:n
    if (await CheckConnection(context))
    {
        //no gnu - skapa db
        if (!IsThereAGnu(DbContext))
        {
            CreateDatabase(Global.ConnectionString);
        }

        //finns det i db.mysettings
        if (IsThereMailCredentials(DbContext))
        {
            keepGoing = false;
        }
        else
        {
            Console.Clear();
            Meny.DefaultWindow2("");
            Meny.Draw(Meny.EnterCredMenu(""), 38, 15, ConsoleColor.White);
            Console.Clear();
            Meny.DefaultWindow2("");
            Meny.Draw(Meny.EnterMailInfo(""), 38, 15,ConsoleColor.White);
            Console.CursorLeft = 38;
            Console.Write("Write your Email: ");
            var email = Console.ReadLine();
            Console.CursorLeft = 38;
            Console.Write("EmailPassword: ");
            var password = pwMask.pwMasker();
            ///hårdkodad
            Console.Clear();
            Meny.DefaultWindow2("");
            Meny.Draw(Meny.EnterUsername(""), 38, 15, ConsoleColor.White);
            Console.CursorLeft = 38;
            Console.Write("Choose your username: ");
            var username = Console.ReadLine();
            var secretk = RandomKey();
            password = AesCryption.Encrypt(password, "secretkey"); //ska bytas

            //Skapar RSA nycklar och sparar dessa i databasen.
            //Framtiden så ska detta skyddas genom krypering av den privata nyckeln.
            RSA rsa = RSA.Create(4096);
            string publicKey = Convert.ToBase64String(rsa.ExportRSAPublicKey());
            string privateKey = Convert.ToBase64String(rsa.ExportRSAPrivateKey());
            
            var settings = new MySettings
            {
                ID = 1,
                Email = email,
                Password = password,
                userName = username,
                Secret = "secretkey",
                MyPrivKey = privateKey //här ska private key finnas
            };
            var profile = new myProfile //Hårdkodat
            {
                ID = 1,
                Email = email,
                pictureID = 1,
                MyPubKey = publicKey //här ska public key finnas
            };
            rsa.Dispose();
            try
            {
                await DbContext.MyProfile.AddAsync(profile);
                await DbContext.MySettings.AddAsync(settings);
                await DbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            keepGoing = false;
        };
    }
    else
    {
        Console.Clear();
        Console.WriteLine("Det gick inte att ansluta till databasen testa igen");
        Console.WriteLine();
    }
}

//Öppnar websidan.
//Process.Start(new ProcessStartInfo
//{
//    FileName = "https://localhost:5001/",
//    UseShellExecute = true
//});

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
            ///får att tillåta kommunikation från/till FrontEND
            builder.AllowAnyHeader();
            builder.AllowAnyOrigin();
            builder.AllowAnyMethod();

            string[] array = { "https://localhost:5001", "http://localhost:5000", "http://localhost:7261", "http://localhost:5261", "https://localhost:44486", "https://localhost:7261" };
            builder.WithOrigins(array);
        });
});
builder.Services.AddControllersWithViews();
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "ClientApp/build";
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSpaStaticFiles();
app.UseRouting();
app.UseCors();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.UseSpa(spa =>
{
    spa.Options.SourcePath = "ClientApp";

});
app.MapFallbackToFile("index.html");


int a = 0; //Visualiserar att mailfunktionen rullar.
var loop1Task = Task.Run(async () =>
{

    while (true)
    {

        try
        {
            using (MariaContext context = new MariaContext(_connectionstring))
            {


                MailReader.ReadUnOpenEmails(context, _connectionstring);
                a++;
                Console.Clear();
                Meny.DefaultWindow2("");
                Meny.Draw(empty, 38, 20, ConsoleColor.White);

                Console.WriteLine("The Gnu is reading your dedicated inbox");
                Meny.Draw(empty, 0, 25, ConsoleColor.White);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        await Task.Delay(2000);
    }
});
app.Run();



bool IsThereAGnu(MariaContext dbcontext)
{
    try
    {
        dbcontext.Standardpictures.Any();
        return true;
    }
    catch
    {
        return false;
    }
}
static bool IsThereMailCredentials(MariaContext _newContext)
{
    //finns det nått finns det allt - Frontend validering
    if (_newContext.MySettings.Any())
    {
        return true;
    }
    else
    {
        return false;
    }
}
static void CreateDatabase(/*bool created,*/ string connection)
{
    DbCommand.CreateCommand(Global.sql, connection);
}
static async Task<bool> CheckConnection(MariaContext context)
{
    try
    {   //Försöker skicka in ett vanligt sql scrip
        var dontreallycareaboutwhatgetsback = await context.Database.ExecuteSqlRawAsync("SELECT 1");
    }
    catch (Exception)
    {
        return false;
    }
    return true;
}
static string EnterCredentials()
{
    Console.WriteLine("Hello! \n"
                      + "Thanks for using this app.\n" +
                          "Please enter your heidi-username and password.");
    Console.WriteLine();
    Console.Write("Username: ");

    var inputUserName = Console.ReadLine();

    Console.Write("Password: ");

    string inputPassWord = pwMask.pwMasker();

    Console.WriteLine();

    ///connectionstringen byggs
    string newConnection = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";";
    /// den fullständiga med DB till global
    Global.CompleteConnectionString = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";database=gnu;";

    ///write to appsettings.json

    return newConnection;
    //Console.Clear();
    //Console.ForegroundColor = ConsoleColor.Blue;
    //Console.WriteLine(" - Dont shut this window down - ");
    //Console.WriteLine();
}
static void WriteToJson(string sectionPathKey, string value)
{
    string file = "\\appsettings.json";
    string path = Directory.GetCurrentDirectory();
    string fullpath = Path.GetFullPath(path + file);

    //Console.WriteLine(Path.GetFullPath(path));
    try
    {
        var filePath = fullpath;
        string json = File.ReadAllText(filePath);
        dynamic? jsonObj = JsonConvert.DeserializeObject(json);

        SetValueRecursively(sectionPathKey, jsonObj, value);

        string output = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObj, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText(filePath, output);
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error writing app settings | {0}", ex.Message);
    }
}
static void SetValueRecursively(string sectionPathKey, dynamic? jsonObject, string value)
{
    // split the string at the first ':' character
    var remainingSections = sectionPathKey.Split(":", 2);

    var currentSection = remainingSections[0];
    if (remainingSections.Length > 1)
    {
        // continue with the procress, moving down the tree
        var nextSection = remainingSections[1];
        SetValueRecursively(nextSection, jsonObject[currentSection], value);
    }
    else
    {
        // we've got to the end of the tree, set the value
        jsonObject[currentSection] = value;
    }
}
static string RandomKey()
{
    StringBuilder sb = new StringBuilder();
    Random rnd = new Random();
    string letters = @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!""#%&/()=?[]@$*-+";
    for (int i = 0; i < 10; i++)
    {
        sb.Append(letters[rnd.Next(0, letters.Length)]);
    }
    return sb.ToString();
}




static string[] FirstStartInput(int menuNum)
{
    switch (menuNum)
    {
        case 1:
            {
                int cursorLength = Meny.LongestString(Meny.FirstTimeUserMenu(""));
                Console.SetCursorPosition(cursorLength + 38, 16);
                string email = Console.ReadLine();
                Console.SetCursorPosition(cursorLength + 38, 17);
                string password = pwMasker();
                Console.SetCursorPosition(cursorLength + 38, 19);
                string username = Console.ReadLine();
                Meny.DefaultConsoleSettings();

                string newConnection;


                return new string[] { email, password, username };
            }
        case 2:
            {
                int cursorLength = Meny.LongestString(Meny.FirstTimeUserMenu(""));
                Console.SetCursorPosition(cursorLength + 38, 16);
                string email = Console.ReadLine();
                Console.SetCursorPosition(cursorLength + 38, 17);
                string password = pwMasker();
                Console.SetCursorPosition(cursorLength + 38, 19);
                string username = Console.ReadLine();
                Meny.DefaultConsoleSettings();


                return new string[] { email, password, username };
            }
        case 3:
            {
                int cursorLength = Meny.LongestString(Meny.FirstTimeUserMenu(""));
                Console.SetCursorPosition(cursorLength + 2, 16);
                string email = Console.ReadLine();
                Console.SetCursorPosition(cursorLength + 2, 17);
                string password = pwMasker();
                Console.SetCursorPosition(cursorLength + 2, 19);
                string username = Console.ReadLine();
                Meny.DefaultConsoleSettings();
                return new string[] { email, password, username };
            }
        default:
            return new string[] { "", "", "" };
    }
}
static string EnterCredMenuInput()
{

    int cursorLength = Meny.LongestString(Meny.EnterCredMenu(""), 4);
    Console.SetCursorPosition(cursorLength + 40, 20);
    string? inputUserName = Console.ReadLine();
    Console.SetCursorPosition(cursorLength + 40, 21);
    string? inputPassWord = pwMasker();
    Meny.DefaultConsoleSettings();

    string newConnection = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";";
    Global.CompleteConnectionString = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";database=gnu;";

    return newConnection;

}
static string pwMasker()
{
    var inputP = string.Empty;
    ConsoleKey key;
    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && inputP.Length > 0)
        {
            Console.Write("\b \b");
            inputP = inputP[0..^1];
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            Console.Write("*");
            inputP += keyInfo.KeyChar;
        }
    } while (key != ConsoleKey.Enter);
    return inputP;
}