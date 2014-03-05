using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FaceTrackingBasics
{
    /**
     * Primer hem de definir un delegat que actua com una signatura per a la
     * funció que es crida quan en última instància, s'activa l'esdeveniment.
     * El segon paràmetre és de tipus MyEventArgs. 
     * Aquest objecte conté informació sobre l'esdeveniment disparat.
     */
    public delegate void PrincipalEventHandler(object source, PrincipalEventArgs e);

    /**
     * Aquesta és una classe que descriu l'esdeveniment a la classe que el rep. 
     * Una classe EventArgs sempre ha de derivar de System.EventArgs.
     */
    public class PrincipalEventArgs : EventArgs
    {
        private Point EventInfo;
        public PrincipalEventArgs(Point p)
        {
            EventInfo = p;
        }
        public Point GetPoint()
        {
            return EventInfo;
        }
    }

    public class E_Principal : Shape
    {
        public event PrincipalEventHandler OnTouch;
        private Point posicio;
        private Ellipse ellipse;

        public E_Principal(Canvas canvas)
        {
            ellipse = new Ellipse();
            posicio = new Point(300, 300);
            ellipse.Height = 50;
            ellipse.Width = 50;
            Canvas.SetLeft(ellipse, posicio.X);
            Canvas.SetTop(ellipse, posicio.Y);
            SolidColorBrush black = new SolidColorBrush();
            black.Color = Colors.Black;
            ellipse.Fill = black;
            canvas.Children.Add(ellipse);
        }

        public Point Posicio
        {
            get { return posicio; }
            set
            {
                posicio = value;
                Canvas.SetLeft(ellipse, value.X);
                Canvas.SetTop(ellipse, value.Y);
                OnTouch(this, new PrincipalEventArgs(posicio));
            }
        }

        //metode de obligat implementació quan un objecte hereta de Shape
        protected override System.Windows.Media.Geometry DefiningGeometry
        {
            get { throw new NotImplementedException(); }
        }
    }
}
