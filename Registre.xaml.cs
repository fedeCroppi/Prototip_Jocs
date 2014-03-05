using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FaceTrackingBasics
{
    /// <summary>
    /// Lógica de interacción para Registre.xaml
    /// </summary>
    public partial class Registre : Window
    {
        private MainWindow mw;

        //Elements per pantalla
        private MyKinect kinect;

        //Sensor Kinect
        private KinectSensor sensor;
        private FaceTracker faceTracker;
        private Vector3DF facePoints3D;
        private byte[] colorPixelData;
        private short[] depthPixelData;
        private Skeleton[] skeletonData;

        //rectangles de la cara
        private DrawingVisual drawingVisual;
        private System.Windows.Rect rect, rect2;
        private bool esVermell;

        // Elements per registrar jugadors
        private Xml xml;
        private List<string[]> dades;

        public Registre(MainWindow mw,ref List<string[]> dades)
        {
            InitializeComponent();

            this.xml = new Xml();
            this.dades = dades;

            this.rect2 = new System.Windows.Rect(new System.Windows.Point(280,180), new Size(100,80));
            this.rect = new System.Windows.Rect();
            this.esVermell = true;
            this.drawingVisual = new DrawingVisual();

            slider1.ValueChanged += new RoutedPropertyChangedEventHandler<double>(slider1_ValueChanged);

            foto.Visibility = Visibility.Hidden;
            this.mw = mw;
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            if (this.sensor != null)
            {
                this.sensor.Stop();      
            }
            this.mw.Show();
        }

        private void bFoto_Click(object sender, RoutedEventArgs e)
        {
            if (txNom.LineCount > 0 && txCognom.LineCount > 0)
            {
                formulari.Visibility = Visibility.Hidden;
                foto.Visibility = Visibility.Visible;
                this.WindowState = System.Windows.WindowState.Maximized;
                this.WindowStyle = System.Windows.WindowStyle.None;
            }
        }

        private void bPrendreFoto_Click(object sender, RoutedEventArgs e)
        {
            string[] aux = new string[5];
            aux[0] = txNom.Text;
            aux[1] = txCognom.Text;
            aux[2] = txHistoria.Text;
            SaveImage().CopyTo(aux, 3);
            dades.Add(aux);
            mw.cbPlayers.Items.Add(aux[0] + " " + aux[1]);
            xml.desarXML(ref dades);
            this.Close();
            mw.Visibility = Visibility.Visible;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            int value = sensor.ElevationAngle;
            if (!(value - (int)slider1.Value < 27) || !(value + (int)slider1.Value > 27))
            {
                try
                {
                    sensor.ElevationAngle = (int)slider1.Value;
                }
                catch (Exception exc)
                {
                    //res per fe
                }
            }
        }

        /// <summary>
        /// Executa les tasques necessàries per iniciar el programa (Inicialització de Kinect)
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param> 
        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //Ens permet iniciar la càmera RGB
            kinect = new MyKinect();

            VideoControl.Source = kinect.getcolorBitmap();

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

                this.depthPixelData = kinect.getDepthPixelData;
                this.colorPixelData = kinect.getColorPixels;
                
                //depthPixelData = new short[sensor.DepthStream.FramePixelDataLength];
                //colorPixelData = new byte[sensor.ColorStream.FramePixelDataLength];
                this.skeletonData = kinect.getSkeletonData;
                // Initialize a new FaceTracker with the KinectSensor
                this.faceTracker = new FaceTracker(sensor);

                

                // Iniciem el Sensor Kinect detectat
                /*try
                {         
                    this.sensor.Start();
                }
                catch (IOException)
                {
                    this.sensor = null;
                }*/
            }
            //En cas que no hi hagi cap Kinect connectat
            if (this.sensor == null)
            {
                MessageBox.Show("NO S'HA DETECTAT CAP KINECT", "ERROR", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
                mw.Show();
            }
        }

        /// <summary>
        /// Metode que desa una imatge de la camara kinect agafan les dades
        /// del control Image
        /// </summary>
        private string[] SaveImage()
        {
            string[] aux = new string[2];

            FileStream fileStream = new FileStream(string.Format(@"Jugadors\{0}_{1}.jpg", txCognom.Text, txNom.Text), System.IO.FileMode.Create);
            BitmapSource imageSource = (BitmapSource)VideoControl.Source;
            JpegBitmapEncoder jpegEncoder = new JpegBitmapEncoder();
            jpegEncoder.Frames.Add(BitmapFrame.Create(imageSource));
            jpegEncoder.Save(fileStream);
            fileStream.Close();

            FileStream fs = new FileStream(string.Format(@"Jugadors\{0}_{1}.skd", txCognom.Text, txNom.Text), System.IO.FileMode.Create);
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(fs, this.skeletonData);
            fs.Close();

            aux[0] = fileStream.Name;
            aux[1] = fs.Name;
            return aux;
        }

        /// <summary>
        /// Controlador d'esdeveniment que controla els events que genera el Kinect
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void SensorSkeletonFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            SkeletonFrame skeletonFrame = null;
            try
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
                using (skeletonFrame = e.OpenSkeletonFrame())
                {
                    if (skeletonFrame == null)
                    {
                        return;
                    }
                    else
                    {
                        skeletonData = new Skeleton[skeletonFrame.SkeletonArrayLength];
                        skeletonFrame.CopySkeletonDataTo(skeletonData);
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
                        var trackSucceeded = faceFrame.TrackSuccessful;
                        if (trackSucceeded)
                        {
                            facePoints3D = faceFrame.Rotation;
                            //rectángulo que representa la posición de la cara
                            this.rect.X = (double)faceFrame.FaceRect.Left;
                            this.rect.Y = (double)faceFrame.FaceRect.Top;
                            this.rect.Size = new Size(faceFrame.FaceRect.Width, faceFrame.FaceRect.Height);

                            if (rect.IntersectsWith(rect2) && esVermell)
                            {
                                SolidColorBrush greenYellow = new SolidColorBrush();
                                greenYellow.Color = Colors.GreenYellow;
                                topLeft.Stroke = greenYellow;
                                topRight.Stroke = greenYellow;
                                bottonRight.Stroke = greenYellow;
                                bottonLeft.Stroke = greenYellow;
                                esVermell = false;
                                bPrendreFoto.IsEnabled = true;
                            }
                            if (!rect.IntersectsWith(rect2) && !esVermell)
                            {
                                SolidColorBrush red = new SolidColorBrush();
                                red.Color = Colors.Red;
                                topLeft.Stroke = red;
                                topRight.Stroke = red;
                                bottonRight.Stroke = red;
                                bottonLeft.Stroke = red;
                                esVermell = true;
                                bPrendreFoto.IsEnabled = false;
                            }
                        }
                        else
                        {
                            SolidColorBrush red = new SolidColorBrush();
                            red.Color = Colors.Red;
                            topLeft.Stroke = red;
                            topRight.Stroke = red;
                            bottonRight.Stroke = red;
                            bottonLeft.Stroke = red;
                            esVermell = true;
                            bPrendreFoto.IsEnabled = false;
                        }
                    }
                    catch (NullReferenceException nu) { }
                }
            }
            finally
            {
                if (skeletonFrame != null)
                {
                    skeletonFrame.Dispose();
                }
            }
        }

        /// <summary>
        /// controla el tracking mode del skeketon stream
        /// </summary>
        /// <param name="sender">check seated</param>
        /// <param name="e"></param>
        private void asegutClic(object sender, RoutedEventArgs e)
        {
            if (seated.IsChecked.Value)
            {
                sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;
            }
            else
            {
                sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Default;
            }
        }
    }
}
