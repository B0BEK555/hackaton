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
        var app = new Application(Eto.Platforms.Gtk); // nebo WinForms/Wpf podle platformy
        string apiKey = "ad2ca6f113cbb990beb256aafaf1e7a1";

        // Vstupy a výstupy vlevo
        var label = new Label { Text = "Zadej město:" };
        var textBox = new TextBox { Width = 200 };
        var buttonCheck = new Button { Text = "Check", Width = 200 };

        var fieldCity = new Label { Text = "Město:" };
        var fieldCountry = new Label { Text = "Země:" };
        var fieldConditions = new Label { Text = "Podmínky:" };
        var fieldTemp = new Label { Text = "Teplota:" };
        var fieldFeelsLike = new Label { Text = " ↪ Připadá jako:" };
        var filedHumidity = new Label { Text = "Vlhkost:" };
        var fieldWindSpeed = new Label { Text = "Rychlost výtru:" };
        var fieldWindStep = new Label { Text = " ↪ Beaufortova stup.:" };

        // Mapa na vpravo
        var mapView = new WebView
        {
            Size = new Size(500, 400),
            Visible = false
        };

        // Levý layout (text, tlačítko, outputy atd...)
        var leftLayout = new DynamicLayout { DefaultSpacing = new Size(5, 5), Padding = 10 };
        leftLayout.Add(label);
        leftLayout.Add(textBox);
        leftLayout.Add(buttonCheck);
        leftLayout.Add(fieldCity);
        leftLayout.Add(fieldCountry);
        leftLayout.Add(fieldConditions);
        leftLayout.Add(fieldTemp);
        leftLayout.Add(fieldFeelsLike);
        leftLayout.Add(filedHumidity);
        leftLayout.Add(fieldWindSpeed);
        leftLayout.Add(fieldWindStep);

        // Hlavní Layout
        var mainLayout = new DynamicLayout { DefaultSpacing = new Size(10, 10), Padding = 10 };
        mainLayout.BeginHorizontal();
        mainLayout.Add(leftLayout);
        mainLayout.Add(mapView);
        mainLayout.EndHorizontal();

        var mainForm = new Form
        {
            Title = "Weather App",
            ClientSize = new Size(800, 450),
            Resizable = false,
            Content = mainLayout
        };

        buttonCheck.Click += async (sender, e) =>
        {
            string city = textBox.Text.Trim();
            if (string.IsNullOrEmpty(city))
            {
                MessageBox.Show(mainForm, "Zadej město ty opičko.");
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

                string country = "";
                string cityName = "";
                double temp = 0;
                double feelsLike = 0;
                string conditions = "";
                int humidity = 0;
                double windSpeed = 0;
                string windStep = "";

                if (root.TryGetProperty("main", out JsonElement main) && root.TryGetProperty("sys", out JsonElement sys) && root.TryGetProperty("wind", out JsonElement wind) && root.TryGetProperty("name", out JsonElement name))
                {
                    cityName = name.GetString();
                    temp = main.GetProperty("temp").GetDouble();
                    feelsLike = main.GetProperty("feels_like").GetDouble();
                    humidity = main.GetProperty("humidity").GetInt32();
                    country = sys.GetProperty("country").GetString();
                    windSpeed = wind.GetProperty("speed").GetDouble();
                    if (windSpeed <= 0.2)
                    {
                        windStep = "bezvětřé";
                    }
                    else if (windSpeed >= 0.3)
                    {
                        windStep = "vánek";
                    }
                    else if (windSpeed >= 1.6)
                    {
                        windStep = "slabý vítr";
                    }
                    else if (windSpeed >= 3.4)
                    {
                        windStep = "mírný vítr";
                    }
                    else if (windSpeed >= 5.5)
                    {
                        windStep = "dosti čerstvý vítr";
                    }
                    else if (windSpeed >= 8)
                    {
                        windStep = "čerstvý vítr";
                    }
                    else if (windSpeed >= 10.8)
                    {
                        windStep = "silný vítr";
                    }
                    else if (windSpeed >= 13.9)
                    {
                        windStep = "prudký vítr";
                    }
                    else if (windSpeed >= 17.2)
                    {
                        windStep = "bouřlivý vítr";
                    }
                    else if (windSpeed >= 20.8)
                    {
                        windStep = "vichřice";
                    }
                    else if (windSpeed >= 24.5)
                    {
                        windStep = "silná vichřice";
                    }
                    else if (windSpeed >= 28.5)
                    {
                        windStep = "mohutná vichřice";
                    }
                    else if (windSpeed >= 32.7)
                    {
                        windStep = "orkán";
                    }

                    conditions = "Neznámé";
                    if (root.TryGetProperty("weather", out JsonElement weatherArray) && weatherArray.GetArrayLength() > 0)
                    {
                        var description = weatherArray[0].GetProperty("description").GetString()?.ToLower() ?? "";
                        string emoji = "";

                        if (description.Contains("jasno"))
                            emoji = "☀️";
                        else if (description.Contains("polojasno"))
                            emoji = "🌤️";
                        else if (description.Contains("oblačno") || description.Contains("zataženo"))
                            emoji = description.Contains("zataženo") ? "☁️" : "⛅";
                        else if (description.Contains("déšť") || description.Contains("přeháňky"))
                            emoji = "🌧️";
                        else if (description.Contains("mlha"))
                            emoji = "🌫️";
                        else if (description.Contains("sníh"))
                            emoji = "❄️";
                        else if (description.Contains("bouřka"))
                            emoji = "⛈️";

                        conditions = $"{emoji} {description}";
                    }

                    fieldCity.Text = $"Město: {cityName}";
                    fieldCountry.Text = $"Země: {country}";
                    fieldConditions.Text = $"Podmínky: {conditions}";
                    fieldTemp.Text = $"Teplota: {temp}°C";
                    fieldFeelsLike.Text = $" ↪ Připadá jako {feelsLike}°C";
                    filedHumidity.Text = $"Vlhkost: {humidity}%";
                    fieldWindSpeed.Text = $"Ryclost Větru: {windSpeed}m/s";
                    fieldWindStep.Text = $" ↪ Beaufortova stup.: {windStep}";
                }

                if (root.TryGetProperty("coord", out JsonElement coord))
                {
                    decimal lon = coord.GetProperty("lon").GetDecimal();
                    decimal lat = coord.GetProperty("lat").GetDecimal();

                    // Embed verze Windy mapy bez GUI
                    string mapUrl = $"https://embed.windy.com/embed2.html?lat={lat}&lon={lon}&detailLat={lat}&detailLon={lon}&width=500&height=400&zoom=11&level=surface&overlay=wind&menu=&message=false&marker=true&calendar=&pressure=&type=map&location=coordinates&detail=true";

                    mapView.Url = new Uri(mapUrl);
                    mapView.Visible = true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(mainForm, $"Chyba při načítání:\n{ex.Message}");
            }
        };

        app.Run(mainForm);
    }
}
