using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Controls;
using Microsoft.Research.Kinect.Nui;
using Color = System.Windows.Media.Color;
using Vector = Microsoft.Research.Kinect.Nui.Vector;

namespace Kinect.Toolbox
{
    public abstract class GestureDetector
    {       
        public int MinimalPeriodBetweenGestures { get; set; }

        readonly List<Entry> entries = new List<Entry>();

        public event Action<string> OnGestureDetected;

        DateTime lastGestureDate = DateTime.Now;

        readonly int windowSize;

        Canvas displayCanvas;
        Color displayColor;

        protected GestureDetector(int windowSize = 20)
        {
            this.windowSize = windowSize;
            MinimalPeriodBetweenGestures = 0;
        }

        protected List<Entry> Entries
        {
            get { return entries; }
        }

        public int WindowSize
        {
            get { return windowSize; }
        }

        public virtual void Add(Vector position, SkeletonEngine engine)
        {
            Entry newEntry = new Entry {Position = position.ToVector3(), Time = DateTime.Now};
            Entries.Add(newEntry);

            if (displayCanvas != null)
            {
                newEntry.DisplayEllipse = new Ellipse
                {
                    Width = 4,
                    Height = 4,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    StrokeThickness = 2.0,
                    Stroke = new SolidColorBrush(displayColor),
                    StrokeLineJoin = PenLineJoin.Round
                };


                float x, y;

                engine.SkeletonToDepthImage(position, out x, out y);

                x = (float)(x * displayCanvas.ActualWidth);
                y = (float)(y * displayCanvas.ActualHeight);

                Canvas.SetLeft(newEntry.DisplayEllipse, x - newEntry.DisplayEllipse.Width / 2);
                Canvas.SetTop(newEntry.DisplayEllipse, y - newEntry.DisplayEllipse.Height / 2);

                displayCanvas.Children.Add(newEntry.DisplayEllipse);
            }

            if (Entries.Count > WindowSize)
            {
                Entry entryToRemove = Entries[0];
                
                if (displayCanvas != null)
                {
                    displayCanvas.Children.Remove(entryToRemove.DisplayEllipse);
                }

                Entries.Remove(entryToRemove);
            }

            LookForGesture();
        }

        protected void RaiseGestureDetected(string gesture)
        {
            if (DateTime.Now.Subtract(lastGestureDate).TotalMilliseconds > MinimalPeriodBetweenGestures)
            {
                if (OnGestureDetected != null)
                    OnGestureDetected(gesture);

                lastGestureDate = DateTime.Now;
            }

            Entries.ForEach(e=>
                                {
                                    if (displayCanvas != null)
                                        displayCanvas.Children.Remove(e.DisplayEllipse);
                                });
            Entries.Clear();
        }

        protected abstract void LookForGesture();

        public void TraceTo(Canvas canvas, Color color)
        {
            displayCanvas = canvas;
            displayColor = color;
        }
    }
}