namespace NightOwlEnterprise.Api;

public class Common
{
    public static List<string> Cities = new List<string>()
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Amasya",
        "Ankara", "Antalya", "Artvin", "Aydın", "Balıkesir",
        "Bilecik", "Bingöl", "Bitlis", "Bolu", "Burdur",
        "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
        "Diyarbakır", "Edirne", "Elazığ", "Erzincan", "Erzurum",
        "Eskişehir", "Gaziantep", "Giresun", "Gümüşhane", "Hakkâri",
        "Hatay", "Isparta", "İçel (Mersin)", "İstanbul", "İzmir",
        "Kars", "Kastamonu", "Kayseri", "Kırklareli", "Kırşehir",
        "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa",
        "Kahramanmaraş", "Mardin", "Muğla", "Muş", "Nevşehir",
        "Niğde", "Ordu", "Rize", "Sakarya", "Samsun",
        "Siirt", "Sinop", "Sivas", "Tekirdağ", "Tokat",
        "Trabzon", "Tunceli", "Şanlıurfa", "Uşak", "Van",
        "Yozgat", "Zonguldak", "Aksaray", "Bayburt", "Karaman",
        "Kırıkkale", "Batman", "Şırnak", "Bartın", "Ardahan",
        "Iğdır", "Yalova", "Karabük", "Kilis", "Osmaniye", "Düzce"
    };

    public static List<(string, bool)> NamesAndGender = new List<(string, bool)>
    {
        ("Ahmet", true), ("Mehmet", true), ("Mustafa", true), ("Ali", true), ("Hüseyin", true), ("Hasan", true),
        ("İbrahim", true), ("Ömer", true), ("Osman", true), ("İsmail", true),
        ("Mahmut", true), ("Yusuf", true), ("İsmet", true), ("Adem", true), ("Kemal", true), ("Turgut", true),
        ("Kadir", true), ("Hakan", true), ("Erdem", true), ("Ertuğrul", true),
        ("Süleyman", true), ("Yunus", true), ("Kerim", true), ("Murat", true), ("Selim", true), ("Berkay", true),
        ("Kaan", true), ("Kerem", true), ("Emre", true), ("Cem", true),
        ("Ayşe", false), ("Fatma", false), ("Emine", false), ("Hatice", false), ("Zeynep", false), ("Hacer", false),
        ("Melek", false), ("Hülya", false), ("Gül", false), ("Şükran", false),
        ("Merve", false), ("Melike", false), ("Gizem", false), ("Esra", false), ("Sibel", false), ("Aynur", false),
        ("Zeliha", false), ("Gülay", false), ("Elif", false), ("Nesrin", false),
        ("Yasemin", false), ("Nur", false), ("Derya", false), ("Şeyma", false), ("Selma", false), ("Duygu", false),
        ("Büşra", false), ("Nazlı", false), ("Sema", false), ("Dilek", false),
        ("Rabia", false), ("Sultan", false), ("Rumeysa", false), ("Nurdan", false), ("Özlem", false),
        ("Serpil", false), ("Tuğba", false), ("Hilal", false), ("Gönül", false),
        ("Songül", false), ("Tülay", false), ("Zehra", false), ("Özge", false), ("Elvan", false),
        ("Aslı", false), ("Fulya", false), ("Berrin", false), ("Fadime", false),
        ("Münevver", false), ("Leyla", false), ("Pınar", false), ("Yeter", false), ("Burcu", false), ("Gülcan", false),
        ("Tuba", false), ("Gülsüm", false), ("Serap", false), ("Gülnur", false),
        ("Gülizar", false), ("Süheyla", false), ("Süreyya", false), ("Gülay", false), ("Gülçin", false),
        ("Nermin", false), ("Esma", false), ("Feride", false), ("Tansu", false), ("Tansel", false),
        ("Tanseli", false), ("Eylem", false), ("Feraye", false), ("Gülfem", false), ("Gülistan", false),
        ("Hafize", false), ("Kadriye", false), ("Nimet", false), ("Saadet", false), ("Zehra", false),
        ("Lale", false), ("Sevda", false), ("Nuray", false), ("Nergiz", false),
        ("Müge", false), ("Yurdagül", false), ("Yücel", false), ("Zeki", false),
        ("Deniz", false), ("Bülent", false), ("Yasin", false), ("Uğur", false), ("Onur", true), ("Sedat", true),
        ("Halis", true), ("Şaban", true), ("Özgür", true), ("Necati", false), ("Şevket", false),
        ("Gökhan", true), ("Alper", true), ("Oğuz", true), ("Fikret", true), ("Metin", true), ("Halil", true),
        ("Erkan", true), ("Emir", true), ("Kazım", true), ("Berkant", true),
        ("Emrah", true), ("Olcay", true), ("Nihat", true), ("Sercan", true), ("Yavuz", true), ("Fırat", true),
        ("İsmet", true), ("Ahmet", true), ("Kadir", true),
        ("Cihan", true), ("Can", true), ("Mustafa", true),
        ("Hüseyin", true), ("Ali", true), ("Ömer", true), ("Yusuf", true),
        ("Hasan", true), ("İsmail", true), ("Mahmut", true), ("Adem", true),
        ("Murat", true), ("Turgut", true), ("Taylan", true), ("Gürkan", true),
        ("Ertuğrul", true), ("Selim", true), ("Osman", true), ("Kerim", true),
        ("Berkay", true), ("Kerem", true), ("Emre", true),
        ("Cem", true), ("Cihan", true), ("Yunus", true),
        ("Barış", true), ("Furkan", true), ("Mert", true),
        ("Emin", true), ("Kaan", true), ("Orhan", true),
        ("Tarık", true), ("Onur", true), ("Deniz", false),
        ("Berk", true), ("Bülent", false), ("Yasin", false),
        ("Uğur", false), ("Sedat", true), ("Halis", true),
        ("Şaban", true), ("Özgür", true), ("Necati", false),
        ("Şevket", false), ("Gökhan", true), ("Alper", true),
        ("Oğuz", true), ("Fikret", true), ("Metin", true),
        ("Halil", true), ("Erkan", true), ("Emir", true),
        ("Kazım", true), ("Berkant", true), ("Emrah", true),
        ("Olcay", true), ("Nihat", true), ("Sercan", true),
        ("Fırat", true), ("Yavuz", true)
    };

}