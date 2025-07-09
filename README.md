# Project

“VegShop” proyekti altı moduledan ibarətdir: Models, Services, Utils, UI, References və Program.

Models qovluğunda yer alan `Customer` və `Vegetable` classları sistemin obyekt modelini təşkil edir. `Customer` classı customerin adını və istədiyi tərəvəz növünü sadə constructor və `ToString()` metodu ilə saxlayır. `Vegetable` obyekti isə ad, vəziyyət (`Condition` enum: Fresh, Normal, Rotten, Toxic), yaş, miqdar, 1 kiloqramının qiyməti və əlavə olunduğu vaxt kimi məlumatları özündə saxlayır. Onun `AgeOneDay()` methodu yaşlanmanı və vəziyyət dəyişikliyini göstərir, `TryTake()` isə anbardan istənilən miqdarda tərəvəz götürməyə imkan verir.

Services qovluğunda `VegetableStandManager`, `CustomerQueueManager`, `CustomerGenerator`, `CustomerProcessor` və `EpidemicManager` classları yerləşir. `VegetableStandManager` anbar idarəçiliyini həyata keçirir: yeni çatdırılmaları qəbul edir, köhnə çürük və yoluxmuş məhsulları münasib şəkildə atır, satış həcmi və reytinqə əsasən bonus və restock mexanizmlərini icra edir. `CustomerQueueManager` növbəni izləyir, satış statistikalarını toplayır və `ExportStatisticsToFile()` ilə hesabat yaradır. `CustomerGenerator` fonda System.Timers.Timer əsasında müştəri yaradılmasını avtomatlaşdırır; reytinqə görə dinamik interval təyin edir və epidemiya zamanı *Observer pattern ilə yaradılmanı müvəqqəti dayandırır. `CustomerProcessor` isə hər 3 saniyədə bir `ProcessNextCustomer()` çağırışı ilə növbədən müştəri götürüb emal edir. `EpidemicManager` epidemiya dövrlərini təsadüfi və ya əl ilə (StartManually) başlatmaq, gün sayını idarə etmək və hadisələr (`EpidemicStarted`, `EpidemicEnded`) vasitəsilə generatoru dayandırmaq funksiyalarını təmin edir.

Utils qovluğunda `IConsoleWriter` interfeysi və onun `SafeConsoleWriter` implementasiyası var. Bu modul konsola çıxışı sinxronlaşdırır (**`lock` mexanizmi ilə) və menyu ekrana çıxan zaman arxa fon mesajlarını susdurur (via `ConsoleControl.IsUserViewing`). Eyni kod həm test mühitində, həm də real konsol tətbiqində rahat istifadə oluna bilər.

UI qovluğunda yerləşən `MenuService` classı aşağı-yuxarı ox düymələri ilə interaktiv menyu təqdim edir: menyunun başlığı cyan rəngdə, seçilmiş element isə sarı rəng və “→” işarəsi ilə vurğulanır. İstifadəçi Enter düyməsini sıxdıqda həmin seçimə uyğun xidmət (`ShowShopStatus`, canlı simulyasiya, hesabat, stock baxışı və epidemiya başlanması) icra olunur.

References qovluğundakı `ProgramReferences` tək bir factory rolunu oynayır: bütün servislərin əsasını qoyur, aging, customer generation, customer processing və auto-restock timerlərini işə salır, ilkin anbar initializationu həyata keçirir və nəticədə `MenuService` obyektini qaytarır.

Nəhayət, Program.cs yeganə vəzifəsi `SafeConsoleWriter` və `Random` obyektlərini yaratmaq, `ProgramReferences.CreateMenuService()` metodunu çağırmaq və əldə olunan `MenuService.Run()` ilə tətbiqi işə salmaqdan ibarətdir. Bu dizayn composition root, ***dependency injection, Observer və ****Strategy patternlərinin kombinasiyasını təmin edərək həm sadə, həm də test edilə bilən, eyni zamanda iş prinsipi aydın və genişlənməsi asan bir arxitektura yaradır.

*****************************************************************************************************************************************************************************

Composition Root – bütün obyektlərin yaradılıb bir-birinə bağlandığı yeganə mərkəzdir. Bizdə bu rol “ProgramReferences”  classına verilib: burada bütün manager və service classları constructorda bir-birinə ötürülür, timerlər işə salınır və sonda bircə “MenuService” obyektini geri qaytarır. Beləliklə tətbiqin qurulma məntiqi yalnız bir yerdə cəmlənir və kodun qalan hissəsi bu əlaqələndirici detallardan xəbərsiz qalır.

Dependency Injection – classlar öz asılılıqlarını (məsələn, `CustomerGenerator` → `CustomerQueueManager`, `EpidemicManager`, `IConsoleWriter`) constructorda qəbul edir və özləri `new` açmırlar. Bu yanaşma classları bir-birindən zəif asılı(loose coupling) edir, test üçün saxta (mock) obyektlər ötürməyə imkan verir və ümumən kodu genişləndirməyi asanlaşdırır. 

Observer Pattern – hadisə mexanizmi ilə bir obyektin dəyişikliklərinə digərləri reaksiya verir. `EpidemicManager` iki event təqdim edir: `EpidemicStarted` və `EpidemicEnded`. `CustomerGenerator` bu event-lərə subscribe olur, `_isPaused` bayrağını tənzimləyir və epidemiya aktiv olanda yeni müştəri yaratmır. Eyni üsul `Vegetable.Aged` event-i ilə də mümkündür (tərəvəz yaşlananda başqa xidmət xəbərdar ola bilər).

Strategy Pattern – icra zamanı seçilən alqoritm ailələrini kapsullaşdırmaqdır. `CustomerGenerator`-da `CalculateIntervalBasedOnRating()` metodu cari reytinqə baxıb timer intervalını 1, 2, 3 və ya 5 saniyəyə təyin edir. Bundan ötrü ayrıca strategiya obyektinə ehtiyac yoxdur, amma funksiya eyni pattern prinsiplərini (dinamik seçim) yerinə yetirir. Nəticədə mağaza yüksək reytinqdə daha tez, aşağı reytinqdə daha ləng müştəri qəbul edir.

*****************************************************************************************************************************************************************************

Layihə müxtəlif timerlər və arxa fon prosessləri (məsələn, tərəvəzlərin yaşlanması, avtomatik restock, müştəri yaradılması, müştəri emalı) paralel icra olunduğuna görə bir anda bir neçə thread konsola çıxış verə bilər. Əgər bu çıxışlar idarə olunmasa, mətnlər üst-üstə düşər və menyu oxunmaz vəziyyətə gələr. Problemi həll etmək üçün əvvəlcə `IConsoleWriter` adlı bir interface müəyyənləşdirdim: o, cəmi üç metod saxlayır – `Write`, `WriteLine` və `SafeReadLine`. Sonra bu interfece implement eden `SafeConsoleWriter` classını yazdım. Class hər yazı əməliyyatını `lock(ConsoleControl.LockObj)` blokuna alır; eyni anda iki thread gələndə kilid bir‐birinin ardınca saxlanıldığı üçün konsol sətirləri düzgün qaydada görünür.

Bundan əlavə, menyu və ya status paneli ekranda olanda fon prosesslərinin mesajlarının qarışmaması üçün `ConsoleControl.IsUserViewing` adlı static flag istifadə olunur. `MenuService`-də menyu çəkilən kimi bu bayraq `true` edilir, `SafeConsoleWriter` isə daxilində “əgər IsUserViewing aktivdirsə, heç nə yazma” qaydasına malikdir; nəticədə fon mesajları menyu ekranını pozmur. İstifadəçi Enter basıb menyudan çıxanda bayraq yenidən `false` olur və fon prosesləri normal şəkildə yazmağa davam edir.

Beləliklə, `IConsoleWriter` vasitəsilə konsola çıxış funksiyası abstraksiya edildi, `SafeConsoleWriter` isə həmin abstraksiyanı thread-safe formada gerçəkləşdirdi. Bunun sayəsində proqramın istənilən nöqtəsində sadəcə `writer.WriteLine(...)` çağırmaq kifayətdir; paralel timerlər, lock mexanizmi və “IsUserViewing” bayrağı konsolun təmiz və ardıcıl görünməsini avtomatik təmin edir.

*****************************************************************************************************************************************************************************

`Proxy (Wrapper)` – `SafeConsoleWriter` classı `Console`-a birbaşa çıxışı örtür: hər `Write` və `WriteLine` çağırışından əvvəl `IsUserViewing` bayrağını yoxlayır, sonra `lock(ConsoleControl.LockObj)` ilə thread-safety təmin edir və sonda real `Console.Write(Line)`-ı çağırır. Məqsəd paralel timerlər yazı göndərəndə mətnlərin qarışmamasını və menyu ekranda ikən fon mesajlarının susdurulmasını təmin etməkdir.

`Observer` – `EpidemicManager` iki hadisə declare edir: `EpidemicStarted` və `EpidemicEnded`. `CustomerGenerator` bu hadisələrə subscribe edib `_isPaused` bayrağını tənzimləyir; epidemiya aktiv olanda yeni müştəri yaratmaq tam dayandırılır, bitəndə avtomatik bərpa olunur. Eyni model `System.Timers.Timer.Elapsed` eventləri ilə timer callbackləri üçün də işləyir.

`Strategy` – `CustomerGenerator.CalculateIntervalBasedOnRating()` mağazanın cari reytinqinə baxır və timer intervalını 1 s, 2 s, 3 s və ya 5 s seçir. Alqoritm icra vaxtında dəyişdiyi üçün müştəri axınının sürəti dinamik şəkildə reytinqə uyğun adaptasiya olunur.

`Simple Factory / Composition Root` – `ProgramReferences.CreateMenuService()` bütün service obyektlərinin (`EpidemicManager`, `VegetableStandManager`, `CustomerQueueManager`, `CustomerGenerator`, `CustomerProcessor`) instansiyasını yaradır, lazımi constructor asılılıqlarını ötürür, timerləri işə salır və tam qurulmuş `MenuService` qaytarır. Beləcə obyektin yığılması vahid bir mərkəzdə cəmlənir və qalan kod həmin instansiyalardan hazır şəkildə istifadə edir.
