using Microsoft.Kinect;
using Microsoft.Kinect.Toolkit.FaceTracking;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace FaceTrackingBasics
{
    class MyKinect
    {
        /// <summary>
        /// Active Kinect sensor
        /// </summary>
        private KinectSensor sensor;

        /// <summary>
        /// Bitmap that will hold color information
        /// </summary>
        private WriteableBitmap colorBitmap;

        /// <summary>
        /// Intermediate storage for the color data received from the camera
        /// </summary>
        private byte[] colorPixels;
        private ColorImageFormat currentColorImageFormat = ColorImageFormat.Undefined;

        /// <summary>
        /// Intermediate storage for the depth data received from the camera
        /// </summary>
        private short[] depthPixelData;

        private Skeleton[] skeletonData;

        


        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MyKinect()
        {
            InitializeComponent();
        }

        public WriteableBitmap getcolorBitmap()
        {
            return this.colorBitmap;
        }

        public Skeleton[] getSkeletonData
        {
            get { return skeletonData; }
        }

        public byte[] getColorPixels
        {
            get { return colorPixels; }
        }

        public short[] getDepthPixelData
        {
            get { return depthPixelData; }
        }

        /// <summary>
        /// Execute startup tasks
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void InitializeComponent()
        {
            foreach (var potentialSensor in KinectSensor.KinectSensors)
            {
                if (potentialSensor.Status == KinectStatus.Connected)
                {
                    this.sensor = potentialSensor;
                    break;
                }
            }

            if (null != this.sensor)
            {
                this.sensor.ColorStream.Enable(ColorImageFormat.RgbResolution640x480Fps30);
                this.sensor.DepthStream.Enable(DepthImageFormat.Resolution320x240Fps30);

                this.colorPixels = new byte[this.sensor.ColorStream.FramePixelDataLength];
                this.depthPixelData = new short[this.sensor.DepthStream.FramePixelDataLength];
                this.colorBitmap = new WriteableBitmap(this.sensor.ColorStream.FrameWidth, this.sensor.ColorStream.FrameHeight, 96.0, 96.0, PixelFormats.Bgr32, null);

                this.sensor.AllFramesReady += this.AllFrameReady;

                this.sensor.SkeletonStream.TrackingMode = SkeletonTrackingMode.Seated;

                /**
                 * Suportat només per Kinect per a Windows
                 * this.sensor.DepthStream.Range = DepthRange.Near;
                 * this.sensor.SkeletonStream.EnableTrackingInNearRange = true;
                 */

                // Si hem trobat un Kinect, iniciem l'Skeleton
                this.sensor.SkeletonStream.Enable();

                skeletonData = new Skeleton[6];

                try
                {
                    this.sensor.Start();
                    this.sensor.ElevationAngle = 5;
                }
                catch (IOException)
                {
                    this.sensor = null;
                }
            }
        }

        /// <summary>
        /// Event handler for Kinect sensor's
        /// </summary>
        /// <param name="sender">object sending the event</param>
        /// <param name="e">event arguments</param>
        private void AllFrameReady(object sender, AllFramesReadyEventArgs e)
        {
            using (ColorImageFrame colorFrame = e.OpenColorImageFrame())
            {
                if (colorFrame == null) return;

                // Make a copy of the color frame for displaying.
                var haveNewFormat = this.currentColorImageFormat != colorFrame.Format;
                if (haveNewFormat)
                {
                    this.currentColorImageFormat = colorFrame.Format;
                    this.colorPixels = new byte[colorFrame.PixelDataLength];
                    
                }
                this.colorBitmap.WritePixels(
                       new Int32Rect(0, 0, this.colorBitmap.PixelWidth, this.colorBitmap.PixelHeight),
                       this.colorPixels,
                       this.colorBitmap.PixelWidth * sizeof(int), 0);
                colorFrame.CopyPixelDataTo(this.colorPixels);
            }

            using (DepthImageFrame depthImageFrame = e.OpenDepthImageFrame())
            {
                if (depthImageFrame == null) return;
                depthImageFrame.CopyPixelDataTo(depthPixelData);
            }

            using (SkeletonFrame skeletonFrame = e.OpenSkeletonFrame())
            {
                if (skeletonFrame == null) return;
                skeletonFrame.CopySkeletonDataTo(this.skeletonData);
            }
        }
    }
}
