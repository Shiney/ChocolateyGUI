﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright company="Chocolatey" file="MainWindow.xaml.cs">
//   Copyright 2014 - Present Rob Reynolds, the maintainers of Chocolatey, and RealDimensions Software, LLC
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace ChocolateyGui.Views.Windows
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Input;
    using ChocolateyGui.ChocolateyFeedService;
    using ChocolateyGui.Controls.Dialogs;
    using ChocolateyGui.Models;
    using ChocolateyGui.Services;
    using ChocolateyGui.ViewModels.Items;
    using ChocolateyGui.ViewModels.Windows;
    using ChocolateyGui.Views.Controls;
    using MahApps.Metro.Controls.Dialogs;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IProgressService _progressService;

        public MainWindow(IMainWindowViewModel viewModel, INavigationService navigationService, IProgressService progressService)
        {
            if (navigationService == null)
            {
                throw new ArgumentNullException("navigationService");
            }

            InitializeComponent();
            DataContext = viewModel;

            if (progressService is ProgressService)
            {
                (progressService as ProgressService).MainWindow = this;
            }

            this._progressService = progressService;
            
            // RichiCoder1 (21-09-2014) - Why are we doing this, especially here?
            AutoMapper.Mapper.CreateMap<V2FeedPackage, PackageViewModel>();
            AutoMapper.Mapper.CreateMap<PackageMetadata, PackageViewModel>();

            navigationService.SetNavigationItem(GlobalFrame);
            navigationService.Navigate(typeof(SourcesControl));
        }

        public Task<ChocolateyDialogController> ShowChocolateyDialogAsync(string title, bool isCancelable = false, MetroDialogSettings settings = null)
        {
            return (Task<ChocolateyDialogController>)Dispatcher.Invoke(new Func<Task<ChocolateyDialogController>>(async () =>
            {
                // create the dialog control
                var dialog = new ChocolateyDialog(this)
                {
                    Title = title,
                    IsCancelable = isCancelable,
                    OutputBufferCollection = _progressService.Output
                };

                if (settings == null)
                {
                    settings = MetroDialogOptions;
                }

                dialog.NegativeButtonText = settings.NegativeButtonText;

                await this.ShowMetroDialogAsync(dialog);
                return new ChocolateyDialogController(dialog, () => this.HideMetroDialogAsync(dialog));
            }));
        }

        private void SettingsButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.Width = Width / 3;
            SettingsFlyout.IsOpen = !SettingsFlyout.IsOpen;
        }

        private void AboutButton_OnClick(object sender, RoutedEventArgs e)
        {
            AboutFlyout.Width = Width / 3;
            AboutFlyout.IsOpen = !AboutFlyout.IsOpen;
        }
        
        private void SourcesButton_OnClick(object sender, RoutedEventArgs e)
        {
            SettingsFlyout.IsOpen = false;
            SourcesFlyout.IsOpen = true;
            SourcesFlyout.IsOpenChanged += this.SourcesFlyout_IsOpenChanged;
        }

        private void SourcesFlyout_IsOpenChanged(object sender, EventArgs e)
        {
            if (SourcesFlyout.IsOpen == false)
            {
                SettingsFlyout.IsOpen = true;
                SourcesFlyout.IsOpenChanged -= this.SourcesFlyout_IsOpenChanged;
            }
        }
        
        private void CanGoToPage(object sender, CanExecuteRoutedEventArgs e)
        {
            // GEP: I can't think of any reason that we would want to prevent going to the linked
            // page, so just going to default this to returning true
            e.CanExecute = true;
        }

        private void PerformGoToPage(object sender, ExecutedRoutedEventArgs e)
        {
            // https://github.com/theunrepentantgeek/Markdown.XAML/issues/5
            Process.Start(new ProcessStartInfo(e.Parameter.ToString()));
            e.Handled = true;
        }        
    }
}