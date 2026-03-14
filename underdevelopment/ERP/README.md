Specifikáció

A projekt célja egy nagykereskedelmi vállalat működését támogató webalapú informatikai rendszer megvalósítása C# ASP.NET technológia felhasználásával.
A rendszer feladata a termékek nyilvántartása, a raktárkészlet kezelése, a megrendelések feldolgozása, a számlázás, valamint a szállítási folyamatok adminisztrációja,
továbbá a működéshez szükséges riportok és kezelőfelületek biztosítása.

A rendszer több felhasználói szerepkört különböztet meg, amelyek eltérő jogosultságokkal rendelkeznek.
A raktárkezelő felhasználó a készletmozgások kezeléséért és a raktári állapot nyomon követéséért felelős. 
A bevételezési jogosultsággal rendelkező felhasználó új termékek és beérkező szállítmányok rögzítését végezheti. 
A rendeléskezelő a vevői megrendelések feldolgozását, státuszkezelését és teljesítését végzi. 
Az adminisztrátor teljes körű hozzáféréssel rendelkezik a rendszer minden funkciójához, beleértve a felhasználók kezelését is. 
Az ügyfél szerepkörrel rendelkező felhasználók saját rendeléseiket tekinthetik meg, illetve új megrendeléseket adhatnak le.

A rendszer központi eleme a terméknyilvántartás, amely tartalmazza a termékek alapadatait, bevételezési dátumát, 
valamint az átlagárazás alapján számított aktuális beszerzési értéket. A termékek több kategóriába sorolhatók, 
például romlandó áruk, növények, nem romlandó termékek, veszélyes vegyszerek és élőlények. 
A kategorizálás lehetővé teszi a speciális kezelési szabályok alkalmazását, 
például lejárati idő figyelést vagy külön tárolási feltételeket.

A raktárkezelési modul biztosítja több raktár egyidejű kezelését, a készlet mennyiségi nyilvántartását, 
valamint a készletmozgások rögzítését. A rendszer képes kezelni a bevételezést, kiadást, 
valamint a raktárak közötti áthelyezéseket, miközben minden művelet naplózásra kerül. 
A készletérték számítása átlagár alapú módszerrel történik.

A megrendeléskezelő modul lehetővé teszi a vevői rendelések rögzítését, 
módosítását és státusz szerinti követését a létrehozástól a teljesítésig. 
A rendszer automatikusan ellenőrzi a készlet rendelkezésre állását, és szükség esetén készletfoglalást végez, 
emellett, ahol szükséges, ott figyelembe veszi az áru beszerzésének dátumát és ez alapján választja ki az elküldendő terméket. 
A megrendelésekhez kapcsolódóan a rendszer képes számlák generálására, 
amelyek tartalmazzák a tételeket, árakat, adókat és végösszegeket.

A számlázási modul feladata a rendelés adatai alapján történő számlagenerálás, a számlák nyilvántartása, 
valamint azok exportálása például PDF formátumban. A rendszer támogatja a fizetési státuszok kezelését is.

A szállítási modul kezeli a szállítók adatait, a rendelkezésre álló járműveket, valamint az egyes szállítmányokat.
A szállítmányok összekapcsolhatók megrendelésekkel, és nyomon követhető azok állapota a kiszállítási folyamat során.
A járművekhez kapcsolódó információk, például kapacitás vagy azonosító adatok szintén tárolásra kerülnek.

A rendszer több kezelőfelületet biztosít a különböző felhasználói szerepkörök számára, amelyek lehetővé teszik az adatok megtekintését, 
rögzítését és módosítását. A felhasználói felület célja az átlátható működés és a hatékony adminisztráció támogatása.

A riportkészítő modul különböző statisztikák és kimutatások előállítását teszi lehetővé, 
például készletérték jelentések, forgalmi kimutatások, rendelési statisztikák vagy szállítási teljesítmény elemzések formájában. 
Ezek a riportok segítik a vállalat működésének elemzését és a döntéshozatalt.

A rendszer relációs adatbázist használ az adatok tárolására, 
biztosítva az adatok konzisztenciáját és visszakereshetőségét, valamint támogatja a több felhasználó egyidejű munkavégzését.