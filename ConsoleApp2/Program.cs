using System;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

class Book
{
    public string Id = Guid.NewGuid().ToString();
    public string Title { get; set; }
    public string Author { get; set; }
    public string Language { get; set; }
    public string Publisher { get; set; }
    public string Year { get; set; }
    public int Pages { get; set; }
    public string CoverUrl { get; set; }
}

class Program
{
    static string ConnectNumbers(int start, int end)
    {
        string result = "";

        if (start <= end)
        {
            result = start.ToString();
            int current = start + 1;

            while (current <= end)
            {
                result += ",";
                result += current.ToString();
                current++;
            }
        }

        return result;
    }

    static async Task Main(string[] args)
    {
        int numberOfBooks = 500;

        var ids = ConnectNumbers(1, numberOfBooks);

        // Specify the number of best books you want to retrieve.

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"Retrieving {numberOfBooks} books");
        Console.ResetColor();

        List<Book> books = new List<Book>();
        // Create an HttpClient to make the API request.
        using (HttpClient client = new HttpClient())
        {
            try
            {
                // Build the Libgen API URL with your API key and the number of books.
                string apiUrl = $"http://libgen.is/json.php?ids={ids}&fields=title,author,year,language,publisher,coverurl";

                // Make the API request.
                HttpResponseMessage response = await client.GetAsync(apiUrl);

                // Check if the request was successful.
                if (response.IsSuccessStatusCode)
                {
                    // Read the JSON response as a string.
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    // Parse the JSON into a list of books.
                    List<Book> booksData = JsonConvert.DeserializeObject<List<Book>>(jsonResponse);
                    
                    foreach (var b in booksData)
                    {
                        if (!books.Any(a => a.Title == b.Title))
                        b.Id = Guid.NewGuid().ToString();

                        string fullCoverUrl = "http://libgen.is/covers/" + b.CoverUrl;
                        b.CoverUrl = fullCoverUrl;

                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($"Retrieved book : {b.Title}. Rank : {books.Count + 1}.");
                        Console.ResetColor();

                        books.Add(b);
                    }
                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }

        // Get the path to the user's desktop folder.
        string desktopFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Create a folder named "BookImages" on the desktop if it doesn't exist.
        string bookImagesFolder = Path.Combine(desktopFolderPath, "BookImages");
        Directory.CreateDirectory(bookImagesFolder);

        var validBooks = new List<Book>();
        using (WebClient client = new WebClient())
        {
            foreach (var book in books)
            {
                try
                {
                    // Construct the full path for the image file.
                    string filename = Path.Combine(bookImagesFolder, $"{book.Title.Replace(" ", "_").ToLower()}.jpg");

                    // Download the book image to the "BookImages" folder.
                    client.DownloadFile(book.CoverUrl, filename);

                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"Downloaded {book.Title} image");
                    Console.ResetColor();

                    book.Title = "https://media.aykhan.net/assets/images/step-it-academy/react/task13/book-images/" + filename;
                    validBooks.Add(book);
                }
                catch (Exception)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Error Downloading {book.Title} image");
                    Console.ResetColor();
                }
            }
        }

        // Serialize the book list to JSON and save it to a file.
        string json = JsonConvert.SerializeObject(validBooks, Newtonsoft.Json.Formatting.Indented);
        File.WriteAllText("~/../../../books.json", json);

        Console.WriteLine("Books data saved to 'books.json'");
    }
}
