using System;
using WeifenLuo.WinFormsUI.Docking;
using WeifenLuo.WinFormsUI.ThemeVS2015;

namespace CodeWalker.Properties
{


    // This class allows you to handle specific events on the settings class:
    //  The SettingChanging event is raised before a setting's value is changed.
    //  The PropertyChanged event is raised after a setting's value is changed.
    //  The SettingsLoaded event is raised after the setting values are loaded.
    //  The SettingsSaving event is raised before the setting values are saved.
    public sealed partial class Settings
    {

        public Settings()
        {
            // // To add event handlers for saving and changing settings, uncomment the lines below:
            //
            // this.SettingChanging += this.SettingChangingEventHandler;
            //
            // this.SettingsSaving += this.SettingsSavingEventHandler;
            //
        }

        private void SettingChangingEventHandler(object sender, System.Configuration.SettingChangingEventArgs e)
        {
            // Add code to handle the SettingChangingEvent event here.
        }

        private void SettingsSavingEventHandler(object sender, System.ComponentModel.CancelEventArgs e)
        {
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

        public static string[] renderLodNames =
        [
            "ORPHANHD",
            "HD",
            "LOD",
            "SLOD1",
            "SLOD2",
            "SLOD3",
            "SLOD4"
        ];

        public static CodeWalker.GameFiles.rage__eLodType[] renderLodValues =
        [
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_ORPHANHD,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_HD,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_LOD,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_SLOD1,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_SLOD2,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_SLOD3,
            CodeWalker.GameFiles.rage__eLodType.LODTYPES_DEPTH_SLOD4,
        ];

        public CodeWalker.GameFiles.rage__eLodType GetRenderworldMaxLOD(string lodlevel)
        {
            var indexOf = Array.IndexOf(renderLodNames, lodlevel);
            return renderLodValues[Math.Max(0, indexOf)];
        }
    }
}
