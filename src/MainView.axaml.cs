using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SpaceTraffic.Engine;
using SpaceTraffic.Model;
using SpaceTraffic.Parser;

namespace SpaceTraffic;

public partial class MainView : UserControl
{
    private readonly SimulationEngine _engine = new();
    private readonly DispatcherTimer _renderTimer = new();
    private readonly DispatcherTimer _simTimer = new();

    public MainView()
    {
        InitializeComponent();

        // Načtení parametru z příkazové řádky (např. default nebo complex)
        string scenarioName = "default";
        var args = Environment.GetCommandLineArgs();
        if (args.Length > 1 && !string.IsNullOrWhiteSpace(args[1]))
        {
            scenarioName = args[1];
        }

        // Vyhledání a načtení scénáře
        string scenarioPath = FindScenarioPath(scenarioName);
        var (bodies, destinations, ships) = ScenarioParser.Parse(scenarioPath);
        _engine.LoadScenario(bodies, destinations, ships);

        // Časovač pro plynulé překreslování (60 FPS)
        _renderTimer.Interval = TimeSpan.FromMilliseconds(16);
        _renderTimer.Tick += (s, e) => InvalidateVisual();
        _renderTimer.Start();

        // Simulační časovač (1 sekunda reálného času = 1 simulační den)
        _simTimer.Interval = TimeSpan.FromSeconds(1);
        _simTimer.Tick += (s, e) => _engine.ExecuteDiscreteStep();
        _simTimer.Start();
    }

    private string FindScenarioPath(string scenarioName)
    {
        var candidates = new List<string>
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", scenarioName),
            Path.Combine(Directory.GetCurrentDirectory(), "data", scenarioName)
        };

        // Prohledání nadřazených složek směrem nahoru k kořeni projektu
        string? current = AppDomain.CurrentDomain.BaseDirectory;
        for (int i = 0; i < 10 && current != null; i++)
        {
            candidates.Add(Path.Combine(current, "data", scenarioName));
            current = Directory.GetParent(current)?.FullName;
        }

        current = Directory.GetCurrentDirectory();
        for (int i = 0; i < 10 && current != null; i++)
        {
            candidates.Add(Path.Combine(current, "data", scenarioName));
            current = Directory.GetParent(current)?.FullName;
        }

        foreach (var path in candidates)
        {
            if (File.Exists(path))
                return path;
        }

        throw new FileNotFoundException(
            $"Scénář '{scenarioName}' nebyl nalezen!\nProhledané cesty:\n" + string.Join("\n", candidates.Distinct()));
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        double w = Bounds.Width;
        double h = Bounds.Height;

        // Pozadí
        context.FillRectangle(Brushes.DarkBlue, new Rect(Bounds.Size));

        if (_engine.Bodies.Count == 0) return;

        // Zachování poměru stran scény
        double maxRadius = _engine.Bodies.Max(b => b.OrbitRadius) * 2.4;
        if (maxRadius <= 0) maxRadius = 10.0;

        double scale = Math.Min(w / maxRadius, h / maxRadius);
        double centerX = w / 2.0;
        double centerY = h / 2.0;

        Point WorldToScreen(double x, double y) => new Point(centerX + x * scale, centerY - y * scale);

        // 1. Zobrazení simulačního času
        var timeText = new FormattedText(
            $"Simulační čas: {_engine.CurrentDate:dd. MM. yyyy}",
            CultureInfo.InvariantCulture, FlowDirection.LeftToRight,
            Typeface.Default, 15, Brushes.White);
        context.DrawText(timeText, new Point(15, 15));

        // 2. Vykreslení drah a těles
        foreach (var body in _engine.Bodies)
        {
            var center = WorldToScreen(body.X, body.Y);

            if (body.Parent != null)
            {
                var parentCenter = WorldToScreen(body.Parent.X, body.Parent.Y);
                double orbitPx = body.OrbitRadius * scale;
                context.DrawEllipse(null, new Pen(Brushes.Gray, 1, dashStyle: DashStyle.Dash), parentCenter, orbitPx, orbitPx);
            }

            double r = Math.Max(4, body.Size * 8);
            IBrush bodyBrush = body.Type switch
            {
                BodyType.Star => Brushes.Gold,
                BodyType.Planet => Brushes.DeepSkyBlue,
                BodyType.Moon => Brushes.LightGray,
                _ => Brushes.SaddleBrown
            };
            context.DrawEllipse(bodyBrush, null, center, r, r);

            // Popisek a teploměr zásoby s rozlišenými offsety
            string label = $"{body.Name} ({body.Warehouse})";
            var text = new FormattedText(label, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface.Default, 11, Brushes.Yellow);

            // Offsety podle typu tělesa pro přehlednost
            double textOffsetX = body.Type switch
            {
                BodyType.Moon => r + 4,
                BodyType.Asteroid => -(r + 30),
                _ => r + 5
            };

            double textOffsetY = body.Type switch
            {
                BodyType.Moon => 6,        // Měsíce posuneme dolů
                BodyType.Asteroid => -18,  // Asteroidy posuneme nahoru
                _ => -14                   // Planety a hvězdy mírně nad střed
            };

            double barOffsetY = textOffsetY + 14;

            context.DrawText(text, new Point(center.X + textOffsetX, center.Y + textOffsetY));

            double barW = 35;
            double barH = 4;
            double fillRatio = Math.Clamp(body.Warehouse / 500.0, 0.0, 1.0);
            context.DrawRectangle(Brushes.DimGray, null, new Rect(center.X + textOffsetX, center.Y + barOffsetY, barW, barH));
            context.DrawRectangle(Brushes.LimeGreen, null, new Rect(center.X + textOffsetX, center.Y + barOffsetY, barW * fillRatio, barH));
        }

        // 3. Vykreslení lodí a indikátorů
        foreach (var ship in _engine.Ships)
        {
            var pos = WorldToScreen(ship.X, ship.Y);

            context.DrawRectangle(Brushes.Cyan, null, new Rect(pos.X - 4, pos.Y - 4, 8, 8));

            string shipLabel = $"{ship.Name} [{ship.CurrentCargo}/{ship.Capacity}]";
            var shipText = new FormattedText(shipLabel, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface.Default, 10, Brushes.White);
            context.DrawText(shipText, new Point(pos.X + 8, pos.Y - 10));

            if (ship.State != ShipState.InTransit)
            {
                string arrow = ship.IsLoadingArrow ? "▲" : "▼";
                IBrush arrowBrush = ship.IsLoadingArrow ? Brushes.Lime : Brushes.Red;
                var arrowText = new FormattedText(arrow, CultureInfo.InvariantCulture, FlowDirection.LeftToRight, Typeface.Default, 13, arrowBrush);
                context.DrawText(arrowText, new Point(pos.X - 12, pos.Y - 8));
            }
        }
    }
}