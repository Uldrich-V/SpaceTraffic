using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using Avalonia.Threading;
using SpaceTraffic.Data;
using SpaceTraffic.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SpaceTraffic;

public partial class MainView : UserControl
{
    public Scenar? CurrentScenario { get; private set; }

    // Časovače
    private DispatcherTimer _simTimer;
    private DispatcherTimer _renderTimer;
    private DateTime _simDate = new DateTime(2026, 9, 21);
    private double _totalSimDays = 0;
    private Random _random = new Random();

    // Maximální poloměr celého systému (pro automatické škálování na okno)
    private double _maxSystemRadius = 1.0;

    private class TelesoVisual
    {
        public Ellipse OrbitLine { get; set; } = new();
        public Ellipse BodyShape { get; set; } = new();
        public TextBlock Label { get; set; } = new();
        public Rectangle WarehouseBg { get; set; } = new();
        public Rectangle WarehouseFg { get; set; } = new();
        public TextBlock WarehouseText { get; set; } = new();
    }

    private class LodVisual
    {
        public Polygon ShipShape { get; set; } = new();
        public TextBlock Label { get; set; } = new();
        public TextBlock StatusArrow { get; set; } = new();
    }

    private Dictionary<VesmirneTeleso, TelesoVisual> _bodyVisuals = new();
    private Dictionary<Lod, LodVisual> _shipVisuals = new();

    public MainView()
    {
        InitializeComponent();

        _simTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        _simTimer.Tick += SimTimer_Tick;

        _renderTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };
        _renderTimer.Tick += RenderTimer_Tick;

        // Pokud nebyl scénář načten přes NactiScenar, načteme výchozí
        if (CurrentScenario == null)
        {
            NactiScenar(Program.ScenarioName);
        }
    }

    public void NactiScenar(string scenarioName)
    {
        try
        {
            string filePath = System.IO.Path.Combine("data", scenarioName);
            CurrentScenario = NacteniScn.Load(filePath);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Chyba načítání scénáře: {ex.Message}");
            return;
        }

        SpoctiMaxPolomerSystemu();
        InicializujLode();
        VytvorPrvkySceny();

        _simTimer.Start();
        _renderTimer.Start();

        SimulationTimeText.Text = _simDate.ToString("dd. MM. yyyy");
    }

    private void SpoctiMaxPolomerSystemu()
    {
        if (CurrentScenario == null) return;

        double maxR = 0;
        foreach (var body in CurrentScenario.Bodies)
        {
            double cumR = GetCumulativeOrbitRadius(body);
            if (cumR > maxR) maxR = cumR;
        }

        // Pokud tělesa nemají oběžné dráhy nebo jsou moc malé, dáme min. hodnotu
        _maxSystemRadius = maxR > 0 ? maxR : 100.0;
    }

    private double GetCumulativeOrbitRadius(VesmirneTeleso body)
    {
        double r = body.OrbitRadius;
        if (!string.IsNullOrEmpty(body.ParentName) && CurrentScenario != null)
        {
            var parent = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == body.ParentName);
            if (parent != null) r += GetCumulativeOrbitRadius(parent);
        }
        return r;
    }

    private void InicializujLode()
    {
        if (CurrentScenario == null) return;
        foreach (var lod in CurrentScenario.Ships)
        {
            var startBody = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == lod.StartBodyName);
            if (startBody != null)
            {
                var (x, y) = VypoctiFyzikalniPozici(startBody, 0);
                lod.CurrentX = x;
                lod.CurrentY = y;
            }
            lod.Stav = LodStav.NAKLADKA;
            lod.TargetBodyName = lod.StartBodyName;
        }
    }

    private void VytvorPrvkySceny()
    {
        if (CurrentScenario == null) return;

        SpaceCanvas.Children.Clear();
        _bodyVisuals.Clear();
        _shipVisuals.Clear();

        // Tělesa
        foreach (var teleso in CurrentScenario.Bodies)
        {
            var visual = new TelesoVisual();

            if (teleso.OrbitRadius > 0)
            {
                visual.OrbitLine = new Ellipse
                {
                    Stroke = new SolidColorBrush(Color.Parse("#40FFFFFF")),
                    StrokeThickness = 1,
                    StrokeDashArray = new Avalonia.Collections.AvaloniaList<double> { 2, 2 }
                };
                SpaceCanvas.Children.Add(visual.OrbitLine);
            }

            IBrush fillBrush = teleso.Type.ToLower() switch
            {
                "star" => Brushes.Gold,
                "planet" => Brushes.DeepSkyBlue,
                "moon" => Brushes.LightGray,
                _ => Brushes.White
            };

            // Zajištění rozumné viditelné velikosti
            double drawSize = Math.Max(12, teleso.Size);

            visual.BodyShape = new Ellipse { Width = drawSize, Height = drawSize, Fill = fillBrush };
            SpaceCanvas.Children.Add(visual.BodyShape);

            visual.Label = new TextBlock
            {
                Text = teleso.Name,
                Foreground = Brushes.White,
                FontSize = 11,
                FontWeight = FontWeight.Bold
            };
            SpaceCanvas.Children.Add(visual.Label);

            visual.WarehouseBg = new Rectangle { Width = 32, Height = 4, Fill = Brushes.DarkGray };
            visual.WarehouseFg = new Rectangle { Width = 0, Height = 4, Fill = Brushes.LimeGreen };
            visual.WarehouseText = new TextBlock { Text = "0", Foreground = Brushes.LightGreen, FontSize = 10 };

            SpaceCanvas.Children.Add(visual.WarehouseBg);
            SpaceCanvas.Children.Add(visual.WarehouseFg);
            SpaceCanvas.Children.Add(visual.WarehouseText);

            _bodyVisuals.Add(teleso, visual);
        }

        // Lodě
        foreach (var lod in CurrentScenario.Ships)
        {
            var visual = new LodVisual();

            visual.ShipShape = new Polygon
            {
                Points = new List<Avalonia.Point> { new(0, -7), new(-6, 6), new(6, 6) },
                Fill = Brushes.Orange,
                Stroke = Brushes.White,
                StrokeThickness = 1
            };
            SpaceCanvas.Children.Add(visual.ShipShape);

            visual.Label = new TextBlock { Text = lod.Name, Foreground = Brushes.Orange, FontSize = 10 };
            visual.StatusArrow = new TextBlock { Text = "", Foreground = Brushes.Yellow, FontSize = 13, FontWeight = FontWeight.Bold };

            SpaceCanvas.Children.Add(visual.Label);
            SpaceCanvas.Children.Add(visual.StatusArrow);

            _shipVisuals.Add(lod, visual);
        }
    }

    private (double X, double Y) VypoctiFyzikalniPozici(VesmirneTeleso teleso, double simDays)
    {
        double uhelRad = (teleso.InitialAngle + teleso.AngularSpeed * simDays) * (Math.PI / 180.0);

        double lokX = teleso.OrbitRadius * Math.Cos(uhelRad);
        double lokY = -teleso.OrbitRadius * Math.Sin(uhelRad);

        if (!string.IsNullOrEmpty(teleso.ParentName) && CurrentScenario != null)
        {
            var parent = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == teleso.ParentName);
            if (parent != null)
            {
                var (pX, pY) = VypoctiFyzikalniPozici(parent, simDays);
                return (pX + lokX, pY + lokY);
            }
        }

        return (lokX, lokY);
    }

    private (double ScreenX, double ScreenY) FyzikalniNaObrazovkove(double physX, double physY)
    {
        double canvasW = SpaceCanvas.Bounds.Width > 0 ? SpaceCanvas.Bounds.Width : 800;
        double canvasH = SpaceCanvas.Bounds.Height > 0 ? SpaceCanvas.Bounds.Height : 600;

        // Odsazení od okrajů okna
        double margin = 80;

        // Měřítko spočtené tak, aby se celý systém vešel do okna a rozprostřel po ploše
        double scale = Math.Min((canvasW - 2 * margin) / (2 * _maxSystemRadius), (canvasH - 2 * margin) / (2 * _maxSystemRadius));

        double centerX = canvasW / 2.0;
        double centerY = canvasH / 2.0;

        return (centerX + physX * scale, centerY + physY * scale);
    }

    // --- SIMULACE ---

    private void SimTimer_Tick(object? sender, EventArgs e)
    {
        _simDate = _simDate.AddDays(1);
        SimulationTimeText.Text = _simDate.ToString("dd. MM. yyyy");

        if (CurrentScenario == null) return;

        foreach (var body in CurrentScenario.Bodies)
        {
            if (body.ProdMax > 0)
            {
                body.Warehouse += _random.Next(body.ProdMin, body.ProdMax + 1);
            }
        }

        foreach (var lod in CurrentScenario.Ships)
        {
            ProvedSimulaciLodi(lod);
        }
    }

    private void ProvedSimulaciLodi(Lod lod)
    {
        if (CurrentScenario == null) return;

        if (lod.Stav == LodStav.NA_CESTE)
        {
            var targetBody = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == lod.TargetBodyName);
            if (targetBody == null) return;

            var (targetX, targetY) = VypoctiFyzikalniPozici(targetBody, _totalSimDays);
            double dx = targetX - lod.CurrentX;
            double dy = targetY - lod.CurrentY;
            double vzdalenost = Math.Sqrt(dx * dx + dy * dy);

            if (vzdalenost <= lod.Speed)
            {
                lod.CurrentX = targetX;
                lod.CurrentY = targetY;
                lod.StartBodyName = lod.TargetBodyName;
                lod.Stav = LodStav.VYKLADKA;
            }
            else
            {
                lod.CurrentX += (dx / vzdalenost) * lod.Speed;
                lod.CurrentY += (dy / vzdalenost) * lod.Speed;
            }
        }
        else if (lod.Stav == LodStav.VYKLADKA)
        {
            var currentBody = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == lod.StartBodyName);
            if (lod.CurrentLoad > 0)
            {
                int vylozeno = Math.Min(lod.CurrentLoad, lod.TransferSpeed);
                lod.CurrentLoad -= vylozeno;
                if (currentBody != null) currentBody.Warehouse += vylozeno;
            }

            if (lod.CurrentLoad == 0)
            {
                lod.Stav = LodStav.NAKLADKA;
                lod.DobaCekaniDni = 0;
            }
        }
        else if (lod.Stav == LodStav.NAKLADKA)
        {
            var currentBody = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == lod.StartBodyName);
            if (currentBody != null)
            {
                int mozneNacist = Math.Min(lod.Capacity - lod.CurrentLoad, Math.Min(currentBody.Warehouse, lod.TransferSpeed));
                lod.CurrentLoad += mozneNacist;
                currentBody.Warehouse -= mozneNacist;
            }

            lod.DobaCekaniDni++;

            bool odletet = lod.Strategy switch
            {
                LoadStrategies.IMMEDIATE => true,
                LoadStrategies.FULL => lod.CurrentLoad >= lod.Capacity,
                LoadStrategies.MAX_LOADTIME => lod.CurrentLoad >= lod.Capacity || lod.DobaCekaniDni >= lod.MaxLoadTime,
                _ => true
            };

            if (odletet)
            {
                string dalsiCil = VyberDalsiDestinaci(lod.StartBodyName);
                if (!string.IsNullOrEmpty(dalsiCil))
                {
                    lod.TargetBodyName = dalsiCil;
                    lod.Stav = LodStav.NA_CESTE;
                    lod.DobaCekaniDni = 0;
                }
            }
        }
    }

    private string VyberDalsiDestinaci(string sourceBodyName)
    {
        if (CurrentScenario == null) return string.Empty;

        var mozne = CurrentScenario.Destinations.Where(d => d.Source == sourceBodyName).ToList();
        if (mozne.Count == 0) return sourceBodyName;

        int celkovaSuma = mozne.Sum(d => d.Probability);
        if (celkovaSuma <= 0) return mozne[0].Target;

        int hod = _random.Next(1, celkovaSuma + 1);
        int mezipoucet = 0;

        foreach (var dest in mozne)
        {
            mezipoucet += dest.Probability;
            if (hod <= mezipoucet) return dest.Target;
        }

        return mozne[0].Target;
    }

    // --- VYKRESLOVÁNÍ (~60 FPS) ---

    private void RenderTimer_Tick(object? sender, EventArgs e)
    {
        _totalSimDays += 0.016;

        if (CurrentScenario == null) return;

        double canvasW = SpaceCanvas.Bounds.Width > 0 ? SpaceCanvas.Bounds.Width : 800;
        double canvasH = SpaceCanvas.Bounds.Height > 0 ? SpaceCanvas.Bounds.Height : 600;
        double margin = 80;
        double scale = Math.Min((canvasW - 2 * margin) / (2 * _maxSystemRadius), (canvasH - 2 * margin) / (2 * _maxSystemRadius));

        // 1. Tělesa
        foreach (var body in CurrentScenario.Bodies)
        {
            if (!_bodyVisuals.TryGetValue(body, out var vis)) continue;

            var (physX, physY) = VypoctiFyzikalniPozici(body, _totalSimDays);
            var (screenX, screenY) = FyzikalniNaObrazovkove(physX, physY);

            // Oběžná dráha
            if (vis.OrbitLine != null && !string.IsNullOrEmpty(body.ParentName))
            {
                var parent = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == body.ParentName);
                if (parent != null)
                {
                    var (pX, pY) = VypoctiFyzikalniPozici(parent, _totalSimDays);
                    var (parentSX, parentSY) = FyzikalniNaObrazovkove(pX, pY);

                    double orbitDiameter = body.OrbitRadius * 2 * scale;
                    vis.OrbitLine.Width = orbitDiameter;
                    vis.OrbitLine.Height = orbitDiameter;
                    Canvas.SetLeft(vis.OrbitLine, parentSX - orbitDiameter / 2);
                    Canvas.SetTop(vis.OrbitLine, parentSY - orbitDiameter / 2);
                }
            }

            double drawSize = Math.Max(12, body.Size);

            Canvas.SetLeft(vis.BodyShape, screenX - drawSize / 2);
            Canvas.SetTop(vis.BodyShape, screenY - drawSize / 2);

            Canvas.SetLeft(vis.Label, screenX + drawSize / 2 + 4);
            Canvas.SetTop(vis.Label, screenY - 14);

            double fillRatio = Math.Min(1.0, body.Warehouse / 300.0);
            Canvas.SetLeft(vis.WarehouseBg, screenX - 16);
            Canvas.SetTop(vis.WarehouseBg, screenY + drawSize / 2 + 4);

            vis.WarehouseFg.Width = 32 * fillRatio;
            Canvas.SetLeft(vis.WarehouseFg, screenX - 16);
            Canvas.SetTop(vis.WarehouseFg, screenY + drawSize / 2 + 4);

            vis.WarehouseText.Text = $"{body.Warehouse}";
            Canvas.SetLeft(vis.WarehouseText, screenX - 16);
            Canvas.SetTop(vis.WarehouseText, screenY + drawSize / 2 + 9);
        }

        // 2. Lodě
        foreach (var lod in CurrentScenario.Ships)
        {
            if (!_shipVisuals.TryGetValue(lod, out var vis)) continue;

            double physX = lod.CurrentX;
            double physY = lod.CurrentY;

            if (lod.Stav != LodStav.NA_CESTE)
            {
                var dockedBody = CurrentScenario.Bodies.FirstOrDefault(b => b.Name == lod.StartBodyName);
                if (dockedBody != null)
                {
                    (physX, physY) = VypoctiFyzikalniPozici(dockedBody, _totalSimDays);
                    lod.CurrentX = physX;
                    lod.CurrentY = physY;
                }
            }

            var (screenX, screenY) = FyzikalniNaObrazovkove(physX, physY);

            if (lod.Stav != LodStav.NA_CESTE) screenY -= 16;

            Canvas.SetLeft(vis.ShipShape, screenX);
            Canvas.SetTop(vis.ShipShape, screenY);

            vis.Label.Text = $"{lod.Name} ({lod.CurrentLoad}/{lod.Capacity})";
            Canvas.SetLeft(vis.Label, screenX + 10);
            Canvas.SetTop(vis.Label, screenY - 7);

            vis.StatusArrow.Text = lod.Stav switch
            {
                LodStav.VYKLADKA => "↓",
                LodStav.NAKLADKA => "↑",
                _ => ""
            };
            Canvas.SetLeft(vis.StatusArrow, screenX - 14);
            Canvas.SetTop(vis.StatusArrow, screenY - 9);
        }
    }
}