using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MediaPlayer
{
    /// <summary>
    /// Interaction logic for EditMediaInfoUserControl.xaml
    /// </summary>
    public partial class EditMediaInfoUserControl : UserControl
    {

        public delegate void SaveClickEvent(object sender, RoutedEventArgs e);
        public event SaveClickEvent OnSaveClickEvent;

        public EditMediaInfoUserControl()
        {
            InitializeComponent();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            this.OnSaveClickEvent?.Invoke(sender, e);
        }

    }
}

//References:
//https://stackoverflow.com/questions/61158159/handling-a-button-click-inside-a-user-control-from-the-main-window