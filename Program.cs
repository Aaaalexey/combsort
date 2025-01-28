using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
class Program
{
    private static HttpClient client = new HttpClient();
    private const string BaseUrl = "http://localhost:5000";
    static async Task Main(string[] args)
    {
        client.BaseAddress = new Uri(BaseUrl);
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        bool isRunning = true;

        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1. Регистрация");
            Console.WriteLine("2. Авторизация");
            Console.WriteLine("3. Выход");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await HandleSignUp();
                    break;

                case "2":
                    await HandleLogin();
                    break;

                case "3":
                    isRunning = false;
                    break;

                default:
                    Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    static async Task HandleSignUp()
    {
        Console.Write("Введите логин: ");
        string? login = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string? password = Console.ReadLine();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        { Console.WriteLine("Логин и пароль не могут быть пустыми."); return; }

        try
        {
            var response = await client.PostAsync($"/signup?login={Uri.EscapeDataString(login)}&password={Uri.EscapeDataString(password)}", null);

            if (response.IsSuccessStatusCode)
            { Console.WriteLine("Пользователь успешно зарегистрирован."); }
            else if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
            { Console.WriteLine("Пользователь с таким логином уже существует.");  }
            else
            { Console.WriteLine("Ошибка регистрации: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }

    static async Task HandleLogin()
    {
        Console.Write("Введите логин: ");
        string? login = Console.ReadLine();
        Console.Write("Введите пароль: ");
        string? password = Console.ReadLine();

        if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
        { Console.WriteLine("Логин и пароль не могут быть пустыми."); return; }
        try
        {
            var response = await client.PostAsync($"/login?login={Uri.EscapeDataString(login)}&password={Uri.EscapeDataString(password)}", null);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Авторизация прошла успешно.");
                await HandleArrayOperations();
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            { Console.WriteLine("Неверный логин или пароль."); }
            else
            { Console.WriteLine("Ошибка авторизации: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен. Проверьте подключение к интернету."); }
    }

    static async Task HandleArrayOperations()
    {
        bool isRunning = true;
        while (isRunning)
        {
            Console.Clear();
            Console.WriteLine("Выберите действие с массивом:");
            Console.WriteLine("1. Преобразовать строку в массив");
            Console.WriteLine("2. Отсортировать массив");
            Console.WriteLine("3. Получить случайный массив");
            Console.WriteLine("4. Получить часть массива");
            Console.WriteLine("5. Удалить массив");
            Console.WriteLine("6. Получить текущего пользователя");
            Console.WriteLine("7. Выйти");
            string? choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    await ConvertArrayToValues();
                    break;

                case "2":
                    await CombSort();
                    break;

                case "3":
                    await GetRandomArray();
                    break;

                case "4":
                    await GetArrayPartByBoarders();
                    break;

                case "5":
                    await DeleteArray();
                    break;

                case "6":
                    await GetCurrentUser();
                    break;

                case "7":
                    isRunning = false;
                    break;

                default:
                    Console.WriteLine("Некорректный выбор. Попробуйте снова.");
                    break;
            }
        }
    }

    static async Task ConvertArrayToValues()
    {
        Console.Write("Введите массив чисел через запятую: ");
        string? strArray = Console.ReadLine();

        if (string.IsNullOrEmpty(strArray))
        { Console.WriteLine("Ввод не может быть пустым."); return; }

        try
        {
            var content = new StringContent($"\"{strArray}\"", Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/get_array", content);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Преобразованный массив: " + result);
            }
            else
            { Console.WriteLine("Ошибка: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }
    static async Task CombSort()
    {
        try
        {
            var response = await client.PostAsync("/array_sorting", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Отсортированный массив: " + result);
            }
            else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Ошибка: " + errorMessage);
            }
            else
            { Console.WriteLine("Ошибка: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }
    static async Task GetRandomArray()
    {
        Console.Write("Введите размер массива: ");
        if (!int.TryParse(Console.ReadLine(), out int n) || n <= 0)
        { Console.WriteLine("Некорректный размер массива. Введите положительное число."); return; }

        try
        {
            var response = await client.GetAsync($"/get_random_array?n={n}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Случайный массив: " + result);
            }
            else
            { Console.WriteLine("Ошибка: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }

static async Task GetArrayPartByBoarders()
{
    Console.Write("Введите начальный индекс: ");
    if (!int.TryParse(Console.ReadLine(), out int start))
    { Console.WriteLine("Некорректный начальный индекс. Введите целое число."); return; }

    Console.Write("Введите конечный индекс: ");
    if (!int.TryParse(Console.ReadLine(), out int end))
    { Console.WriteLine("Некорректный конечный индекс. Введите целое число."); return; }

    if (start < 0)
    { Console.WriteLine("Ошибка: Начальный индекс не может быть меньше нуля."); return; }

    if (start > end)
    {Console.WriteLine("Ошибка: Начальный индекс не может быть больше конечного."); return; }

    try
    {
        var response = await client.GetAsync($"/get_array_part?start={start}&end={end}");

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Часть массива: " + result);
        }
        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            
            var errorMessage = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Ошибка: " + errorMessage);
        }
        else
        { Console.WriteLine("Ошибка: " + response.StatusCode); }
    }
    catch (HttpRequestException)
    { Console.WriteLine("Сервер недоступен"); }
}

    static async Task DeleteArray()
    {
        try
        {
            var response = await client.PostAsync("/delete_array", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine(result);
            }
            else
            { Console.WriteLine("Ошибка: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }

    static async Task GetCurrentUser()
    {
        try
        {
            var response = await client.GetAsync("/current_user");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                Console.WriteLine("Текущий пользователь: " + result);
            }
            else
            { Console.WriteLine("Ошибка: " + response.StatusCode); }
        }
        catch (HttpRequestException)
        { Console.WriteLine("Сервер недоступен"); }
    }
}