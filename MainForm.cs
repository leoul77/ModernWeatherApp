using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModernWeatherApp;

public class MainForm : Form
{
    private readonly WeatherService _weatherService = new();
    private WeatherInfo? _currentData;
    private bool _isCelsius = true;
    private string _activeCity = "London";

    // Vibrant Sky Blue theme gradients
    private Color _bgGradStart = Color.FromArgb(32, 125, 222);  // Rich sky blue
    private Color _bgGradEnd = Color.FromArgb(102, 188, 250);   // Bright horizon sky blue

    // Search & Control elements
    private readonly Panel searchContainer;
    private readonly TextBox txtSearch;
    private readonly ModernButton btnSearch;
    private readonly UnitToggleSwitch toggleUnit;
    private readonly Label lblStatus;
    private readonly List<ChipButton> _chipButtons = new();

    // Hero Card controls
    private readonly GlassCard heroCard;
    private readonly Label lblLocation;
    private readonly Label lblDate;
    private readonly Label lblCurrentTemp;
    private readonly Label lblConditionTag;
    private readonly Label lblTempSummary;
    private readonly Panel pnlActivityAdvice;
    private readonly Label lblActivityIcon;
    private readonly Label lblActivityText;
    private readonly WeatherAnimationView animationView;

    // 4 Metrics Cards
    private readonly GlassCard cardFeelsLike;
    private readonly GlassCard cardHumidity;
    private readonly GlassCard cardWind;
    private readonly GlassCard cardPrecip;
    private readonly Label lblFeelsLikeVal;
    private readonly Label lblHumidityVal;
    private readonly Label lblWindVal;
    private readonly Label lblPrecipVal;

    // Forecast container
    private readonly FlowLayoutPanel forecastPanel;

    public MainForm()
    {
        Text = "Modern Weather";
        ClientSize = new Size(960, 720);
        MinimumSize = new Size(960, 720);
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;
        DoubleBuffered = true;

        // ================= TOP BAR (Search & Controls) =================
        searchContainer = new Panel
        {
            Location = new Point(24, 20),
            Size = new Size(330, 40),
            BackColor = Color.FromArgb(50, 255, 255, 255)
        };
        searchContainer.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = UIHelper.GetRoundedPath(new Rectangle(0, 0, searchContainer.Width - 1, searchContainer.Height - 1), 12);
            using var pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1);
            e.Graphics.DrawPath(pen, path);
        };

        var lblSearchIcon = new Label
        {
            Text = "🔍",
            Location = new Point(8, 9),
            Size = new Size(24, 22),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Emoji", 10F),
            BackColor = Color.Transparent
        };
        searchContainer.Controls.Add(lblSearchIcon);

        txtSearch = new TextBox
        {
            Location = new Point(36, 10),
            Size = new Size(280, 22),
            BorderStyle = BorderStyle.None,
            BackColor = Color.FromArgb(52, 138, 225),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F),
            Text = "London"
        };
        txtSearch.KeyDown += async (s, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                await SearchCityAsync(txtSearch.Text);
            }
        };
        searchContainer.Controls.Add(txtSearch);
        Controls.Add(searchContainer);

        // Search Button (Crisp white pill button with sky-blue text)
        btnSearch = new ModernButton("Search", Color.White, Color.FromArgb(240, 246, 255), 10.5F, 12)
        {
            Location = new Point(364, 20),
            Size = new Size(90, 40),
            ForeColor = Color.FromArgb(20, 100, 185)
        };
        btnSearch.Click += async (s, e) => await SearchCityAsync(txtSearch.Text);
        Controls.Add(btnSearch);

        // Unit Switcher: Sleek Segmented [ °C | °F ] Pill Toggle
        toggleUnit = new UnitToggleSwitch
        {
            Location = new Point(836, 21),
            Size = new Size(100, 38),
            IsCelsius = true
        };
        toggleUnit.UnitChanged += (s, e) =>
        {
            _isCelsius = toggleUnit.IsCelsius;
            UpdateUI();
        };
        Controls.Add(toggleUnit);

        // Status Label (Loading / feedback)
        lblStatus = new Label
        {
            Location = new Point(465, 28),
            Size = new Size(360, 24),
            ForeColor = Color.FromArgb(240, 248, 255),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Italic),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.Transparent
        };
        Controls.Add(lblStatus);

        // City Quick Chips
        string[] quickCities = { "London", "Tokyo", "New York", "Paris", "Dubai", "Sydney" };
        int chipX = 24;
        foreach (var city in quickCities)
        {
            var chip = new ChipButton(city)
            {
                Location = new Point(chipX, 68),
                IsActive = city == _activeCity
            };
            string target = city;
            chip.Click += async (s, e) =>
            {
                txtSearch.Text = target;
                await SearchCityAsync(target);
            };
            _chipButtons.Add(chip);
            Controls.Add(chip);
            chipX += chip.Width + 8;
        }

        // ================= HERO CARD =================
        heroCard = new GlassCard
        {
            Location = new Point(24, 108),
            Size = new Size(912, 204),
            CornerRadius = 18,
            CardBackColor = Color.FromArgb(45, 255, 255, 255),
            BorderColor = Color.FromArgb(85, 255, 255, 255)
        };

        lblLocation = new Label
        {
            Location = new Point(24, 14),
            Size = new Size(520, 32),
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold),
            Text = "Loading...",
            BackColor = Color.Transparent
        };
        heroCard.Controls.Add(lblLocation);

        lblDate = new Label
        {
            Location = new Point(26, 46),
            Size = new Size(520, 20),
            ForeColor = Color.FromArgb(235, 245, 255),
            Font = new Font("Segoe UI", 9.5F),
            Text = "",
            BackColor = Color.Transparent
        };
        heroCard.Controls.Add(lblDate);

        // Current Temperature: AutoSize = true with generous space so numbers NEVER clip
        lblCurrentTemp = new Label
        {
            Location = new Point(22, 68),
            AutoSize = true,
            ForeColor = Color.White,
            Font = new Font("Segoe UI Variable Display", 46F, FontStyle.Bold),
            Text = "--°",
            BackColor = Color.Transparent
        };
        heroCard.Controls.Add(lblCurrentTemp);

        lblConditionTag = new Label
        {
            Location = new Point(245, 78),
            Size = new Size(380, 28),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 15F, FontStyle.Bold),
            Text = "",
            BackColor = Color.Transparent
        };
        heroCard.Controls.Add(lblConditionTag);

        lblTempSummary = new Label
        {
            Location = new Point(245, 110),
            Size = new Size(380, 22),
            ForeColor = Color.FromArgb(235, 245, 255),
            Font = new Font("Segoe UI", 10.5F),
            Text = "",
            BackColor = Color.Transparent
        };
        heroCard.Controls.Add(lblTempSummary);

        // Activity Advice Pill (Walking, Hiking, Staying in, etc.)
        pnlActivityAdvice = new Panel
        {
            Location = new Point(24, 154),
            Size = new Size(595, 36),
            BackColor = Color.FromArgb(55, 255, 255, 255)
        };
        pnlActivityAdvice.Paint += (s, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = UIHelper.GetRoundedPath(new Rectangle(0, 0, pnlActivityAdvice.Width - 1, pnlActivityAdvice.Height - 1), 10);
            using var pen = new Pen(Color.FromArgb(90, 255, 255, 255), 1);
            e.Graphics.DrawPath(pen, path);
        };

        lblActivityIcon = new Label
        {
            Text = "✨",
            Location = new Point(8, 7),
            Size = new Size(26, 22),
            Font = new Font("Segoe UI Emoji", 11F),
            ForeColor = Color.White,
            BackColor = Color.Transparent
        };
        pnlActivityAdvice.Controls.Add(lblActivityIcon);

        lblActivityText = new Label
        {
            Text = "Loading weather recommendations...",
            Location = new Point(36, 8),
            Size = new Size(550, 20),
            Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            AutoEllipsis = true
        };
        pnlActivityAdvice.Controls.Add(lblActivityText);

        heroCard.Controls.Add(pnlActivityAdvice);

        // Animated Canvas on the Right
        animationView = new WeatherAnimationView
        {
            Location = new Point(640, 14),
            Size = new Size(250, 175)
        };
        heroCard.Controls.Add(animationView);

        Controls.Add(heroCard);

        // ================= 4 METRICS CARDS =================
        int cardY = 324;
        int cardW = 217;
        int cardH = 86;
        int cardGap = 14;

        (cardFeelsLike, lblFeelsLikeVal) = CreateMetricCard("FEELS LIKE", "🌡️", 24, cardY, cardW, cardH);
        (cardHumidity, lblHumidityVal) = CreateMetricCard("HUMIDITY", "💧", 24 + (cardW + cardGap), cardY, cardW, cardH);
        (cardWind, lblWindVal) = CreateMetricCard("WIND SPEED", "💨", 24 + (cardW + cardGap) * 2, cardY, cardW, cardH);
        (cardPrecip, lblPrecipVal) = CreateMetricCard("PRECIPITATION", "🌧️", 24 + (cardW + cardGap) * 3, cardY, cardW, cardH);

        Controls.Add(cardFeelsLike);
        Controls.Add(cardHumidity);
        Controls.Add(cardWind);
        Controls.Add(cardPrecip);

        // ================= 7-DAY FORECAST =================
        var lblForecastHeading = new Label
        {
            Location = new Point(26, 422),
            Size = new Size(300, 24),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            Text = "7-Day Forecast",
            BackColor = Color.Transparent
        };
        Controls.Add(lblForecastHeading);

        forecastPanel = new FlowLayoutPanel
        {
            Location = new Point(24, 452),
            Size = new Size(912, 235),
            BackColor = Color.Transparent,
            WrapContents = false,
            AutoScroll = false
        };
        Controls.Add(forecastPanel);

        // Initial Load
        Shown += async (s, e) => await SearchCityAsync("London");
    }

    private (GlassCard Card, Label ValueLabel) CreateMetricCard(string title, string icon, int x, int y, int w, int h)
    {
        var card = new GlassCard
        {
            Location = new Point(x, y),
            Size = new Size(w, h),
            CornerRadius = 14,
            CardBackColor = Color.FromArgb(40, 255, 255, 255),
            BorderColor = Color.FromArgb(75, 255, 255, 255)
        };

        var lblTitle = new Label
        {
            Location = new Point(14, 12),
            Size = new Size(w - 28, 18),
            ForeColor = Color.FromArgb(220, 240, 255),
            Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold),
            Text = $"{icon}  {title}",
            BackColor = Color.Transparent
        };
        card.Controls.Add(lblTitle);

        var lblVal = new Label
        {
            Location = new Point(14, 34),
            Size = new Size(w - 28, 38),
            ForeColor = Color.White,
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            Text = "--",
            BackColor = Color.Transparent
        };
        card.Controls.Add(lblVal);

        return (card, lblVal);
    }

    private async Task SearchCityAsync(string city)
    {
        if (string.IsNullOrWhiteSpace(city)) return;

        string query = city.Trim();
        lblStatus.Text = $"Fetching live data for \"{query}\"...";
        btnSearch.Enabled = false;

        var data = await _weatherService.GetWeatherByCityAsync(query);

        btnSearch.Enabled = true;

        if (data == null)
        {
            lblStatus.Text = $"City \"{query}\" not found. Please try again.";
            return;
        }

        _activeCity = data.City;
        UpdateChipHighlights();

        lblStatus.Text = $"Updated at {DateTime.Now:hh:mm:ss tt}";
        _currentData = data;
        UpdateUI();
    }

    private void UpdateChipHighlights()
    {
        foreach (var chip in _chipButtons)
        {
            chip.IsActive = string.Equals(chip.Text, _activeCity, StringComparison.OrdinalIgnoreCase);
            chip.Invalidate();
        }
    }

    private double ConvertTemp(double c) => _isCelsius ? c : (c * 9.0 / 5.0) + 32;

    private string FormatTemp(double c) => $"{Math.Round(ConvertTemp(c))}°{(_isCelsius ? "C" : "F")}";

    private void UpdateUI()
    {
        if (_currentData == null) return;

        var d = _currentData;

        // Set Animation Mode & dynamic sky blue tint
        string condLower = d.Condition.ToLowerInvariant();
        if (condLower.Contains("rain") || condLower.Contains("drizzle") || condLower.Contains("shower") || condLower.Contains("thunder"))
        {
            animationView.Mode = WeatherAnimationMode.Rain;
            _bgGradStart = Color.FromArgb(46, 115, 185);
            _bgGradEnd = Color.FromArgb(92, 162, 222);
        }
        else if (d.IsDay && (condLower.Contains("sun") || condLower.Contains("clear")))
        {
            animationView.Mode = WeatherAnimationMode.Sun;
            _bgGradStart = Color.FromArgb(32, 125, 222);
            _bgGradEnd = Color.FromArgb(102, 188, 250);
        }
        else if (!d.IsDay && (condLower.Contains("clear") || condLower.Contains("night")))
        {
            animationView.Mode = WeatherAnimationMode.Night;
            _bgGradStart = Color.FromArgb(24, 85, 155);
            _bgGradEnd = Color.FromArgb(68, 138, 212);
        }
        else
        {
            animationView.Mode = WeatherAnimationMode.Cloudy;
            _bgGradStart = Color.FromArgb(42, 120, 195);
            _bgGradEnd = Color.FromArgb(95, 170, 235);
        }

        Invalidate(); // Repaint sky blue background gradient

        lblLocation.Text = string.IsNullOrEmpty(d.Country) ? d.City : $"{d.City}, {d.Country}";
        lblDate.Text = $"{DateTime.Now:dddd, MMMM d} • Live Weather";

        // Current Temp (Explicit unit, auto-sized so it NEVER clips)
        lblCurrentTemp.Text = $"{Math.Round(ConvertTemp(d.CurrentTemp))}°{(_isCelsius ? "C" : "F")}";

        // Dynamically align adjacent condition & summary tags next to the auto-sized temperature
        int tagLeft = Math.Max(lblCurrentTemp.Right + 16, 235);
        lblConditionTag.Location = new Point(tagLeft, 78);
        lblTempSummary.Location = new Point(tagLeft, 110);

        lblConditionTag.Text = $"{d.Icon}  {d.Condition}";
        lblTempSummary.Text = $"High: {FormatTemp(d.TodayMax)}  •  Low: {FormatTemp(d.TodayMin)}  •  Feels like {FormatTemp(d.FeelsLike)}";

        // Generate tailored activity advice (Walking, Hiking, Staying in, etc.)
        var (adviceIcon, adviceText) = GenerateActivityAdvice(d);
        lblActivityIcon.Text = adviceIcon;
        lblActivityText.Text = adviceText;

        lblFeelsLikeVal.Text = FormatTemp(d.FeelsLike);
        lblHumidityVal.Text = $"{d.Humidity}%";
        lblWindVal.Text = $"{d.WindSpeed:0.0} km/h";
        lblPrecipVal.Text = $"{d.Precipitation:0.0} mm";

        // Render 7-day forecast cards
        forecastPanel.Controls.Clear();
        int fCardW = (forecastPanel.Width - (10 * 6)) / 7;
        int fCardH = 220;

        foreach (var item in d.Forecast)
        {
            var fCard = new GlassCard
            {
                Size = new Size(fCardW, fCardH),
                Margin = new Padding(0, 0, 10, 0),
                CornerRadius = 14,
                CardBackColor = Color.FromArgb(38, 255, 255, 255),
                BorderColor = Color.FromArgb(75, 255, 255, 255)
            };

            var lblDay = new Label
            {
                Text = item.DayName,
                Location = new Point(0, 14),
                Size = new Size(fCardW, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblDay);

            var lblDateSub = new Label
            {
                Text = item.DateText,
                Location = new Point(0, 34),
                Size = new Size(fCardW, 16),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(230, 242, 255),
                Font = new Font("Segoe UI", 8F),
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblDateSub);

            var lblIcon = new Label
            {
                Text = item.Icon,
                Location = new Point(0, 54),
                Size = new Size(fCardW, 46),
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI Emoji", 23F),
                ForeColor = Color.White,
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblIcon);

            var lblCond = new Label
            {
                Text = item.Condition,
                Location = new Point(4, 106),
                Size = new Size(fCardW - 8, 32),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(240, 248, 255),
                Font = new Font("Segoe UI", 8F),
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblCond);

            var lblMax = new Label
            {
                Text = $"{Math.Round(ConvertTemp(item.MaxTemp))}°",
                Location = new Point(0, 144),
                Size = new Size(fCardW, 22),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                Font = new Font("Segoe UI Semibold", 12F, FontStyle.Bold),
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblMax);

            var lblMin = new Label
            {
                Text = $"{Math.Round(ConvertTemp(item.MinTemp))}°",
                Location = new Point(0, 168),
                Size = new Size(fCardW, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(220, 238, 255),
                Font = new Font("Segoe UI", 9.5F),
                BackColor = Color.Transparent
            };
            fCard.Controls.Add(lblMin);

            forecastPanel.Controls.Add(fCard);
        }
    }

    /// <summary>
    /// Generates tailored activity advice (Walking, Hiking, Staying in, Swimming, etc.)
    /// based on live temperature, precipitation, wind speed, and conditions.
    /// </summary>
    private static (string Icon, string Text) GenerateActivityAdvice(WeatherInfo d)
    {
        string cond = d.Condition.ToLowerInvariant();
        double temp = d.CurrentTemp; // Celsius
        double wind = d.WindSpeed;
        double rain = d.Precipitation;
        bool isDay = d.IsDay;

        // 1. Severe / Thunderstorm
        if (cond.Contains("thunder") || cond.Contains("severe"))
            return ("⚡", "Thunderstorms detected • Best to stay safely indoors today.");

        // 2. Heavy Rain
        if (cond.Contains("heavy rain") || rain > 3.0)
            return ("☕", "Heavy rain • Perfect day to stay in, read a book, or enjoy a hot drink!");

        // 3. Rain / Showers / Drizzle
        if (cond.Contains("rain") || cond.Contains("drizzle") || cond.Contains("shower") || rain > 0.4)
            return ("🌧️", "Rainy conditions • Great for staying indoors, or grab an umbrella for walks.");

        // 4. Freezing / Snow
        if (cond.Contains("snow") || temp < 0)
            return ("❄️", "Freezing weather • Bundle up for winter sports, or stay cozy inside!");

        // 5. Strong Wind
        if (wind > 32)
            return ("💨", $"Breezy ({wind:0.0} km/h) • Windbreaker recommended; hold onto your hat outdoors!");

        // 6. High Heat (> 30°C / 86°F)
        if (temp > 30)
            return ("🏊", "Hot & sunny • Great for swimming! Stay hydrated and avoid strenuous midday hikes.");

        // 7. Warm & Pleasant Day (18°C to 30°C)
        if (temp >= 18 && (cond.Contains("sun") || cond.Contains("clear") || cond.Contains("partly")))
        {
            return isDay
                ? ("🥾", "Gorgeous weather • Perfect for hiking, jogging, or a walk in the park!")
                : ("🌌", "Mild clear evening • Wonderful for an evening stroll or stargazing.");
        }

        // 8. Cool & Refreshing (10°C to 18°C)
        if (temp >= 10 && temp < 18)
        {
            return ("🚶", "Cool & refreshing • Excellent for a brisk neighborhood walk or hiking trails.");
        }

        // 9. Chilly / Overcast (0°C to 10°C)
        if (temp >= 0 && temp < 10)
        {
            return ("🧣", "Chilly outside • Put on a warm jacket for walks, or relax indoors.");
        }

        return ("✨", "Comfortable conditions • Good for casual outdoor walks and exploring.");
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        // Paint vibrant Sky Blue vertical horizon gradient
        using var brush = new LinearGradientBrush(
            ClientRectangle,
            _bgGradStart,
            _bgGradEnd,
            LinearGradientMode.Vertical
        );
        e.Graphics.FillRectangle(brush, ClientRectangle);
    }
}
