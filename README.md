# 7 Days to Die — RealTimeHorde MOD

現実の曜日・時刻にホードナイト（BloodMoon）を連動させる Harmony C# MOD。

## 機能

- 毎週水曜22時・土曜21時など、現実のカレンダーに合わせてホードが来る
- 複数スケジュールを XML で自由に設定可能
- 既存の7日周期ホードは完全無効化
- ホード前に警告テキストをゲーム内に表示

## 動作環境

- 7 Days to Die v1.x（Steam版 / Windows）
- シングルプレイ対応（マルチ未保証）

## インストール

1. [Releases](https://github.com/sue02asuka/7d2d-realtimehorde/releases) から `RealTimeHorde-vX.X.X.zip` をダウンロード
2. 解凍して `RealTimeHorde` フォルダを以下に配置：
   ```
   C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\
   ```
3. ゲームを起動してMODリストに `RealTimeHorde` が表示されれば完了

## スケジュール設定

`Mods\RealTimeHorde\Config\horde-schedule.xml` を編集：

```xml
<HordeSchedule>
  <!-- 水曜22:00（30分前に警告） -->
  <HordeEvent dayOfWeek="Wednesday" hour="22" minute="0" warningMinutes="30" />
  <!-- 土曜21:00（60分前に警告） -->
  <HordeEvent dayOfWeek="Saturday" hour="21" minute="0" warningMinutes="60" />
</HordeSchedule>
```

## 開発者向け

- [セットアップ手順書](docs/setup-guide.md)
- [API調査メモ](docs/api-research.md)
- [要件定義書](https://github.com/sue02asuka/sue02AI/blob/master/output/requirements/2026-04-23-7D2D-RealTimeHorde.md)（別リポジトリ）

## ライセンス

MIT
