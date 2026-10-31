Tento projekt tvoří kostru semestrální práce KIV/UPG 2026/2027 a je k dispozici ke stažení na https://gitlab.kiv.zcu.cz/UPG/spacetraffic.git. 

Projekt je plně využitelný pro studenty, kteří se rozhodnou semestrální práci vypracovat v C#, a to s využitím Avalonia UI nebo WinForms. Tito studenti jednoduše přejmenují src/SpaceTraffic_Avalonia nebo src/SpaceTraffic_WinForms na src/SpaceTrafic a druhý adresář zcela smažou. Ostatní studenti z projektu využijí pouze data a příp. instrukce k dokumentaci.

*UPOZORNĚNÍ:* Před odevzdáním (na https://portal.zcu.cz/portal/studium/moje-vyuka/odevzdavani-praci.html) odstraňte všechny soubory, které nejsou pro odevzdávanou práci relevantní, tj. např. pokud pro dokumentaci jste šablonu dokumentace.dot nevyužili, ale využili jste pouze dokumentace.docx, ponechte soubor .docx; analogicky, pokud Vaše práce má být spuštěna pod Windows, ponechte skripty Run.cmd a Build.cmd, pokud pod Linux/Mac, ponechte skripty Run.sh a Build.sh, oba skripty ponechte, pokud práci lze spouštět jak na Windows tak na Linux/Mac.

*UPOZORNĚNÍ:* Kostra se v průběhu semestru může změnit. Studentům je proto doporučeno, aby pro svou práci využili verzovací systém Git, a to tak, že mají dva vzdálené repozitáře: vlastní origin pro FETCH/PULL/PUSH, založený např. na github / gitlab, a upstream pro FETCH/PULL vedoucí na původní zdroj a na výzvu přednášejícího/cvičícího provedli FETCH/PULL z upstream.



Návod pro použití s GIT pro úplné začátečníky
---------------------------------------------
1. Pokud již máte zřízen někde GIT účet umožňující vám zakládat soukromé (private) repozitáře, jděte na krok 3
2. Založte si účet na https://gitlab.com/ nebo https://bitbucket.org nebo https://github.com. 
3. Založte nový SOUKROMÝ (private) repozitář a nějak vhodně si ho pojmenujte. Rady, ať založíte .gitignore nebo soubor readme, ignorujte - vy již máte svůj existující projekt.
4. Získejte HTTPS adresu k vašemu repozitáři (bývá zřetelně uvedena).
5. Pokud jste tento projekt získali doporučeným klonováním z Git (máte zde skrytý podadresář .git), jděte na krok 8.
6. Prostřednictvím TortoiseGit (vyvolá se z kontextového menu v průzkumníkovi) nebo Git Extensions (či jiných) založte lokální repozitář v tomto adresáři (Git Create repository here ...). Od této chvíle můžete provádět "commit" a uchovávat lokálně změny.
7. Vyvolejte Git Commit a všechny soubory "commitujte" do lokálního repozitáře (počáteční/první commit).
8. Vyvolejte Push a v nastavení "Remote" přidejte nový vzdálený repozitář s názvem "origin" a jako URL volte tu, kterou jste získali v kroku 4 (tj. HTTPS adresu k vašemu repozitáři). Dokončete Push. TortoiseGit si během toho vyžádá vaše přihlašovací údaje a všechny vaše změny (lokálně vedené v podadresáři .git) zkopíruje do vzdáleného repozitáře.
8. Zkontrolujte, že data jsou skutečně uložena.

Poznámka: využijte soubor .gitignore pro specifikaci automaticky generovaných souborů (.class, javadoc dokumentace, apod.), aby se tyto soubory neukládaly do Git repozitářů.
