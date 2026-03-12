using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace CodeWalker.Properties {
    
    
    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings {
        
        public Settings() {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }
        
        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e) {
            // Add code to handle the SettingChangingEvent event here.
        }
        
        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e) {
            // Add code to handle the SettingsSaving event here.
        }

        public VS2015ThemeBase GetProjectWindowTheme()
        {
            return GetProjectWindowTheme(ProjectWindowTheme);
        }

        public VS2015ThemeBase GetProjectWindowTheme(string theme)
        {
            switch (theme)
            {
                default:
                case "Blue":
                    return new VS2015BlueTheme();
                case "Light":
                    return new VS2015LightTheme();
                case "Dark":
                    return new VS2015DarkTheme();
            }
        }
    }
}
