using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace ImageWatchCSharp.ToolWindow
{
    /// <summary>
    /// Interaction logic for MainToolWindowControl.
    /// </summary>
    public partial class MainToolWindowControl : UserControl
    {
        public MainToolWindowControl()
        {
            this.InitializeComponent();
            this.DataContext = DebuggerMonitor.Instance.MainViewModel;
        }
    }
}