using System.Configuration;

namespace AspNetClassicSessionState.AspNet
{

    /// <summary>
    /// Describes the ASP.Net Classic Session state configuration.
    /// </summary>
    public class AspNetClassicStateConfigurationSection : ConfigurationSection
    {

        /// <summary>
        /// Gets the <see cref="AspNetClassicStateConfigurationSection"/>.
        /// </summary>
        /// <returns></returns>
        public static AspNetClassicStateConfigurationSection DefaultSection => ((AspNetClassicStateConfigurationSection)ConfigurationManager.GetSection("aspNetClassicSessionState"));

        /// <summary>
        /// Determines whether ASP Classic Session state to ASP.Net forwarding is enabled.
        /// </summary>
        [ConfigurationProperty("enabled", DefaultValue = true)]
        public bool Enabled
        {
            get => (bool)this["enabled"];
            set => this["enabled"] = value;
        }

        /// <summary>
        /// Determines the prefix to apply to ASP Classic session variables within ASP.NET.
        /// </summary>
        [ConfigurationProperty("prefix", DefaultValue = "ASP_")]
        public string Prefix
        {
            get => (string)this["prefix"];
            set => this["prefix"] = value;
        }

    }

}
