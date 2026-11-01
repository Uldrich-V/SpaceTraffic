SpaceTraffic 🚀
Interaktivní 2D simulátor vesmírné dopravní sítě vyvíjený jako semestrální práce v rámci předmětu KIV/UPG (Uživatelská rozhraní v počítačové grafice) na Západočeské univerzitě v Plzni.

Autoři: Vojtěch Uldrich, Jáchym Růt

Technologie: .NET (C#), Avalonia UI

🌌 O projektu
Aplikace simuluje a vizualizuje vesmírnou dopravu v planetárním systému. Sleduje pohyb vesmírných těles po oběžných drahách, produkci nákladu na jednotlivých planetách/měsících a logistiku transportních lodí přepravujících zásoby mezi destinacemi.

Klíčové vlastnosti

Oddělená logika a renderování: Výpočet simulačního stavu probíhá v diskrétních krocích (1 den = 1 sekunda reálného času), zatímco vykreslování běžít plynule (~60 FPS).

Fyzikálně správná vizualizace: Tělesa obíhají po kruhových drahách s reálným oběhem proti směru hodinových ručiček.

Dynamické škálování: Celý vesmírný systém se automaticky přizpůsobuje velikosti okna bez zkreslení stran.

Logistika & Strategie lodí: Lodě autonomně nakládají a vykládají zboží na základě definovaných strategií (FULL, IMMEDIATE, MAX_LOADTIME) a vybírají další cíle podle pravděpodobnostní matice destinací.

Přehledné UI: Zobrazení simulačního data, vizuálních ukazatelů skladů (teploměrů), kapacity lodí a indikátorů právě probíhající nakládky/vykládky ($\uparrow$ / $\downarrow$).

🛠️ Požadavky a Spuštění
Aplikace vyžaduje nainstalované prostředí .NET SDK (verze 8.0 nebo novější).

Spuštění přes skript (doporučeno)
Windows:

DOS
Run.cmd [nazev_scenare]


📂 Struktura scénářů
Scénáře se nachází ve složce data/ a obsahují definice ve třech sekcích:

[BODIES] – Vesmírná tělesa, jejich poloměr oběhu, úhlová rychlost, velikost a produkce skladu.

[DESTINATIONS] – Možné trasy mezi tělesy s pravděpodobností jejich výběru.

[SHIPS] – Transportní lodě, jejich rychlost, kapacita, rychlost nakládky a zvolená strategie.

