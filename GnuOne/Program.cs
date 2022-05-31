using GnuOne.Data;
using Library;
using Library.HelpClasses;
using Library.Models;
using Newtonsoft.Json;
using System.Data.SQLite;
using System.Security.Cryptography;
using System.Text;
using Welcome_Settings;
using Microsoft.EntityFrameworkCore;

string[] empty = { string.Empty };
bool keepGoing = false;
Global.ConnectionString = "DataSource=gnuone.sqlite";
MariaContext context = new MariaContext(Global.ConnectionString);
MariaContext DbContext = new MariaContext(Global.ConnectionString);
WriteToJson("ConnectionStrings:Defaultconnection", Global.ConnectionString);
if (!File.Exists("gnuone.sqlite"))
{
    //Skapar databasen
    CreateDatabase("sqlite");
    keepGoing = true;
}
if (keepGoing)
{
    Console.Clear();
    Meny.DefaultWindow2("");
    Meny.Draw(Meny.EnterCredMenu(""), 38, 15, ConsoleColor.White);
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
        UserName = username,
        Secret = "secretkey",
        //PEM FORMAT
        MyPrivKey = "-----BEGIN RSA PRIVATE KEY-----" + privateKey + "-----END RSA PRIVATE KEY-----", //här ska private key finnas
        DarkMode = false
    };

    var profile = new myProfile //Hårdkodat
    {
        ID = 1,
        Email = email,
        pictureID = 1,
        myUserInfo = "",
        //PEM FORMAT
        MyPubKey = "-----BEGIN RSA PUBLIC KEY-----" + publicKey +"-----END RSA PUBLIC KEY-----" //här ska public key finnas
    };
    rsa.Dispose();
    DbContext.AddAsync(settings);
    DbContext.AddAsync(profile);
    DbContext.SaveChangesAsync();
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
                .UseSqlite(_connectionstring)
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
app.Run();

void CreateDatabase(string typ)
{
    switch(typ)
    {
        case "sqlite":
            SQLiteConnection.CreateFile(@"gnuone.sqlite");
            SQLiteConnection m_dbConnection;
            m_dbConnection = new SQLiteConnection(@"Data Source=gnuone.sqlite;");
            //m_dbConnection.SetPassword("root");
            m_dbConnection.Open();
            string sql = Global.sqlite;
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            command.ExecuteNonQuery();
            m_dbConnection.Close();
            break;
        case "mysql":
            break;
        default:
            break;
    }
    //Behöver lite felhantering

}

//static string EnterCredentials()
//{
//    Console.WriteLine("Hello! \n"
//                      + "Thanks for using this app.\n" +
//                          "Please enter your heidi-username and password.");
//    Console.WriteLine();
//    Console.Write("Username: ");

//    var inputUserName = Console.ReadLine();

//    Console.Write("Password: ");

//    string inputPassWord = pwMask.pwMasker();

//    Console.WriteLine();

//    ///connectionstringen byggs
//    //string newConnection = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";";
//    /// den fullständiga med DB till global
//    //Global.CompleteConnectionString = "server=localhost;user id=" + inputUserName + ";password=" + inputPassWord + ";database=gnu;";

//    ///write to appsettings.json

//    //return newConnection;
//}
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