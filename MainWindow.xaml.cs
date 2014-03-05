// -----------------------------------------------------------------------
// <copyright file="MainWindow.xaml.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// -----------------------------------------------------------------------

namespace FaceTrackingBasics
{
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
    using System.Windows.Navigation;
    using System.Windows.Shapes;

    using Microsoft.Kinect;
    using Microsoft.Kinect.Toolkit;
    using Microsoft.Kinect.Toolkit.FaceTracking;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Xml xml;
        private List<string[]> dades;

        public MainWindow()
        {
            InitializeComponent();

            xml = new Xml();
            dades = xml.LlegirXml();
            foreach (string[] aux in dades)
            {
                cbPlayers.Items.Add(aux[0] + " " + aux[1]);
            }
        }

        private void Joc1_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Joc1 J1 = new Joc1(this, cbPlayers.Text);
            J1.Show();
        }

        private void Joc2_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Joc2 J2 = new Joc2(this, cbPlayers.Text);
            J2.Show();
        }

        private void Joc3_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Joc3 J3 = new Joc3(this, cbPlayers.Text);
            J3.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            Registre reg = new Registre(this, ref dades);
            reg.Show();
        }

        private void WindowClosed(object sender, EventArgs e)
        {
            this.Close();
        }

        private void jugSelec(object sender, RoutedEventArgs e)
        {
            if (cbPlayers.SelectedValue != null)
            {
                bJoc1.IsEnabled = bJoc2.IsEnabled = bJoc3.IsEnabled = true;
            }
        }

        private void cbPlayers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
