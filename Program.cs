using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Eto.Forms;
using Eto.Drawing;

class Program
{
    [STAThread]
    static void Main()
    {
        var app = new Application(Eto.Platforms.Gtk);
        string apiKey = "ad2ca6f113cbb990beb256aafaf1e7a1";

        var label = new Label { Text = "Zadej město:" };
        var textBox = new TextBox { Width = 50, Height = 10 };
        var button = new Button { Text = "Zjisti počasí" };

        var fieldWeather = new Label { Text = "Město" };

        var layout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = 10 };
        layout.AddRow(label);
        layout.AddRow(textBox);
        layout.AddRow(fieldWeather);
        layout.AddRow(button);

        var mainForm = new Form
        {
            Title = "Weather App",
            ClientSize = new Size(250, 400),
            Content = layout
        };

        button.Click += async (sender, e) =>
        {
            string city = textBox.Text.Trim();

            fieldWeather.Text = $"Město: {city}";

            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show(mainForm, "Prosím, zadej název města.");
                return;
            }

            string url = $"https://api.openweathermap.org/data/2.5/weather?q={Uri.EscapeDataString(city)}&appid={apiKey}&units=metric&lang=cz";

            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "CSharpApp");
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();

                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Získáme teplotu a vlhkost z "main"
                if (root.TryGetProperty("main", out JsonElement main))
                {
                    double temp = main.GetProperty("temp").GetDouble();
                    int humidity = main.GetProperty("humidity").GetInt32();

                    // Získáme popis počasí z pole "weather"
                    string conditions = "Neznámé";
                    if (root.TryGetProperty("weather", out JsonElement weatherArray) && weatherArray.GetArrayLength() > 0)
                    {
                        conditions = weatherArray[0].GetProperty("description").GetString();
                    }

                    string info = $"Počasí v {city}:\n" +
                                $"Teplota: {temp} °C\n" +
                                $"Podmínky: {conditions}\n" +
                                $"Vlhkost: {humidity} %";

                    MessageBox.Show(mainForm, info);
                }
                else
                {
                    MessageBox.Show(mainForm, "Nepodařilo se načíst aktuální počasí.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                MessageBox.Show(mainForm, $"HTTP chyba: {httpEx.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(mainForm, $"Chyba při získávání dat:\n{ex.Message}");
            }
        };


        app.Run(mainForm);
    }
}
