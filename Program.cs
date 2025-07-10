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
        string apiKey = "ad2ca6f113cbb990beb256aafaf1e7a1"; // secret api key, don't look!

        var label = new Label { Text = "Zadej město:" }; // tohle nastavuje variable
        var textBox = new TextBox { Width = 50, Height = 10 };
        var button = new Button { Text = "Zjisti počasí" };

        var fieldCity = new Label { Text = "Město" };
        var fieldCountry = new Label { Text = "Země" };
        var fieldConditions = new Label { Text = "Podmínky" };
        var fieldTemp = new Label { Text = "Teplota" };
        var filedHumidity = new Label { Text = "Vlhkost" };


        var layout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = 10 };
        layout.AddRow(label);
        layout.AddRow(textBox);
        layout.AddRow(fieldCity);
        layout.AddRow(fieldCountry);
        layout.AddRow(fieldConditions);
        layout.AddRow(fieldTemp);
        layout.AddRow(filedHumidity);
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
            string country = "";
            double temp = 0;
            string conditions = "";
            int humidity = 0;

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

                if (root.TryGetProperty("main", out JsonElement main) & root.TryGetProperty("sys", out JsonElement sys))
                { 
                    temp = main.GetProperty("temp").GetDouble();
                    humidity = main.GetProperty("humidity").GetInt32();
                    country = sys.GetProperty("country").GetString();

                    conditions = "Neznámé";
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
            

            fieldCity.Text = $"Město: {city}";
            fieldCountry.Text = $"Země: {country}";
            fieldConditions.Text = $"Podmínky: {conditions}";
            fieldTemp.Text = $"Teplota: {temp}°C";
            filedHumidity.Text = $"Vlhkost: {humidity}%";
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
