namespace TheOmenDen.CrowBot36.Models.DivineApi.Tarot.Daily;

public class Rootobject
{
    public int success { get; set; }
    public string message { get; set; }
    public Data data { get; set; }
}

public class Data
{
    public Prediction prediction { get; set; }
}

public class Prediction
{
    public string card { get; set; }
    public string category { get; set; }
    public string career { get; set; }
    public string love { get; set; }
    public string finance { get; set; }
    public string image { get; set; }
}
