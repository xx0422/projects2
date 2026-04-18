# Specifikáció

A projekt célja egy nagykereskedelmi vállalat működését támogató webalapú informatikai rendszer megvalósítása C# ASP.NET technológia felhasználásával.

A rendszer feladata a termékek nyilvántartása, a raktárkészlet kezelése, a megrendelések feldolgozása, a számlázás, valamint a szállítási folyamatok adminisztrációja, továbbá a működéshez szükséges riportok és kezelőfelületek biztosítása.

### 1. Felhasználói szerepkörök és jogosultságok

A rendszer több felhasználói szerepkört különböztet meg, amelyek eltérő jogosultságokkal rendelkeznek:

*   **Raktárkezelő:** A készletmozgások kezeléséért és a raktári állapot nyomon követéséért felelős.
*   **Bevételező:** Új termékek és beérkező szállítmányok rögzítését végezheti.
*   **Rendeléskezelő:** A vevői megrendelések feldolgozását, státuszkezelését és teljesítését végzi.
*   **Adminisztrátor:** Teljes körű hozzáféréssel rendelkezik a rendszer minden funkciójához, beleértve a felhasználók kezelését is.
*   **Ügyfél:** Saját rendeléseiket tekinthetik meg, illetve új megrendeléseket adhatnak le.

### 2. Terméknyilvántartás

A rendszer központi eleme a terméknyilvántartás, amely tartalmazza:

*   Termékek alapadatai.
*   Bevételezési dátum.
*   **Átlagárazás** alapján számított aktuális beszerzési érték.

A termékek az alábbi kategóriákba sorolhatók, ami lehetővé teszi speciális kezelési szabályok (pl. lejárati idő figyelés, külön tárolási feltételek) alkalmazását:

*   Romlandó áruk
*   Növények
*   Nem romlandó termékek
*   Veszélyes vegyszerek
*   Élőlények

### 3. Raktárkezelési modul

*   Több raktár egyidejű kezelése.
*   Készlet mennyiségi nyilvántartása.
*   Készletmozgások rögzítése (bevételezés, kiadás, raktárak közötti áthelyezés).
*   **Minden művelet naplózásra kerül.**
*   Készletérték számítása **átlagár alapú módszerrel** történik.

### 4. Megrendeléskezelő modul

*   Vevői rendelések rögzítése, módosítása és státusz szerinti követése (létrehozástól a teljesítésig).
*   A rendszer automatikusan ellenőrzi a készlet rendelkezésre állását.
*   **Készletfoglalás** végzése.
*   A termékek kiszállítása a beszerzés dátuma alapján (pl. FIFO elv) történik.

### 5. Számlázási modul

*   Számlák generálása a rendelés adatai alapján (tételek, árak, adók, végösszegek).
*   Számlák nyilvántartása.
*   Számlák exportálása (pl. **PDF formátumban**).
*   Fizetési státuszok kezelése.

### 6. Szállítási modul

*   Szállítók adatainak kezelése.
*   Rendelkezésre álló járművek (kapacitás, azonosító adatok) nyilvántartása.
*   Szállítmányok kezelése és összekapcsolása megrendelésekkel.
*   Szállítmányok állapotának nyomon követése a kiszállítási folyamat során.

### 7. Riportkészítő modul

A modul különböző statisztikák és kimutatások előállítását teszi lehetővé a döntéshozatal támogatására, például:

*   Készletérték jelentések
*   Forgalmi kimutatások
*   Rendelési statisztikák
*   Szállítási teljesítmény elemzések

### 8. Felhasználói felület és technológia

*   **Több kezelőfelület** a különböző felhasználói szerepkörök számára az adatok megtekintésére, rögzítésére és módosítására.
*   Cél az átlátható működés és a hatékony adminisztráció támogatása.
*   **Relációs adatbázis** használata az adatok konzisztenciájának és visszakereshetőségének biztosítására.
*   Több felhasználó egyidejű munkavégzésének támogatása.tékony adminisztráció támogatása.