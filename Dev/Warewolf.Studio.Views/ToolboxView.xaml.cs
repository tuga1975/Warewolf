﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;
using Dev2;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Studio.Controller;
using Infragistics.Windows.DockManager;
using Warewolf.Studio.ViewModels.ToolBox;

namespace Warewolf.Studio.Views
{
    /// <summary>
    /// Interaction logic for ToolboxView.xaml
    /// </summary>
    public partial class ToolboxView : IToolboxView
    {
        public ToolboxView()
        {
            InitializeComponent();
            Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Arrange(new Rect(new Size(ActualWidth, ActualHeight)));

            PreviewDragOver += DropPointOnDragEnter;
        }

        void UIElement_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid && e.LeftButton == MouseButtonState.Pressed)
            {
                var dataContext = grid.DataContext as ToolDescriptorViewModel;
                if (dataContext?.ActivityType != null)
                {
                    DragDrop.DoDragDrop((DependencyObject)e.Source, dataContext.ActivityType, DragDropEffects.Copy);
                }
            }
        }

        void SelectAllText(object sender, RoutedEventArgs e)
        {
            var tb = sender as TextBox;
            tb?.SelectAll();
        }

        void SelectivelyIgnoreMouseButton(object sender, MouseButtonEventArgs e)
        {
            var imageSource = e.OriginalSource as FontAwesome.WPF.ImageAwesome;
            var rectSource = e.OriginalSource as Rectangle;
            if (imageSource == null && rectSource == null && sender is TextBox tb && !tb.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                tb.Focus();
            }


        }

        void ToolGrid_OnMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is Grid grid)
            {
                var viewModel = grid.DataContext as ToolDescriptorViewModel;
                grid.ToolTip = viewModel?.Tool.ResourceToolTip;
            }
            var variablesPane = Application.Current.MainWindow.FindName("Variables") as ContentPane;
            var explorerPane = Application.Current.MainWindow.FindName("Explorer") as ContentPane;
            var outputPane = Application.Current.MainWindow.FindName("OutputPane") as ContentPane;
            var documentHostPane = Application.Current.MainWindow.FindName("DocumentHost") as ContentPane;

            if (variablesPane != null && !variablesPane.IsActivePane &&
                explorerPane != null && !explorerPane.IsActivePane &&
                outputPane != null && !outputPane.IsActivePane &&
                documentHostPane != null && !documentHostPane.IsActivePane)
            {
                var toolboxPane = Application.Current.MainWindow.FindName("Toolbox") as ContentPane;
                toolboxPane?.Activate();
            }
        }

        void DropPointOnDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            e.Handled = true;
        }

        void ToolGrid_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Grid grid)
            {
                var viewModel = grid.DataContext as ToolDescriptorViewModel;
                if (e.ClickCount == 1)
                {
                    var toolboxViewModel = DataContext as ToolboxViewModel;
                    toolboxViewModel?.UpdateHelpDescriptor(viewModel?.Tool.ResourceHelpText);
                }
                else
                {
                    var popupController = CustomContainer.Get<IPopupController>();
                    popupController?.Show(Studio.Resources.Languages.Core.ToolboxPopupDescription, Studio.Resources.Languages.Core.ToolboxPopupHeader, MessageBoxButton.OK, MessageBoxImage.Information, "", false, false, true, false, false, false);
                }
            }

        }
    }
}