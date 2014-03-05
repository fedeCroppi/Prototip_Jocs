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

namespace FaceTrackingBasics
{
    /// <summary>
    /// Lógica de interacción para Juego3.xaml
    /// </summary>
    public partial class Joc3 : Window
    {
        private MainWindow mw;

        //Sensor Kinect
        private MyKinect kinect;
        private KinectSensor sensor;
        private FaceTracker faceTracker;
        private byte[] colorPixelData;
        private short[] depthPixelData;
        private Skeleton[] skeletonDataPle, skeletonData;

        // mida de la pantalla
        private double width, height;

        // Videos
        private string[] dirs;

        private System.Windows.Point nas;

        public Joc3(MainWindow mw, string player)
        {
            InitializeComponent();
            this.mw = mw;

            //mides de pantalla
            this.width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.height = System.Windows.SystemParameters.PrimaryScreenHeight;

            //emplena el array amb les dades del jugador registrat
            this.skeletonDataPle = obrirSkeletons(player);

            this.dirs = Directory.GetFiles(@"Videos\");

            //emplena el combobox amb els noms dels fitxers de video
            foreach(String dir in dirs)
            {
                llistat.Items.Add(dir);
            }

            video.Play();
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

        private void bClose(object sender, RoutedEventArgs e)
        {
            this.Close();
            this.mw.Show();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
            video.Stop();
            this.Close();
            this.mw.Show();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Ens permet iniciar un sensor kinect
            this.kinect = new MyKinect();

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

                // Iniciar una nova FaceTracker amb el KinectSensor
                this.faceTracker = new FaceTracker(sensor);

            }
            //En cas que no hi hagi cap Kinect connectat
            if (this.sensor == null)
            {
                MessageBox.Show("NO S'HA DETECTAT CAP KINECT", "ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                mw.Show();
            }

            video.Source = new Uri(dirs[0], UriKind.Relative);
        }

        private void Sortir_Click(object sender, RoutedEventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
            video.Stop();
            this.Close();
            mw.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// metode que rep totes les dades del kinect
        /// </summary>
        /// <param name="sender">kinect</param>
        /// <param name="e"></param>
        private void SensorSkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            // Recuperar cada fotograma individual i copiar les dades
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

                    if (skeleton != null)
                    {
                        var cap = skeleton.Joints[JointType.Head];

                    }

                    // Si es fa el seguiment d'una cara , llavors podem usar-lo
                    if (faceFrame.TrackSuccessful)
                    {
                        //punts de la cara
                        var ps = faceFrame.GetProjected3DShape();

                        //punt més alt del cap
                        var parteSuperiorCraneo = ps[FeaturePoint.TopSkull];

                        //punt de sota de la barbeta
                        var debajoBarbilla = ps[FeaturePoint.BottomOfChin];

                        if ((debajoBarbilla.X - parteSuperiorCraneo.X) > 15 || (debajoBarbilla.X - parteSuperiorCraneo.X) < -15)
                        {
                            
                            video.Pause();
                        }
                        else
                        {

                            video.Play();
                        }
                    }
                    else
                    {
                        video.Pause();
                    }
                }
                catch (NullReferenceException nu) { }
            }
        }

        /// <summary>
        /// evento que se produce cuando el video termina.
        /// vuelve a comenzar desde el principio
        /// </summary>
        private void Element_MediaEnded(object sender, RoutedEventArgs e)
        {
            video.Position = new TimeSpan(0, 0, 0, 0, 0);
        }

        /// <summary>
        /// evento que se produce cuando se selecciona un item.
        /// Cambia el video que se reproduce en el juego
        /// </summary>
        private void llistat_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            video.Source = new Uri(llistat.SelectedItem.ToString(), UriKind.Relative);
        }
    }
}
