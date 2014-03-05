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
    /// Lógica de interacción para Juego2.xaml
    /// </summary>
    public partial class Joc2 : Window
    {
        private MainWindow mw;

        //Sensor Kinect
        private MyKinect kinect;
        private KinectSensor sensor;
        private FaceTracker faceTracker;
        private byte[] colorPixelData;
        private short[] depthPixelData;
        private Skeleton[] skeletonDataPle, skeletonData;

        //Gestos
        private System.Windows.Point nas;
        private bool derecha, izquierda, arriba, abajo;
        private DateTime si, no;
        private const float PERIODO_ENTRE_GESTOS = 500;
        private int cont, margeDeteccio = 4;

        // mida de la pantalla
        private double width, height;

        //Musica
        private Musica music;
        private string[] path;

        public Joc2(MainWindow mw, string player)
        {
            InitializeComponent();
            this.mw = mw;

            derecha = izquierda = arriba = abajo = true;

            //mides de pantalla
            this.width = System.Windows.SystemParameters.PrimaryScreenWidth;
            this.height = System.Windows.SystemParameters.PrimaryScreenHeight;

            this.path = Directory.GetFiles(@"Sounds\");
            this.music = new Musica(this.path);

            this.skeletonDataPle = obrirSkeletons(player);
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

        private void WindowClosed(object sender, EventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();
            }
            this.Close();
            this.mw.Show();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Ens permet iniciar un sensor kinect
            this.kinect = new MyKinect();

            video.Source = kinect.getcolorBitmap();

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

                    if (skeleton != null)
                    {
                        var cap = skeleton.Joints[JointType.Head];

                        nas.Y = faceFrame.Rotation.X * -5;
                        nas.X = faceFrame.Rotation.Y * -5;

                    }
                    // If a face is tracked, then we can use it.
                    if (faceFrame.TrackSuccessful)
                    {
                        // Retrieve only the Animation Units coeffs.
                        var AUCoeff = faceFrame.GetAnimationUnitCoefficients();

                        var manAbajo = AUCoeff[AnimationUnit.JawLower];
                        manAbajo = manAbajo < 0 ? 0 : manAbajo;

                    }



                    if (nas.X < (margeDeteccio * -1) && derecha)
                    {
                        izquierda = true;

                        if (derecha && DateTime.Now.TimeOfDay.TotalMilliseconds - no.TimeOfDay.TotalMilliseconds < PERIODO_ENTRE_GESTOS)
                        {
                            cont++;
                        }
                        else
                        {
                            cont = 0;
                        }

                        no = DateTime.Now;

                        derecha = false;

                        if (cont > 4)
                        {
                            music.next();
                            cont = 0;
                        }
                    }
                    if (nas.X > margeDeteccio && izquierda)
                    {
                        derecha = true;

                        if (izquierda && DateTime.Now.TimeOfDay.TotalMilliseconds - no.TimeOfDay.TotalMilliseconds < PERIODO_ENTRE_GESTOS)
                        {
                            cont++;
                        }
                        else
                        {
                            cont = 0;
                        }

                        no = DateTime.Now;

                        izquierda = false;

                        if (cont > 4)
                        {
                            music.next();
                            cont = 0;
                        }
                    }
                    if (nas.Y < (margeDeteccio * -1) && abajo)
                    {
                        arriba = true;

                        if (abajo && DateTime.Now.TimeOfDay.TotalMilliseconds - si.TimeOfDay.TotalMilliseconds < PERIODO_ENTRE_GESTOS)
                        {
                            cont++;
                        }
                        else
                        {
                            cont = 0;
                        }

                        si = DateTime.Now;

                        abajo = false;

                        if (cont > 4)
                        {
                            music.replay();
                            cont = 0;
                        }
                    }
                    if (nas.Y > (margeDeteccio + 2) && arriba)
                    {
                        abajo = true;

                        if (arriba && DateTime.Now.TimeOfDay.TotalMilliseconds - si.TimeOfDay.TotalMilliseconds < PERIODO_ENTRE_GESTOS)
                        {
                            cont++;
                        }
                        else
                        {
                            cont = 0;
                        }

                        si = DateTime.Now;

                        arriba = false;

                        if (cont > 3)
                        {
                            music.replay();
                            cont = 0;
                        }
                    }
                }
                catch (NullReferenceException nu) { }             
            }
        }
    }
}
