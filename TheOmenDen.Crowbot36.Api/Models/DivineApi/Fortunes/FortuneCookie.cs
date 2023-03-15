namespace TheOmenDen.CrowBot36.Models.DivineApi.Fortunes;

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
    public string result { get; set; }
}

