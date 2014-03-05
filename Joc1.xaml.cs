using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Media.Animation;
using NAudio.Wave;
using FaceTrackingBasics;
using System.Threading;

namespace FaceTrackingBasics
{
    /// <summary>
    /// Lógica de interacción para Juego1.xaml
    /// </summary>
    public partial class Joc1 : Window
    {
        private MainWindow mw;
  
        //Sensor Kinect
        private MyKinect kinect;
        private KinectSensor sensor;
        private FaceTracker faceTracker;
        private byte[] colorPixelData;
        private short[] depthPixelData;
        private Skeleton[] skeletonDataPle, skeletonData;

        //imatges
        private string[] dirs;
        private DateTime delayImatges;

        //so
        private string[] path;
        private Musica music;
        private WaveOut wav;
        private SineWaveOscillator oscilador;
        private PlaybackState state;

        // mida de la pantalla
        private double width, height, scrollWidth, scrollHeight;

        //jugador
        private E_Principal jugador;

        public Joc1(MainWindow mw, string player)
        {
            InitializeComponent();
            this.mw = mw;

            this.wav = new WaveOut();
            this.oscilador = new SineWaveOscillator(4400);
            this.wav.Init(oscilador);

            this.skeletonDataPle = obrirSkeletons(player);
            //mides de pantalla
            this.width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.height = System.Windows.SystemParameters.PrimaryScreenHeight;
            
            Fondo.Width = this.width;
            Fondo.Height = this.height;

            this.dirs = Directory.GetFiles(@"Images\Jocs\");
            foreach (string dir in dirs)
            {
                if (dir.EndsWith(".jpg") || dir.EndsWith(".png") || dir.EndsWith(".jpeg"))
                {
                    Image img = new Image();
                    img.Source = new BitmapImage(new Uri(dir, UriKind.Relative));
                    //obtenir les mides de la pantalla
                    this.scrollWidth += img.Width = this.width;
                    this.scrollHeight = img.Height = this.height;
                    //afegir les imatges
                    wrapPanel.Children.Add(img);
                }
            }

            this.path = Directory.GetFiles(@"Sounds\");
            this.music = new Musica(this.path);


            wrapPanel.Width = this.scrollWidth;

            Canvas.SetLeft(myRectangle, width / 3);
            Canvas.SetTop(myRectangle, height / 2);

            this.delayImatges = DateTime.Now;

            jugador = new E_Principal(this.Fondo);
            jugador.OnTouch += new PrincipalEventHandler(hitTest);

            this.MouseLeftButtonUp += new MouseButtonEventHandler(Up);
            this.MouseLeftButtonDown += new MouseButtonEventHandler(Down);
            this.MouseMove += new MouseEventHandler(Move);
        }

        private void Move(object sender, MouseEventArgs e)
        {
            System.Windows.Point p = (System.Windows.Point)e.GetPosition((UIElement)sender);

            if (myRectangle.IsMouseCaptured)
            {
                Canvas.SetLeft(myRectangle, p.X - myRectangle.Width / 2);
                Canvas.SetTop(myRectangle, p.Y - myRectangle.Height / 2);
            }
        }

        private void Up(object sender, MouseButtonEventArgs e)
        {
            myRectangle.ReleaseMouseCapture();
        }

        private void Down(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Point pt = e.GetPosition((UIElement)sender);
            HitTestResult result = VisualTreeHelper.HitTest(Fondo, pt);

            if (result != null)
            {
                if (result.VisualHit == myRectangle)
                {
                    myRectangle.CaptureMouse();
                }
            }
        }

        /// <summary>
        /// mètode que llegeix d'un fitxer les dades d'un skeleton
        /// </summary>
        /// <param name="player">nom del jugador</param>
        /// <returns>array ple de skeletons</returns>
        private Skeleton[] obrirSkeletons(string player)
        {
            // per donar-li la volta al nom
            string[] nom = player.Split(' ');
            Skeleton[] data = new Skeleton[6];

            FileStream fs2 = File.OpenRead(@"Jugadors\" + nom[1] + "_" + nom[0] + ".skd");
            BinaryFormatter bf2 = new BinaryFormatter();
            data = (Skeleton[])bf2.Deserialize(fs2);
            fs2.Close();

            return data;
        }

        private void hitTest(object source, PrincipalEventArgs e)
        {
            System.Windows.Point pt = e.GetPoint();

            // Perform the hit test against a given portion of the visual object tree.
            HitTestResult result = VisualTreeHelper.HitTest(Fondo, pt);

            if (result != null)
            {
                if (result.VisualHit == myRectangle && (DateTime.Now.TimeOfDay.TotalMilliseconds - this.delayImatges.TimeOfDay.TotalMilliseconds) > 4000)
                {
                    this.delayImatges = DateTime.Now;
                    if (scrollViewer.ScrollableWidth == scrollViewer.HorizontalOffset)
                    {
                        scrollViewer.ScrollToHome();
                    }
                    else
                    {
                        scrollViewer.ScrollToHorizontalOffset(scrollViewer.HorizontalOffset + this.width);
                        MyStoryboard.Storyboard.Begin();
                        oscilador.Amplitude = (short)(scrollViewer.HorizontalOffset + 4000);
                        oscilador.Frequency = scrollViewer.HorizontalOffset;

                        state = wav.PlaybackState;
                        wav.Play();

                        Thread t = new Thread(new ThreadStart(ThreadProc));
                        t.Start();
                    }
                }
            }
        }

        public void ThreadProc()
        {
            Thread.Sleep(500);
            wav.Stop();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Ens permet iniciar un sensor kinect
            kinect = new MyKinect();

            //Recorrem tots els perifèrics per tal de trobar un Kinect connectat, si en trobem un
            //connectat, el prenem com a Sesnor
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (this.sensor != null)
            {
                //Afegim un controlador d'esdeveniments que cridarem cada vegada que hi hagi
                //nous events davant la càmera.
                this.sensor.AllFramesReady += this.SensorSkeletonFrameReady;
                //empleno els arrays
                this.depthPixelData = kinect.getDepthPixelData;
                this.colorPixelData = kinect.getColorPixels;
                this.skeletonData = kinect.getSkeletonData;
                // Initialize a new FaceTracker with the KinectSensor
                this.faceTracker = new FaceTracker(sensor);

            }
            //En cas que no hi hagi cap Kinect connectat
            if (this.sensor == null)
            {
                MessageBox.Show("NO S'HA DETECTAT CAP KINECT", "ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                mw.Show();
            }
            music.escoltarCanco(0);
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
            this.Close();
            this.mw.Show();
        }

        private void Sortir_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
            this.Close();
            mw.Visibility = Visibility.Visible;
        }

        private void SensorSkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            // Retrieve each single frame and copy the data
            using (ColorImageFrame colorImageFrame = e.OpenColorImageFrame())
            {
                if (colorImageFrame == null)
                    return;
                colorImageFrame.CopyPixelDataTo(colorPixelData);
            }

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame == null)
                    return;
                depthImageFrame.CopyPixelDataTo(depthPixelData);
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame != null)
                {
                    skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                    skeletonFrame.CopySkeletonDataTo(skeletonData);

                    if (this.skeletonData[0].TrackingState != SkeletonTrackingState.Tracked &&
                    this.skeletonData[1].TrackingState != SkeletonTrackingState.Tracked &&
                    this.skeletonData[2].TrackingState != SkeletonTrackingState.Tracked &&
                    this.skeletonData[3].TrackingState != SkeletonTrackingState.Tracked &&
                    this.skeletonData[4].TrackingState != SkeletonTrackingState.Tracked &&
                    this.skeletonData[5].TrackingState != SkeletonTrackingState.Tracked)
                    {
                        this.skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        this.skeletonDataPle.CopyTo(this.skeletonData, 0);
                    }
                }
            }

            if (this.faceTracker == null)
            {
                try
                {
                    this.faceTracker = new FaceTracker(sensor);
                }
                catch (InvalidOperationException)
                {
                    // Durant alguns escenaris el Face Tracker és incapaç de crear una instància.
                    // Atrapem aquesta excepció i no fem el seguiment d'una cara.
                    this.faceTracker = null;
                }
            }

            if (this.faceTracker != null)
            {
                FaceTrackFrame faceFrame = null;
                try
                {
                    var skeleton = skeletonData.FirstOrDefault(s => s.TrackingState == SkeletonTrackingState.Tracked);
                    faceFrame = faceTracker.Track(sensor.ColorStream.Format, colorPixelData,
                                              sensor.DepthStream.Format, depthPixelData,
                                              skeleton);

                    if (faceFrame.TrackSuccessful)
                    {
                        jugador.Posicio = new System.Windows.Point((faceFrame.Rotation.Y * -8) + this.width / 2, (faceFrame.Rotation.X * -8)+ this.height / 2);
                    }
                }
                catch (NullReferenceException nu) { }
            }
        }
    }
}
