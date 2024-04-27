namespace NightOwlEnterprise.Api;

public static class DateTimeExtensions
{
    public static List<DateTime> GetWeekDays(this DateTime date)
    {
        var weekDays = new List<DateTime>();
        
        // Bugünkü tarihi al
        DateTime now = DateTime.Now;

        // Mevcut haftanın başlangıç gününü bul
        DateTime startOfWeek = now.AddDays(-(int)now.DayOfWeek);

        // Haftanın başlangıç tarihinden başlayarak günleri çıkart
        // Haftanın Başlangıç günü Pazar,Bitiş günü c.tesi
        for (int i = 0; i < 7; i++)
        {
            DateTime day = startOfWeek.AddDays(i);
            weekDays.Add(day);
        }

        return weekDays;
    }
}