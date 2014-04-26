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
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using FimDelta.Xml;

namespace FimDelta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly CollectionViewSource view;
        private Delta delta;
        private DeltaViewController deltaVC;

        internal string SourceFileName;
        internal string TargetFileName;
        internal string ChangesFileName;
        internal string FilteredChangesFileName = "changes_filtered.xml";

        public MainWindow()
        {
            InitializeComponent();
                
            view = (CollectionViewSource)FindResource("ObjectsView");

            Loaded += new RoutedEventHandler(Open_Click);
        }



        private void Open_Click(object sender, RoutedEventArgs e)
        {
            OpenFiles of = new OpenFiles();

            if (!string.IsNullOrEmpty(SourceFileName))
            {
                of.SourceFileName = SourceFileName;
            }

            if (!string.IsNullOrEmpty(TargetFileName))
            {
                of.TargetFileName = TargetFileName;
            }

            if (!string.IsNullOrEmpty(ChangesFileName))
            {
                of.ChangesFileName = ChangesFileName;
            }

            of.Owner = this;
            of.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            of.ShowDialog();
            if (of.delta != null)
            {
                delta = of.delta;
                deltaVC = new DeltaViewController(delta);
                view.Source = deltaVC.View;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (delta == null) return;


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = FilteredChangesFileName;
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Filtered Delta FIM Service objects (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DeltaParser.SaveDelta(delta, dlg.FileName);
            }

        }

        private void Save_Exclusions_Click(object sender, RoutedEventArgs e)
        {
            if (delta == null) return;


            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = "exclusions.xml";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Exclusions (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DeltaParser.SaveExclusions(delta, dlg.FileName);
            }
        }

        private void Load_Exclusions_Click(object sender, RoutedEventArgs e)
        {
            if (delta == null) return;


            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = "exclusions.xml";
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Exclusions (.xml)|*.xml|All Files (*.*)|*.*";

            Nullable<bool> result = dlg.ShowDialog();

            if (result == true)
            {
                DeltaParser.LoadExclusions(delta, dlg.FileName);
            }
        }

        private void MenuItem_Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void MenuItem_Group_None_Click(object sender, RoutedEventArgs e)
        {
            if (view == null || deltaVC == null) return;
            deltaVC.Grouping = GroupType.None;
            view.Source = deltaVC.View;
        }

        private void MenuItem_Group_Operation_Click(object sender, RoutedEventArgs e)
        {
            if (view == null || deltaVC == null) return;
            deltaVC.Grouping = GroupType.State;
            view.Source = deltaVC.View;
        }

        private void MenuItem_Group_Object_Click(object sender, RoutedEventArgs e)
        {
            if (view == null || deltaVC == null) return;
            deltaVC.Grouping = GroupType.ObjectType;
            view.Source = deltaVC.View;
        }

        private void Help_About_Click(object sender, RoutedEventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }
    }
}
