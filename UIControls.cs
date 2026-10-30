using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace ModernWeatherApp;

public static class UIHelper
{
    public static GraphicsPath GetRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = radius * 2;
        if (d <= 0)
        {
            path.AddRectangle(rect);
            return path;
        }

        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}

public class GlassCard : Panel
{
    public int CornerRadius { get; set; } = 16;
    public Color CardBackColor { get; set; } = Color.FromArgb(28, 255, 255, 255);
    public Color BorderColor { get; set; } = Color.FromArgb(45, 255, 255, 255);
    public int BorderThickness { get; set; } = 1;

    public GlassCard()
    {
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Padding = new Padding(14);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = UIHelper.GetRoundedPath(rect, CornerRadius);

        using (var brush = new SolidBrush(CardBackColor))
        {
            e.Graphics.FillPath(brush, path);
        }

        if (BorderThickness > 0)
        {
            using var pen = new Pen(BorderColor, BorderThickness);
            e.Graphics.DrawPath(pen, path);
        }

        base.OnPaint(e);
    }
}

public class ModernButton : Button
{
    private readonly Color _normalBg;
    private readonly Color _hoverBg;
    private readonly int _radius;
    private bool _isHovered;
    private bool _isPressed;

    public ModernButton(string text, Color bg, Color hover, float fontSize = 10.5F, int radius = 10)
    {
        Text = text;
        _normalBg = bg;
        _hoverBg = hover;
        _radius = radius;
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI Semibold", fontSize, FontStyle.Bold);
        Cursor = Cursors.Hand;
        TabStop = false;
        DoubleBuffered = true;

        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; _isPressed = false; Invalidate(); };
        MouseDown += (s, e) => { _isPressed = true; Invalidate(); };
        MouseUp += (s, e) => { _isPressed = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        Color currentBg = _isPressed ? Color.FromArgb(
            Math.Max(0, _hoverBg.R - 25),
            Math.Max(0, _hoverBg.G - 25),
            Math.Max(0, _hoverBg.B - 25)) : (_isHovered ? _hoverBg : _normalBg);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);

        using var path = UIHelper.GetRoundedPath(rect, _radius);
        using var brush = new SolidBrush(currentBg);
        e.Graphics.FillPath(brush, path);

        Color textCol = Enabled ? ForeColor : Color.FromArgb(120, 120, 120);
        TextRenderer.DrawText(e.Graphics, Text, Font, rect, textCol,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

public class ChipButton : Panel
{
    private readonly int _radius = 14;
    private bool _isHovered;
    public bool IsActive { get; set; }

    public ChipButton(string text)
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        Text = text;
        Cursor = Cursors.Hand;
        Size = new Size(86, 28);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);
        ForeColor = Color.White;

        MouseEnter += (s, e) => { _isHovered = true; Invalidate(); };
        MouseLeave += (s, e) => { _isHovered = false; Invalidate(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        Color bg = IsActive
            ? Color.White
            : (_isHovered ? Color.FromArgb(70, 255, 255, 255) : Color.FromArgb(35, 255, 255, 255));

        Color border = IsActive
            ? Color.White
            : (_isHovered ? Color.FromArgb(140, 255, 255, 255) : Color.FromArgb(60, 255, 255, 255));

        Color textCol = IsActive
            ? Color.FromArgb(20, 100, 185)
            : Color.White;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = UIHelper.GetRoundedPath(rect, _radius);

        using (var brush = new SolidBrush(bg))
            e.Graphics.FillPath(brush, path);

        using (var pen = new Pen(border, 1))
            e.Graphics.DrawPath(pen, path);

        using var font = new Font("Segoe UI", 9.5F, IsActive ? FontStyle.Bold : FontStyle.Regular);
        TextRenderer.DrawText(e.Graphics, Text, font, rect, textCol,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}

/// <summary>
/// A sleek segmented toggle pill switch: [ °C | °F ]
/// </summary>
public class UnitToggleSwitch : Control
{
    private bool _isCelsius = true;

    public bool IsCelsius
    {
        get => _isCelsius;
        set
        {
            if (_isCelsius != value)
            {
                _isCelsius = value;
                Invalidate();
                UnitChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler? UnitChanged;

    public UnitToggleSwitch()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        Size = new Size(100, 38);
        Cursor = Cursors.Hand;
        DoubleBuffered = true;
        BackColor = Color.Transparent;
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        // Left half is °C, right half is °F
        if (e.X < Width / 2)
        {
            IsCelsius = true;
        }
        else
        {
            IsCelsius = false;
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        e.Graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        var bounds = new Rectangle(0, 0, Width - 1, Height - 1);
        int radius = Height / 2;

        // Outer pill background
        using (var trackPath = UIHelper.GetRoundedPath(bounds, radius))
        {
            using var trackBrush = new SolidBrush(Color.FromArgb(45, 255, 255, 255));
            e.Graphics.FillPath(trackBrush, trackPath);

            using var trackPen = new Pen(Color.FromArgb(80, 255, 255, 255), 1);
            e.Graphics.DrawPath(trackPen, trackPath);
        }

        // Active indicator pill
        int pillPadding = 3;
        int halfWidth = (Width - pillPadding * 2) / 2;
        int pillX = _isCelsius ? pillPadding : pillPadding + halfWidth;
        var pillRect = new Rectangle(pillX, pillPadding, halfWidth, Height - pillPadding * 2 - 1);
        int pillRadius = (pillRect.Height) / 2;

        using (var pillPath = UIHelper.GetRoundedPath(pillRect, pillRadius))
        {
            using var pillBrush = new SolidBrush(Color.White);
            e.Graphics.FillPath(pillBrush, pillPath);
        }

        // Text Labels
        var leftRect = new Rectangle(0, 0, Width / 2, Height);
        var rightRect = new Rectangle(Width / 2, 0, Width / 2, Height);

        using var fontBold = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
        using var fontNormal = new Font("Segoe UI", 10F, FontStyle.Regular);

        Color activeText = Color.FromArgb(20, 100, 185);
        Color inactiveText = Color.FromArgb(240, 248, 255);

        TextRenderer.DrawText(e.Graphics, "°C", _isCelsius ? fontBold : fontNormal, leftRect,
            _isCelsius ? activeText : inactiveText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

        TextRenderer.DrawText(e.Graphics, "°F", !_isCelsius ? fontBold : fontNormal, rightRect,
            !_isCelsius ? activeText : inactiveText,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}


public enum WeatherAnimationMode
{
    Sun,
    Rain,
    Cloudy,
    Night
}

/// <summary>
/// A modern animated 60 FPS canvas rendering living weather effects:
/// - Sun: Rotating corona rays, warm pulsing halo, glowing gradient sphere
/// - Rain: Dynamic angled falling raindrops, splash ripples on the ground, layered clouds
/// - Night: Glowing crescent moon with twinkling stars
/// - Cloudy: Drifting ambient layered clouds
/// </summary>
public class WeatherAnimationView : Panel
{
    private readonly System.Windows.Forms.Timer _animTimer;
    private float _time;
    private WeatherAnimationMode _mode = WeatherAnimationMode.Sun;

    // Sun state
    private float _sunAngle = 0f;
    private float _sunPulse = 0f;

    // Rain simulation
    private readonly List<RainDrop> _raindrops = new();
    private readonly List<SplashRipple> _ripples = new();
    private readonly Random _rand = new();

    public WeatherAnimationMode Mode
    {
        get => _mode;
        set
        {
            if (_mode != value)
            {
                _mode = value;
                InitParticles();
                Invalidate();
            }
        }
    }

    public WeatherAnimationView()
    {
        SetStyle(ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.UserPaint |
                 ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
        DoubleBuffered = true;
        BackColor = Color.Transparent;
        Size = new Size(200, 160);

        InitParticles();

        _animTimer = new System.Windows.Forms.Timer { Interval = 20 }; // ~50 FPS
        _animTimer.Tick += (s, e) =>
        {
            _time += 0.03f;
            UpdateAnimation();
            Invalidate();
        };

        HandleCreated += (s, e) => _animTimer.Start();
        HandleDestroyed += (s, e) => _animTimer.Stop();
    }

    private void InitParticles()
    {
        _raindrops.Clear();
        _ripples.Clear();

        if (_mode == WeatherAnimationMode.Rain)
        {
            for (int i = 0; i < 45; i++)
            {
                _raindrops.Add(new RainDrop
                {
                    X = _rand.Next(-20, Width + 40),
                    Y = _rand.Next(-100, Height),
                    Speed = (float)(8.0 + _rand.NextDouble() * 7.0),
                    Length = (float)(12.0 + _rand.NextDouble() * 10.0),
                    Alpha = _rand.Next(140, 240),
                    Thickness = (float)(1.2 + _rand.NextDouble() * 0.9)
                });
            }
        }
    }

    private void UpdateAnimation()
    {
        if (_mode == WeatherAnimationMode.Sun)
        {
            _sunAngle += 0.55f;
            if (_sunAngle >= 360f) _sunAngle -= 360f;
            _sunPulse = (float)(Math.Sin(_time * 2.0) * 0.5 + 0.5); // 0 to 1
        }
        else if (_mode == WeatherAnimationMode.Rain)
        {
            float groundY = Height - 14;

            for (int i = 0; i < _raindrops.Count; i++)
            {
                var drop = _raindrops[i];
                drop.Y += drop.Speed;
                drop.X += 1.4f; // Wind slant

                if (drop.Y >= groundY)
                {
                    // Spawn ripple splash
                    if (_ripples.Count < 25)
                    {
                        _ripples.Add(new SplashRipple
                        {
                            X = drop.X,
                            Y = groundY + _rand.Next(-2, 4),
                            Radius = 1.5f,
                            MaxRadius = (float)(5.0 + _rand.NextDouble() * 5.0),
                            Alpha = 190
                        });
                    }

                    // Reset raindrop to top
                    drop.Y = _rand.Next(-40, -5);
                    drop.X = _rand.Next(-20, Width + 20);
                    drop.Speed = (float)(8.0 + _rand.NextDouble() * 7.0);
                }
            }

            // Update ripples
            for (int i = _ripples.Count - 1; i >= 0; i--)
            {
                var rip = _ripples[i];
                rip.Radius += 0.45f;
                rip.Alpha -= 8;
                if (rip.Alpha <= 0 || rip.Radius >= rip.MaxRadius)
                {
                    _ripples.RemoveAt(i);
                }
            }
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

        switch (_mode)
        {
            case WeatherAnimationMode.Sun:
                DrawSun(e.Graphics);
                break;
            case WeatherAnimationMode.Rain:
                DrawRain(e.Graphics);
                break;
            case WeatherAnimationMode.Night:
                DrawNight(e.Graphics);
                break;
            default:
                DrawCloudy(e.Graphics);
                break;
        }
    }

    private void DrawSun(Graphics g)
    {
        float cx = Width / 2f;
        float cy = Height / 2f + 4;
        float coreRadius = 35f;

        // 1. Soft Warm Ambient Glow (Pulsing Corona)
        float glowRadius = coreRadius + 22f + (_sunPulse * 8f);
        using (var glowPath = new GraphicsPath())
        {
            glowPath.AddEllipse(cx - glowRadius, cy - glowRadius, glowRadius * 2, glowRadius * 2);
            using var pgb = new PathGradientBrush(glowPath)
            {
                CenterColor = Color.FromArgb((int)(80 + _sunPulse * 45), 255, 195, 0),
                SurroundColors = new[] { Color.FromArgb(0, 255, 160, 0) }
            };
            g.FillPath(pgb, glowPath);
        }

        // 2. Rotating Tapered Solar Rays
        int rayCount = 12;
        float rayInner = coreRadius + 7f;
        float rayOuter = rayInner + 15f + (_sunPulse * 4f);

        for (int i = 0; i < rayCount; i++)
        {
            float deg = _sunAngle + (i * (360f / rayCount));
            double rad = deg * Math.PI / 180.0;

            float x1 = cx + (float)(Math.Cos(rad) * rayInner);
            float y1 = cy + (float)(Math.Sin(rad) * rayInner);
            float x2 = cx + (float)(Math.Cos(rad) * rayOuter);
            float y2 = cy + (float)(Math.Sin(rad) * rayOuter);

            using var rayPen = new Pen(Color.FromArgb((int)(200 + _sunPulse * 50), 255, 205, 45), 3.2f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLine(rayPen, x1, y1, x2, y2);
        }

        // 3. Vibrant Sun Sphere with Radial Gradient
        using (var sunPath = new GraphicsPath())
        {
            sunPath.AddEllipse(cx - coreRadius, cy - coreRadius, coreRadius * 2, coreRadius * 2);
            using var sunBrush = new PathGradientBrush(sunPath)
            {
                CenterPoint = new PointF(cx - coreRadius * 0.35f, cy - coreRadius * 0.35f),
                CenterColor = Color.FromArgb(255, 255, 230),
                SurroundColors = new[] { Color.FromArgb(255, 150, 0) }
            };
            g.FillPath(sunBrush, sunPath);
        }
    }

    private void DrawRain(Graphics g)
    {
        // 1. Draw top storm clouds from which rain pours
        DrawStormClouds(g, Width / 2f - 10, 36, 1.1f);

        // 2. Draw Falling Raindrops
        foreach (var drop in _raindrops)
        {
            float slantX = drop.X + (drop.Length * 0.18f);
            float endY = drop.Y + drop.Length;

            using var rainPen = new Pen(Color.FromArgb(drop.Alpha, 140, 205, 255), drop.Thickness)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLine(rainPen, drop.X, drop.Y, slantX, endY);
        }

        // 3. Draw Splash Ripples on ground
        foreach (var rip in _ripples)
        {
            using var ripPen = new Pen(Color.FromArgb(rip.Alpha, 150, 215, 255), 1.2f);
            g.DrawEllipse(ripPen, rip.X - rip.Radius, rip.Y - (rip.Radius * 0.35f), rip.Radius * 2, rip.Radius * 0.7f);
        }
    }

    private void DrawStormClouds(Graphics g, float cx, float cy, float scale)
    {
        using var cloudBrush = new LinearGradientBrush(
            new RectangleF(cx - 70 * scale, cy - 35 * scale, 140 * scale, 70 * scale),
            Color.FromArgb(220, 140, 160, 185),
            Color.FromArgb(240, 75, 90, 115),
            LinearGradientMode.Vertical
        );

        // 3 overlapping cloud lobes
        g.FillEllipse(cloudBrush, cx - 55 * scale, cy - 8 * scale, 55 * scale, 42 * scale);
        g.FillEllipse(cloudBrush, cx - 24 * scale, cy - 28 * scale, 65 * scale, 60 * scale);
        g.FillEllipse(cloudBrush, cx + 10 * scale, cy - 10 * scale, 50 * scale, 40 * scale);
    }

    private void DrawNight(Graphics g)
    {
        float cx = Width / 2f + 10;
        float cy = Height / 2f + 5;
        float r = 36f;

        // Twinkling stars
        DrawStar(g, cx - 60, cy - 35, 0.4f + (float)Math.Sin(_time * 3) * 0.3f);
        DrawStar(g, cx - 35, cy + 30, 0.5f + (float)Math.Sin(_time * 2.5 + 1) * 0.3f);
        DrawStar(g, cx + 55, cy - 25, 0.6f + (float)Math.Sin(_time * 2.2 + 2) * 0.3f);

        // Soft Moon Glow
        using (var glowPath = new GraphicsPath())
        {
            glowPath.AddEllipse(cx - r - 16, cy - r - 16, (r + 16) * 2, (r + 16) * 2);
            using var pgb = new PathGradientBrush(glowPath)
            {
                CenterColor = Color.FromArgb(70, 200, 230, 255),
                SurroundColors = new[] { Color.FromArgb(0, 150, 190, 255) }
            };
            g.FillPath(pgb, glowPath);
        }

        // Crescent Moon
        using var moonPath = new GraphicsPath();
        moonPath.AddEllipse(cx - r, cy - r, r * 2, r * 2);
        using var cutPath = new GraphicsPath();
        cutPath.AddEllipse(cx - r + 16, cy - r - 8, r * 2, r * 2);

        var moonRegion = new Region(moonPath);
        moonRegion.Exclude(cutPath);

        using (var moonBrush = new LinearGradientBrush(
            new RectangleF(cx - r, cy - r, r * 2, r * 2),
            Color.FromArgb(255, 255, 240),
            Color.FromArgb(200, 225, 255),
            LinearGradientMode.ForwardDiagonal))
        {
            g.FillRegion(moonBrush, moonRegion);
        }
    }

    private static void DrawStar(Graphics g, float x, float y, float alphaNorm)
    {
        int alpha = Math.Clamp((int)(alphaNorm * 255), 40, 255);
        using var pen = new Pen(Color.FromArgb(alpha, 255, 255, 255), 1.6f);
        g.DrawLine(pen, x - 3, y, x + 3, y);
        g.DrawLine(pen, x, y - 3, x, y + 3);
    }

    private void DrawCloudy(Graphics g)
    {
        float floatY = (float)Math.Sin(_time * 1.5) * 4f;
        DrawStormClouds(g, Width / 2f, Height / 2f + floatY, 1.25f);
    }

    private class RainDrop
    {
        public float X, Y, Speed, Length, Thickness;
        public int Alpha;
    }

    private class SplashRipple
    {
        public float X, Y, Radius, MaxRadius;
        public int Alpha;
    }
}
