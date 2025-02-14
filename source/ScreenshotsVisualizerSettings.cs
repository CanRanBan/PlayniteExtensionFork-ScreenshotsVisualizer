﻿using Playnite.SDK;
using ScreenshotsVisualizer.Models;
using ScreenshotsVisualizer.Views;
using Playnite.SDK.Data;
using System.Collections.Generic;
using ScreenshotsVisualizer.Models.StartPage;
using System.Linq;
using CommonPluginsShared.Extensions;
using System.Threading.Tasks;

namespace ScreenshotsVisualizer
{
    public class ScreenshotsVisualizerSettings : ObservableObject
    {
        #region Settings variables
        public bool MenuInExtensions { get; set; } = true;

        public bool EnableTag { get; set; } = false;


        public bool EnableIntegrationButtonHeader { get; set; } = false;
        public bool EnableIntegrationButtonSide { get; set; } = true;

        private bool _EnableIntegrationViewItem { get; set; } = true;
        public bool EnableIntegrationViewItem
        {
            get => _EnableIntegrationViewItem;
            set
            {
                _EnableIntegrationViewItem = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationButton { get; set; } = true;
        public bool EnableIntegrationButton
        {
            get => _EnableIntegrationButton;
            set
            {
                _EnableIntegrationButton = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationButtonDetails { get; set; } = false;
        public bool EnableIntegrationButtonDetails
        {
            get => _EnableIntegrationButtonDetails;
            set
            {
                _EnableIntegrationButtonDetails = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationShowSinglePicture { get; set; } = true;
        public bool EnableIntegrationShowSinglePicture
        {
            get => _EnableIntegrationShowSinglePicture;
            set
            {
                _EnableIntegrationShowSinglePicture = value;
                OnPropertyChanged();
            }
        }
        
        public double IntegrationShowSinglePictureHeight { get; set; } = 150;
        public bool OpenViewerWithOnSelectionSinglePicture { get; set; } = false;
        public bool AddBorderSinglePicture { get; set; } = true;
        public bool AddRoundedCornerSinglePicture { get; set; } = false;

        private bool _EnableIntegrationShowPictures { get; set; } = true;
        public bool EnableIntegrationShowPictures
        {
            get => _EnableIntegrationShowPictures;
            set
            {
                _EnableIntegrationShowPictures = value;
                OnPropertyChanged();
            }
        }

        private bool _EnableIntegrationShowPicturesVertical { get; set; } = true;
        public bool EnableIntegrationShowPicturesVertical
        {
            get => _EnableIntegrationShowPicturesVertical;
            set
            {
                _EnableIntegrationShowPicturesVertical = value;
                OnPropertyChanged();
            }
        }

        public double IntegrationShowPicturesHeight { get; set; } = 150;
        public bool LinkWithSinglePicture { get; set; } = false;
        public bool OpenViewerWithOnSelection { get; set; } = false;
        public bool AddBorder { get; set; } = true;
        public bool AddRoundedCorner { get; set; } = false;

        public bool HideScreenshotsInfos { get; set; } = false;

        public int JpgQuality { get; set; } = 98;

        public bool EnableFolderToSave { get; set; } = false;
        public string FolderToSave { get; set; } = string.Empty;
        public string FileSavePattern { get; set; } = string.Empty;

        public string GlobalScreenshootsPath { get; set; } = string.Empty;

        public bool UsedThumbnails { get; set; } = true;

        public List<GameSettings> gameSettings { get; set; } = new List<GameSettings>();


        private bool _CarouselAutoChangeEnable { get; set; } = true;
        public bool CarouselAutoChangeEnable
        {
            get => _CarouselAutoChangeEnable;
            set
            {
                _CarouselAutoChangeEnable = value;
                OnPropertyChanged();
            }
        }

        private int _CarouselAutoChangeTimer { get; set; } = 10;
        public int CarouselAutoChangeTimer
        {
            get => _CarouselAutoChangeTimer;
            set
            {
                _CarouselAutoChangeTimer = value;
                OnPropertyChanged();
            }
        }
        #endregion


        #region Settings StartPage
        private SsvCarouselOptions _ssvCarouselOptions { get; set; } = new SsvCarouselOptions();
        public SsvCarouselOptions ssvCarouselOptions
        {
            get => _ssvCarouselOptions;
            set
            {
                _ssvCarouselOptions = value;
                OnPropertyChanged();
            }
        }
        #endregion


        // Playnite serializes settings object to a JSON object and saves it as text file.
        // If you want to exclude some property from being saved then use `JsonDontSerialize` ignore attribute.
        #region Variables exposed
        private bool _HasData { get; set; } = false;
        [DontSerialize]
        public bool HasData
        {
            get => _HasData;
            set
            {
                _HasData = value;
                OnPropertyChanged();
            }
        }

        private List<Screenshot> _ListScreenshots { get; set; } = new List<Screenshot>();
        [DontSerialize]
        public List<Screenshot> ListScreenshots
        {
            get => _ListScreenshots;
            set
            {
                _ListScreenshots = value;
                OnPropertyChanged();
            }
        }
        #endregion  
    }


    public class ScreenshotsVisualizerSettingsViewModel : ObservableObject, ISettings
    {
        private readonly ScreenshotsVisualizer Plugin;
        private ScreenshotsVisualizerSettings EditingClone { get; set; }

        private ScreenshotsVisualizerSettings _Settings;
        public ScreenshotsVisualizerSettings Settings { get => _Settings; set => SetValue(ref _Settings, value); }


        public ScreenshotsVisualizerSettingsViewModel(ScreenshotsVisualizer plugin)
        {
            // Injecting your plugin instance is required for Save/Load method because Playnite saves data to a location based on what plugin requested the operation.
            Plugin = plugin;

            // Load saved settings.
            ScreenshotsVisualizerSettings savedSettings = plugin.LoadPluginSettings<ScreenshotsVisualizerSettings>();

            // LoadPluginSettings returns null if not saved data is available.
            if (savedSettings != null)
            {
                Settings = savedSettings;
            }
            else
            {
                Settings = new ScreenshotsVisualizerSettings();
            }

            // Manage source
            Task.Run(() => 
            {
                System.Threading.SpinWait.SpinUntil(() => API.Instance.Database.IsOpen, -1);
                API.Instance.Database.Sources.ForEach(x =>
                {
                    if (!Settings.ssvCarouselOptions.SourcesList.Any(y => y.Name.IsEqual(x.Name)))
                    {
                        Settings.ssvCarouselOptions.SourcesList.Add(new CommonPluginsShared.Models.CheckElement { Name = x.Name });
                    }
                });
                Settings.ssvCarouselOptions.SourcesList = Settings.ssvCarouselOptions.SourcesList.OrderBy(x => x.Name).ToList();
            });
        }

        // Code executed when settings view is opened and user starts editing values.
        public void BeginEdit()
        {
            EditingClone = Serialization.GetClone(Settings);
        }

        // Code executed when user decides to cancel any changes made since BeginEdit was called.
        // This method should revert any changes made to Option1 and Option2.
        public void CancelEdit()
        {
            Settings = EditingClone;
        }

        // Code executed when user decides to confirm changes made since BeginEdit was called.
        // This method should save settings made to Option1 and Option2.
        public void EndEdit()
        {
            Settings.gameSettings = new List<GameSettings>();
            foreach (ListGameScreenshot item in ScreenshotsVisualizerSettingsView.listGameScreenshots)
            {
                Settings.gameSettings.Add(new GameSettings
                {
                    Id = item.Id,
                    ScreenshotsFolders = item.ScreenshotsFolders,
                    UsedFilePattern = item.UsedFilePattern,
                    FilePattern = item.FilePattern,
                    ScanSubFolders = item.ScanSubFolders
                });
            }

            Plugin.SavePluginSettings(Settings);
            ScreenshotsVisualizer.PluginDatabase.PluginSettings = this;

            if (API.Instance.ApplicationInfo.Mode == ApplicationMode.Desktop)
            {
                Plugin.topPanelItem.Visible = Settings.EnableIntegrationButtonHeader;
                Plugin.ssvViewSidebar.Visible = Settings.EnableIntegrationButtonSide;
            }

            this.OnPropertyChanged();
        }

        // Code execute when user decides to confirm changes made since BeginEdit was called.
        // Executed before EndEdit is called and EndEdit is not called if false is returned.
        // List of errors is presented to user if verification fails.
        public bool VerifySettings(out List<string> errors)
        {
            errors = new List<string>();
            return true;
        }
    }
}
