﻿using ContentPage;
using SessionState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
//using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ViewModel;

namespace Dashboard
{
    /// <summary>
    /// Interaction logic for InstructorPage.xaml
    /// </summary>
    public partial class InstructorPage : Page
    {
        private ServerPage contentServerPage;
        public InstructorPage()
        {
            InitializeComponent();

            try
            {
                InstructorViewModel viewModel = new();
                DataContext = viewModel;

                contentServerPage = new ServerPage ( viewModel.Communicator);
                ResultFrame.Content = contentServerPage;
            }
            catch (Exception exception)
            {
                // If an exception occurs during ViewModel creation, show an error message and shutdown the application.
                _ = MessageBox.Show(exception.Message);
                Application.Current.Shutdown();
            }
        }

        private void LogoutButtonClick(object sender, RoutedEventArgs e)
        {
            // If a valid NavigationService exists, navigate to the "Login.xaml" page.
            NavigationService?.Navigate( new Uri( "AuthenticationPage.xaml" , UriKind.Relative ) );
        }

        private void Student_Selected(object sender, MouseButtonEventArgs e)
        {
            if (sender is ListViewItem item && item.IsSelected)
            {
                if (item.DataContext is Student clickedStudent)
                {
                    Debug.WriteLine( $"Clicked {clickedStudent.Id}" );
                    if (clickedStudent.Id != null)
                    {
                        contentServerPage.SetSessionID( clickedStudent.Id );
                    }
                }
            }
        }
    }
}
