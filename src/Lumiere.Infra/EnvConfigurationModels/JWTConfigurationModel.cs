namespace Lumiere.Infra.EnvConfigurationModels
{
    public class JWTConfigurationModel
    {
        public string JwtKey { get; set; }
        public int JWTExpireTimeInMinutes { get; set; }
    }
}
