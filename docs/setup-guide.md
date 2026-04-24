# セットアップ手順書（開発環境）

## 必要なもの

| ツール | 入手先 | 備考 |
|--------|--------|------|
| Visual Studio Community 2026 | https://visualstudio.microsoft.com/ja/vs/community/ | 無料・最新版（2022でも可） |
| 7 Days to Die（Steam版） | 所持済み | |
| .NET Framework 4.8 SDK | VS インストール時に自動取得 | |

## 手順

### Step 1: Visual Studio Community 2026 のインストール

1. 上記リンクからダウンロードして起動
2. 「ワークロード」タブで **「.NET デスクトップ開発」** にチェックを入れる
3. インストール完了まで待つ（10〜20分）

> ✅ **Visual Studio 2026 推奨**（2025年11月GA・最新版）。2022でも動作します。

### Step 2: プロジェクトを Visual Studio で開く

1. Visual Studio を起動
2. 「プロジェクトまたはソリューションを開く」をクリック
3. 以下のファイルを選択：
   ```
   C:\Users\sue02\sue02AI\03_projects\7d2d-realtimehorde\RealTimeHorde\RealTimeHorde.csproj
   ```

### Step 3: ビルド

1. メニュー → 「ビルド」→「ソリューションのビルド」（Ctrl+Shift+B）
2. 出力ウィンドウに `ビルドに成功しました` と表示されればOK
3. 自動的に以下にコピーされます：
   ```
   C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\RealTimeHorde\Harmony\RealTimeHorde.dll
   ```

### Step 4: 動作確認

1. 7 Days to Die を起動
2. メインメニュー → 「MOD」タブ → **RealTimeHorde** が表示されること ✅
3. ゲームを開始し、`output_log.txt` に以下のログが出ることを確認：
   ```
   [RealTimeHorde] 設定読込完了: 2件のスケジュール
   [RealTimeHorde] MOD初期化完了
   ```

### output_log.txt の場所
```
C:\Users\sue02\AppData\Roaming\7DaysToDie\logs\output_log__YYYY-MM-DD__HH-MM-SS.txt
```

---

## スケジュールの変更方法

以下のファイルをテキストエディタで開いて編集する：
```
C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\Mods\RealTimeHorde\Config\horde-schedule.xml
```

曜日の英語表記：
| 日本語 | 英語 |
|--------|------|
| 月曜日 | Monday |
| 火曜日 | Tuesday |
| 水曜日 | Wednesday |
| 木曜日 | Thursday |
| 金曜日 | Friday |
| 土曜日 | Saturday |
| 日曜日 | Sunday |

---

## よくある問題

### Q: ビルドエラー「参照が見つかりません」
- `Assembly-CSharp.dll` のパスが正しいか確認
- デフォルト: `C:\Program Files (x86)\Steam\steamapps\common\7 Days To Die\7DaysToDie_Data\Managed\`

### Q: ゲームMODリストにRealTimeHordeが表示されない
- `ModInfo.xml` が `Mods\RealTimeHorde\ModInfo.xml` に配置されているか確認

### Q: ホードが来ない
- `output_log.txt` で `[RealTimeHorde]` の行を確認
- スケジュール設定の曜日・時刻が現在時刻と一致しているか確認

### ⚠️ 注意事項
- **ファイルパスに日本語フォルダ名を使わない**（文字化けの原因）
- **BloodMoon系の他MODと併用しない**（競合の可能性）
- **テスト前にセーブデータをバックアップする**
