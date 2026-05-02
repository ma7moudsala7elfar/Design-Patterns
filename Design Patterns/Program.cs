using System;

public class AppConfig
{
    private static AppConfig instance;
    private static readonly object lockObj = new object();

    private AppConfig()
    {
        Theme = "Light";
        Language = "English";
    }

    public static AppConfig GetInstance()
    {
        lock (lockObj)
        {
            if (instance == null)
            {
                instance = new AppConfig();
            }
        }
        return instance;
    }

    public string Theme { get; set; }
    public string Language { get; set; }
}

class Program
{
    static void Main(string[] args)
    {
        var config1 = AppConfig.GetInstance();
        Console.WriteLine("From Config1");
        Console.WriteLine(config1.Theme);
        Console.WriteLine(config1.Language);


        config1.Theme = "Dark";
        config1.Language = "Arabic";

        var config2 = AppConfig.GetInstance();
        Console.WriteLine("From Config2");
        Console.WriteLine(config2.Theme);
        Console.WriteLine(config2.Language);

    }
}