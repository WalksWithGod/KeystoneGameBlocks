namespace Settings
{
    public delegate void ConfigurationChanged(object sender, ConfigurationChangedArgs args);

    public class ConfigurationChangedArgs : System.EventArgs
    {

    }

    public interface IConfigurable
    {
        string ConfigurationName {get; set;}
        bool ReadConfiguration(Initialization ini);
        bool WriteConfiguration(Initialization ini);
        event ConfigurationChanged ConfigurationChangedEvent;

    }
}
