# Cinemachine でのカメラ制御 導入編

## 概要

---

カメラは「置けば映る」ものではありません。プレイヤーを追いかけ、壁を避け、戦闘では少し引く。
これを自分でスクリプトに書くと、条件分岐がどこまでも増えていきます。

この回では、まず**カメラを手で書いてみて**、その後 Cinemachine に置き換えます。
順番が逆だと、Cinemachine のありがたみが分かりません。

> **今週の問い**：カメラを自分で書くと、何がそんなに大変なのか？

---

## 今週扱う内容

---

- **Cinemachine の導入**と、Cinemachine を使ったカメラ制御のやり方
- **Cinemachine なしのカメラ制御との対比**

---

## 演習シーン

---

**使用シーン**：`Assets/Application/Week02-Cinemachine/Week02-Cinemachine_Start.unity`
**完成見本**：`Assets/Application/Week02-Cinemachine/Week02-Cinemachine_Goal.unity`

> 先週欠席した人も `_Start` シーンから入れば合流できます。

---

## 授業の流れ

---

### Step 1: ○○

---

<本文>

### Step 2: ○○

---

<本文>

### Step 3: ○○

---

<本文>

---

## コード例

---

```csharp
namespace Week02
{
    public class Sample : MonoBehaviour
    {
    }
}
```

> **namespace は必ず `Week02` を付けてください。**
> 付けないと、同じクラス名を作った人とぶつかってプロジェクト全体がコンパイルできなくなります。

---

## 🎬 お手本（DemoGame）で見るところ

---

プレイヤー追従カメラ。ボス戦での画角の変化。
お手本の `SimpleFollowCamera.cs` が「手で書いた版」です。これと見比べてください。

> `Assets/_DemoGame/` は**完成形の参考展示**です。授業中に編集はしません。
> 「作るとこうなる」を先に見てから、演習シーンで自分の手を動かします。

---

## 💡 今週持ち帰る考え方

---

> **すでにある仕組みに乗るか、自分で作るか**を見極める

<なぜそう言えるのかを、この回の題材に即して 2〜4 行で書く。>
<技術の話に戻さず、「他の場面でもこう使える」まで書ききること。>

---

## ✅ チェックリスト

---

授業の終わりに、自分で確認してください。

- [ ] <確認項目>
- [ ] <確認項目>
- [ ] <確認項目>

---

## 🔗 参考リンク

---

- [Cinemachine ドキュメント](https://docs.unity3d.com/Packages/com.unity.cinemachine@latest)
