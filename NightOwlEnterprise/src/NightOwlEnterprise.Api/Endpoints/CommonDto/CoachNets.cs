namespace NightOwlEnterprise.Api.Endpoints.CommonDto;

public class CoachTytNets
{
    //Anlam Bilgisi: (Max 40, Min 0)
    public byte Turkish { get; set; }
    //Matematik: (Max 30, Min 0)
    public byte Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte Geometry { get; set; }
    //Tarih: (Max 5, Min 0)
    public byte History { get; set; }
    //Coğrafya: (Max 5, Min 0)
    public byte Geography { get; set; }
    //Felsefe: (Max 5, Min 0)
    public byte Philosophy { get; set; }
    //Din: (Max 5, Min 0)
    public byte Religion { get; set; }
    //Fizik: (Max 7, Min 0)
    public byte Physics { get; set; }
    //Kimya: (Max 7, Min 0)
    public byte Chemistry { get; set; }
    //Biology: (Max 6, Min 0)
    public byte Biology { get; set; }
}

public class CoachMfNets
{
    //Matematik: (Max 30, Min 0)
    public byte Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte Geometry { get; set; }
    //Fizik: (Max 14, Min 0)
    public byte Physics { get; set; }
    //Kimya: (Max 13, Min 0)
    public byte Chemistry { get; set; }
    //Biology: (Max 13, Min 0)
    public byte Biology { get; set; }
}

public class CoachTmNets
{
    //Matematik: (Max 30, Min 0)
    public byte Mathematics { get; set; }
    //Geometri: (Max 10, Min 0)
    public byte Geometry { get; set; }
    //Edebiyat: (Max 24, Min 0)
    public byte Literature { get; set; }
    //Tarih: (Max 10, Min 0)
    public byte History { get; set; }
    //Coğrafya: (Max 6, Min 0)
    public byte Geography { get; set; }
}

public class CoachSozelNets
{
    //Tarih-1: (Max 10, Min 0)
    public byte History1 { get; set; }
    //Coğrafya: (Max 24, Min 0)
    public byte Geography1 { get; set; }
    //Edebiyat-1: (Max 6, Min 0)
    public byte Literature1 { get; set; }
    //Tarih-2: (Max 11, Min 0)
    public byte History2 { get; set; }
    //Coğrafya-2: (Max 11, Min 0)
    public byte Geography2 { get; set; }
    //Felsefe: (Max 12, Min 0)
    public byte Philosophy { get; set; }
    //Din: (Max 6, Min 0)
    public byte Religion { get; set; }
}

public class CoachDilNets
{
    //YDT: (Max 80, Min 0) Yabacnı Dil Testi
    public byte YDT { get; set; }
}