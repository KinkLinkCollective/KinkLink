

using Newtonsoft.Json;

namespace KinkLinkServer.Domain;

[Serializable]
public class Configuration(
    string databaseConnectionString,
    string signingKey,
    string serverUrl)
{
    public static readonly string ConfigurationPath = "/app/config.json";

    public readonly string DatabaseConnectionString = databaseConnectionString;
    public readonly string SigningKey = signingKey;
    public readonly string ServerUrl = serverUrl;

    public static Configuration? Load()
    {
        try
        {
            if (!File.Exists(ConfigurationPath))
            {
                Console.WriteLine($"[Configuration] [Load] Configuration file does not exist at {ConfigurationPath}, the image configuration is incorrect, please fix.");
                return null;
            }

            var json = File.ReadAllText(ConfigurationPath);
            if (JsonConvert.DeserializeObject<Configuration>(json) is not { } configuration)
            {
                Console.WriteLine("[Configuration] [Load] Unable to deserialize configuration.");
                return null;
            }

            if (configuration.HasDefaultValues())
            {
                Console.WriteLine($"[Configuration] [Load] Connection string not set in {ConfigurationPath}, please edit the values and run the application again.");
                return null;
            }

            Console.WriteLine("[Configuration] [Load] Configuration successfully loaded.");
            return configuration;
        }
        catch (Exception e)
        {
            Console.WriteLine($"[Configuration] [Load] An unknown error occured, {e.Message}.");
            return null;
        }
    }

    private bool HasDefaultValues() => DatabaseConnectionString is Empty || SigningKey is Empty || ServerUrl is Empty;

    private const string Empty = "Empty";
    private static readonly Configuration Default = new(Empty, Empty, Empty);
}
