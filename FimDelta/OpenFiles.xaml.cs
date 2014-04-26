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

using FimDelta.Xml;

namespace FimDelta
{
    /// <summary>
    /// Interaction logic for OpenFiles.xaml
    /// </summary>
    public partial class OpenFiles : Window
    {
        public Delta delta;

        public string SourceFileName = "source.xml";
        public string TargetFileName = "target.xml";
        public string ChangesFileName = "changes.xml";

        public OpenFiles()
        {
            Loaded += OpenFiles_Loaded;
            InitializeComponent();
        }

        void OpenFiles_Loaded(object sender, RoutedEventArgs e)
        {
            SourceFileNameTextBox.Text = SourceFileName;
            TargetFileNameTextBox.Text = TargetFileName;
            ChangesFileNameTextBox.Text = ChangesFileName;
        }



        private void SourceButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = SourceFileName;
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Exported FIM Service objects (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                SourceFileName = dlg.FileName;
                SourceFileNameTextBox.Text = System.IO.Path.GetFileName(dlg.FileName);
            }
        }

        private void TargetButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = TargetFileName;
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Exported FIM Service objects (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                TargetFileName = dlg.FileName;
                TargetFileNameTextBox.Text = System.IO.Path.GetFileName(dlg.FileName);
            }
        }

        private void ChangesButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = ChangesFileName;
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Delta FIM Service objects (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                ChangesFileName = dlg.FileName;
                ChangesFileNameTextBox.Text = System.IO.Path.GetFileName(dlg.FileName);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // In case the user just typed in the file names and didn't browse, we need to copy
            // the textbox values into our public properties

            if (!SourceFileName.EndsWith(SourceFileNameTextBox.Text))
            {
                SourceFileName = SourceFileNameTextBox.Text;
            }

            if (!TargetFileName.EndsWith(TargetFileNameTextBox.Text))
            {
                TargetFileName = TargetFileNameTextBox.Text;
            }

            if (!ChangesFileName.EndsWith(ChangesFileNameTextBox.Text))
            {
                ChangesFileName = ChangesFileNameTextBox.Text;
            }

            try
            {
                delta = DeltaParser.ReadDelta(SourceFileName, TargetFileName, ChangesFileName);
            }
            catch (System.IO.FileNotFoundException ex)
            {
                MessageBox.Show(string.Format("Unable to open files: {0}", ex.Message), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (delta != null)
            {
                this.Close();
            }
        }
    }
}
