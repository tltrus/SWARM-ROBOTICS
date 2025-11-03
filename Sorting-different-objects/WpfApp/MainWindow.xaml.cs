using System.Windows;
using System.Windows.Media;
using WpfApp.Classes;


namespace WpfApp;


public partial class MainWindow : Window
{
    System.Windows.Threading.DispatcherTimer timer, timer2;
    public static Random rnd = new Random();
    public static int width, height;

    public static DrawingVisual visual;
    DrawingContext dc;
    List<Robot> robots;
    List<Particle> particles;
    int k1, k2, numR, numI;


    public MainWindow()
    {
        InitializeComponent();

        visual = new DrawingVisual();

        width = (int)g.Width;
        height = (int)g.Height;

        timer = new System.Windows.Threading.DispatcherTimer();
        timer.Tick += new EventHandler(timerTick);
        timer.Interval = new TimeSpan(0, 0, 0, 0, 10);

        timer2 = new System.Windows.Threading.DispatcherTimer();
        timer2.Tick += new EventHandler(timer2Tick);
        timer2.Interval = new TimeSpan(0, 0, 0, 2, 0);

        Init();
    }

    private void Init()
    {
        robots = new List<Robot>();
        particles = new List<Particle>();

        k1 = (int)slK1.Value;
        k2 = (int)slK2.Value;
        numR = (int)slRobots.Value;
        numI = (int)slItems.Value;

        for (int i = 0; i < numI / 2; ++i)
        {
            var pos = new Vector2D(rnd.Next(width), rnd.Next(height));
            particles.Add(new Particle(i, pos, ParticleType.Type1));
        }

        for (int i = numI / 2; i < numI; ++i)
        {
            var pos = new Vector2D(rnd.Next(width), rnd.Next(height));
            particles.Add(new Particle(i, pos, ParticleType.Type2));
        }

        for (int i = 0; i < numR; ++i)
            robots.Add(new Robot());

        sl1Value.Content = k1;
        sl2Value.Content = k2;
        slRValue.Content = numR;
        slItemsValue.Content = numI;

        timer.Start();
        timer2.Start();
    }

    private void slK1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        k1 = (int)slK1.Value;
        if (sl1Value is not null) sl1Value.Content = k1;
    }

    private void slK2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        k2 = (int)slK2.Value;
        if (sl2Value is not null) sl2Value.Content = k2;
    }

    private void slRobots_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        numR = (int)slRobots.Value;
        if (slRValue is not null) slRValue.Content = numR;
    }

    private void slItems_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        numI = (int)slItems.Value;
        if (slItemsValue is not null) slItemsValue.Content = numI;
    }

    private void timerTick(object sender, EventArgs e) => Drawing();
    private void timer2Tick(object sender, EventArgs e)
    {
        foreach (var robot in robots)
        {
            if (robot.visitedIndexes.Count > 0)
                robot.visitedIndexes.Dequeue();
        }
    }

    private void Drawing()
    {
        g.RemoveVisual(visual);

        using (dc = visual.RenderOpen())
        {
            foreach (var p in particles)
                p.Draw(dc);

            foreach (var r in robots)
            {
                r.isDrawRadar = chbDraw.IsChecked.Value;
                r.Behaviors(particles);
                r.Update();
                r.Draw(dc);
            }

            dc.Close();
            g.AddVisual(visual);
        }
    }

    private void btnUpdate_Click(object sender, RoutedEventArgs e)
    {
        timer.Stop();
        timer2.Stop();

        Init();
    }
}
