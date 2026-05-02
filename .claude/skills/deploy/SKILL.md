---
name: deploy
description: Degisiklikleri GitHub'a push et ve projeyi publish et (sadece en/tr dil dosyalari)
user-invocable: true
allowed-tools: Bash(git *), Bash(dotnet *)
argument-hint: "<commit mesaji>"
---

Asagidaki adimlari sirayla uygula.

## Kullanim

```
/deploy "Yeni özellik eklendi"
```

Arguman verilmezse kullanicidan commit mesajini sor.

## Terminal Komutlari (referans)

```bash
# 1. Tüm degisiklikleri stage'e al
git add -A

# 2. Commit olustur (mesaji argumandan al)
git commit -m "<commit mesaji>"

# 3. GitHub'a push et
git push origin main

# 4. Projeyi publish et — sadece en ve tr dil dosyalariyla
#    SatelliteResourceLanguages: publish ciktisindaki culture klasorlerini filtreler
#    (en/, tr/ disindaki tum .resources.dll klasorleri dahil edilmez)
dotnet publish PlaygroundDashboard.csproj -c Release -p:SatelliteResourceLanguages=en%3Btr
# NOT: Noktalı virgül (;) Windows'ta MSBuild'e geçerken %3B olarak escape edilmeli
```

## Adimlar

### Adim 1 — Commit mesajini belirle

- Skill argumani verilmisse onu kullan
- Verilmemisse kullaniciya sor: "Commit mesajini girin:"

### Adim 2 — GitHub push

Asagidaki komutlari sirayla calistir:

```bash
git add -A
```

```bash
git commit -m "<commit mesaji>"
```

```bash
git push origin main
```

Her komutun ciktisini kontrol et. Hata varsa kullaniciya bildir ve dur.

### Adim 3 — Publish

```bash
dotnet publish PlaygroundDashboard.csproj -c Release -p:SatelliteResourceLanguages=en%3Btr
# NOT: Noktalı virgül (;) Windows'ta MSBuild'e geçerken %3B olarak escape edilmeli
```

Build/publish hatasi varsa tam hata metnini kullaniciya goster.

### Adim 4 — Rapor

Basarili oldugunda kullaniciya ozet ver:
- Push edilen branch ve commit hash
- Publish cikti klasoru (genellikle `bin/Release/net*/publish/`)
- Herhangi bir uyari varsa listele
