namespace TheOmenDen.CrowBot36.Models.DivineApi.Horoscopes.Daily;

public class Rootobject
{
    public int success { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public string sign { get; set; }
    public Prediction prediction { get; set; }
}

public class Prediction
{
    public string personal { get; set; }
    public string health { get; set; }
    public string profession { get; set; }
    public string emotions { get; set; }
    public string travel { get; set; }
    public string[] luck { get; set; }
}

