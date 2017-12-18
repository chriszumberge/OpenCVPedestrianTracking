using System;
using System.Drawing;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using Emgu.CV.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using Emgu.CV.Features2D;
using Emgu.CV.Flann;
using Emgu.CV.UI;
using System.Collections;

namespace PedestrianTracker
{
    public partial class Form1 : Form
    {
        static VideoCapture _camera;

        public Form1()
        {
            InitializeComponent();

            string captureSource = @"girl_walking.mp4";
            //string captureSource = @"https://youtu.be/3FXUw98rrUY/?action=stream?dummy=param.mjpg";

            try
            {
                _camera = new VideoCapture(captureSource);
                
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }

            // Occurs when the application finishes processing and is about to enter the idle state
            Application.Idle += Application_Idle;
        }

        private void Application_Idle(object sender, EventArgs e)
        {
            Mat frame = _camera.QueryFrame();

            if (frame != null)
            {
                CvInvoke.Resize(frame, frame, new Size(944, 493));

                long processingTime;
                Rectangle[] results;

                using (UMat uImage = frame.GetUMat(AccessType.ReadWrite))
                {
                    results = FindPedestrian(uImage, out processingTime);
                }

                foreach (Rectangle rect in results)
                {
                    CvInvoke.Rectangle(frame, rect, new Bgr(Color.Red).MCvScalar);
                }

                imageDisplay.Image = frame;
            }
            else
            {
                imageDisplay.Image = frame;
            }
        }

        private static Rectangle[] FindPedestrian(IInputArray image, out long processTime)
        {
            Stopwatch watch;
            Rectangle[] regions;

            using (InputArray iaImage = image.GetInputArray())
            {
                using (HOGDescriptor des = new HOGDescriptor())
                {
                    des.SetSVMDetector(HOGDescriptor.GetDefaultPeopleDetector());
                    watch = Stopwatch.StartNew();

                    MCvObjectDetection[] results = des.DetectMultiScale(image);
                    regions = new Rectangle[results.Length];

                    for (int i = 0; i < results.Length; i++)
                    {
                        regions[i] = results[i].Rect;
                    }
                    watch.Stop();
                }
            }

            processTime = watch.ElapsedMilliseconds;

            return regions;
        }
    }
}
