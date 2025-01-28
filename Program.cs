using System.Data.Common;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

CSWebAdapter csWeb = new CSWebAdapter();

DBManager db = new DBManager();

app.MapGet("/", () => "please, enter array");

app.MapPost("/login", async (string login, string password, HttpContext context) => 
{
    if (!db.CheckUser(login, password))
        return Results.Unauthorized();

    var claims = new List<Claim> {new Claim(ClaimTypes.Name, login) };
    var claimsIdentity = new ClaimsIdentity(claims, "Cookies");
    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));
    return Results.Ok();    
});

app.MapPost("/signup", (string login, string password) => 
{
    if (db.AddUser(login, password))
        return Results.Ok("Пользователь " + login + " успешно зарегистрирован!");
    else
        return Results.Problem("Ошибка решистрации пользователя: " + login);
});

app.MapPost("/get_array", [Authorize]([FromBody] string strArray) => csWeb.ConvertArrayToValues(strArray));

app.MapPost("/array_sorting", [Authorize] () => csWeb.CombSort());

app.MapGet("/get_random_array", [Authorize] ([FromQuery] int n) => csWeb.GetRandomArray(n));

app.MapGet("/get_array_part", [Authorize] ([FromQuery] int start, [FromQuery] int end) => csWeb.GetArrayPartByBoarders(start, end));

app.MapPost("/delete_array", [Authorize] () => csWeb.DeleteArray());

app.MapGet("/current_user", [Authorize] (HttpContext context) => {
    if (context.User.Identity == null)
        return Results.BadRequest("Нет имени пользователя");
    return Results.Ok(context.User.Identity.Name);
});


const string DB_PATH = "/home/alexey/ComSorting.Users.db";
if (!db.ConnectToDB(DB_PATH))
{
    Console.WriteLine("Ошибка подключения к базе данных " + DB_PATH);
    Console.WriteLine("Выключение!");
    return;
}

app.Run();
db.Disconnect();

public class CSWebAdapter()
{
    private CombSorting cs = new CombSorting();

    public string DeleteArray() 
    { return cs.DeleteArray(); }

    public int[] ConvertArrayToValues(string strArray) 
    { return cs.ConvertArrayToValues(strArray); }

    public int[] CombSort() 
    { return cs.CombSort(); }

    public int[] GetRandomArray(int n) 
    { return cs.GetRandomArray(n); }

    public int[] GetArrayPartByBoarders(int start, int end) 
    { return cs.GetArrayPartByBoarders(start, end); } 
} 
public class CombSorting
{
    private int[]? userArray; 

    public string DeleteArray() 
    { userArray = null; return "Массив удален успешно"; }
    
    public int[] GetArrayPartByBoarders(int start, int end) 
    {
        int[] result = new int[end - start + 1];
        Array.Copy(userArray, start, result, 0, end - start + 1);
        return result;
    }

    public int[] GetRandomArray(int n) 
    {
        Random random = new Random(); 
        userArray = new int[n]; 

        for (int i = 0; i < n; i++)
        { userArray[i] = random.Next(-1000, 1000); }

        return userArray;
    }
    public int[] ConvertArrayToValues(string strArray) 
    {
    string[] arrayItems = strArray.Split(',');

    if (arrayItems.Length == 0 || arrayItems.All(string.IsNullOrWhiteSpace)) 
    { throw new ArgumentException("Входная строка не содержит данных."); }

    int[] arrayNumbers = new int[arrayItems.Length];

    for (int i = 0; i < arrayItems.Length; i++)
    { arrayNumbers[i] = int.Parse(arrayItems[i]);}// вспомогательный метод для сортировки

    userArray = arrayNumbers; 

    return arrayNumbers; 
    }

    public int[] CombSort() 
    {
        if (userArray == null || userArray.Length == 0)
        { throw new InvalidOperationException("Массив не инициализирован или пуст.\n\n"); }

        int[] array = new int[userArray.Length];
        Array.Copy(userArray, array, userArray.Length);
        double gap = array.Length;
        bool swapped = true;
        while (gap > 1 || swapped)
        {
            gap /= 1.247;
            if (gap < 1) gap = 1;
            int i = 0;
            swapped = false;
            while (i + gap < array.Length)
            {
                int j = i + (int)gap;
                if (array[i] > array[(int)(i + gap)])
                {
                    Swap(ref array[i], ref array[(int)(i + gap)]);
                    swapped = true;
                }
                i++;
            }
        } 
        userArray = array; 
        
        return array;
    }

    static void Swap(ref int a, ref int b) 
    {
        int temp = a;
        a = b;
        b = temp;
    }
}

