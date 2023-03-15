namespace TheOmenDen.CrowBot36.Models.DivineApi.Coffee;

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
    public string present_title { get; set; }
    public string present_image { get; set; }
    public string present_content { get; set; }
    public string near_future_title { get; set; }
    public string near_future_image { get; set; }
    public string near_future_content { get; set; }
    public string distant_future_title { get; set; }
    public string distant_future_image { get; set; }
    public string distant_future_content { get; set; }
}

