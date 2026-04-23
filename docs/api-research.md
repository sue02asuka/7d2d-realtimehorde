# BloodMoon API 調査結果

**調査日**: 2026-04-23
**ゲームバージョン**: 7 Days to Die v1.x (Steam)
**DLL**: `7DaysToDie_Data/Managed/Assembly-CSharp.dll`
**調査方法**: PowerShell リフレクション（`[Reflection.Assembly]::LoadFrom`）

---

## Issue #1 承認条件：完了 ✅

---

## 主要クラス一覧

| クラス名 | 役割 |
|---------|------|
| `AIDirector` | AIの統括管理クラス。各コンポーネントのコンテナ |
| `AIDirectorBloodMoonComponent` | **BloodMoon制御の本体** |
| `AIDirectorBloodMoonParty` | BloodMoon時の敵パーティ管理 |
| `AIDirectorHordeComponent` | ワンダリングホード制御 |

---

## BloodMoon発動のアクセスチェーン

```csharp
// インスタンス取得
var bloodMoon = GameManager.Instance.World.GetAIDirector().BloodMoonComponent;

// BloodMoon開始
bloodMoon.StartBloodMoon();

// BloodMoon終了
bloodMoon.EndBloodMoon();

// 現在BloodMoon中かどうか
bool isActive = bloodMoon.BloodMoonActive;
```

---

## AIDirectorBloodMoonComponent メソッド詳細

| メソッド | 戻り値 | 引数 | 用途 |
|---------|--------|------|------|
| `StartBloodMoon()` | void | なし | BloodMoonを開始する **← H-003で使用** |
| `EndBloodMoon()` | void | なし | BloodMoonを終了する **← S-005で使用** |
| `get_BloodMoonActive` | bool | なし | 現在BloodMoon中か **← H-004で使用** |
| `SetForToday(bool _keepNextDay)` | bool | keepNextDay | 今日をBloodMoonデーに設定 |
| `IsBloodMoonTime(UInt64 worldTime)` | bool | worldTime | ゲーム内時刻がBloodMoon時間か判定 **← H-001パッチ対象** |
| `CalcNextDay(bool isSeek)` | void | isSeek | 次のBloodMoonデーを計算 |
| `Tick(double _dt)` | void | dt | 毎フレーム呼ばれるティック |

---

## AIDirector インスタンスアクセス

```csharp
// World 経由でアクセス（推奨）
var aiDirector = GameManager.Instance.World.GetAIDirector();

// または内部フィールド直接アクセス（非推奨）
var aiDirector = (AIDirector)Traverse.Create(GameManager.Instance.World)
    .Field("aiDirector").GetValue();
```

---

## 既存BloodMoon無効化の方針（H-001）

`IsBloodMoonTime()` に Prefix パッチを当て、RealTimeHorde が管理する場合は常に `false` を返す。

```csharp
[HarmonyPatch(typeof(AIDirectorBloodMoonComponent), "IsBloodMoonTime")]
public class DisableVanillaBloodMoonPatch
{
    public static bool Prefix(ref bool __result)
    {
        // RealTimeHorde が発動管理中のときは元メソッドを実行
        if (BloodMoonController.IsRealTimeHordeActive) return true;

        // それ以外は常に false（バニラBloodMoonを無効化）
        __result = false;
        return false;
    }
}
```

---

## 参考：コンソールコマンド

`ConsoleCmdAIDirectorSpawnHorde` クラスが存在するため、ゲーム内コンソールで `spawnairdrop` 系コマンドが使える。テスト時に活用可能。

---

## 参考MOD

- [bloodmoon-server-status](https://github.com/jonathan-robertson/bloodmoon-server-status) — `StartBloodMoon()` / `EndBloodMoon()` のパッチ実装例（MIT License）
