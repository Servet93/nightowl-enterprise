namespace NightOwlEnterprise.Api.Endpoints.CommonDto;

public class PrivateTutoringTYTObject
{
    public bool Turkish { get; set; }
    public bool Mathematics { get; set; }
    public bool Geometry { get; set; }
    public bool History { get; set; }
    public bool Geography { get; set; }
    public bool Philosophy { get; set; }
    public bool Religion { get; set; }
    public bool Physics { get; set; }
    public bool Chemistry { get; set; }
    public bool Biology { get; set; }
}
    
public class PrivateTutoringAYTObject
{
    public MF? Mf { get; set; }
    public TM? Tm { get; set; }
    public Sozel? Sozel { get; set; }
    public Dil? Dil { get; set; }
}

public class MF
{
    public bool Mathematics { get; set; }
    public bool Geometry { get; set; }
    public bool Physics { get; set; }
    public bool Chemistry { get; set; }
    public bool Biology { get; set; }
}
    
public class TM
{
    //Matematik: (Max 30, Min 0)
    public bool Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public bool Geometry { get; set; }
    //Edebiyat: (Max 24, Min 0)
    public bool Literature { get; set; }
    //Tarih: (Max 10, Min 0)
    public bool History { get; set; }
    //Coğrafya: (Max 6, Min 0)
    public bool Geography { get; set; }
}
    
//Sözel
public class Sozel
{
    //Tarih-1: (Max 10, Min 0)
    public bool History1 { get; set; }
    //Coğrafya: (Max 24, Min 0)
    public bool Geography1 { get; set; }
    //Edebiyat-1: (Max 6, Min 0)
    public bool Literature1 { get; set; }
    //Tarih-2: (Max 11, Min 0)
    public bool History2 { get; set; }
    //Coğrafya-2: (Max 11, Min 0)
    public bool Geography2 { get; set; }
    //Felsefe: (Max 12, Min 0)
    public bool Philosophy { get; set; }
    //Din: (Max 6, Min 0)
    public bool Religion { get; set; }
}
    
public class Dil
{
    //YDT: (Max 80, Min 0)
    public bool YTD { get; set; }
}