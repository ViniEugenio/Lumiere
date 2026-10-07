namespace Lumiere.Infra.EnvConfigurationModels
{
    public class SecurityModel
    {
        public string Pepper { get; set; }
        public JWTConfigurationModel JWTConfigurations { get; set; }
    }
}
