namespace NightOwlEnterprise.Api.Utils;

public class DateUtils
{
    public static DateTime FindDate(DayOfWeek day)
    {
        // Örnek bir gün
        DayOfWeek verilenGun = day;

        // Bugünkü tarih
        DateTime bugun = DateTime.UtcNow;

        // Verilen günün bugünkü tarihle karşılaştırılması
        int gunFarki = ((int)verilenGun - (int)bugun.DayOfWeek + 7) % 7;

        // İleride mi, geride mi, yoksa bugün mü olduğunun kontrolü ve tarih hesaplaması
        DateTime bulunanTarih;
        if (gunFarki == 0) // Bugün
        {
            // bulunanTarih = bugun;
            bulunanTarih = bugun.AddDays(7);
        }
        else if (gunFarki > 0) // İleride
        {
            bulunanTarih = bugun.AddDays(gunFarki);
        }
        else // Geride
        {
            bulunanTarih = bugun.AddDays(7 + gunFarki);
        }

        return bulunanTarih;
    }
}