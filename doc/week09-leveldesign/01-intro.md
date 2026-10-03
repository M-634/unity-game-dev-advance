# 3D空間のレベルデザイン 導入編

## 概要

---

ここまでは「動くもの」を作ってきました。この回は**空間そのもの**を作ります。

レベルデザインは飾りではありません。プレイヤーがどこへ進み、何に気づくかは、
置かれた壁と光と敵の配置で決まります。**説明文を読ませずに意図を伝える技術**です。

> **今週の問い**：プレイヤーはなぜ、教えてもいない方向へ進んでいけるのか？

---

## 今週扱う内容

---

- **ProBuilder や Blender を使った 3D モデリングの基礎**
  （ゲーム制作全般で通用する概念や語彙の紹介）
- **3D モデルを Unity にインポートするときに気をつけること**（Material や Shader 設定）
- **カメラ制御。LOD やカリングなどの最適化**
- **ライティングの調整や最適化**
- **敵や Object の配置の工夫・導線の意識**

---

## 演習シーン

---

**使用シーン**：`Assets/Application/Week09-LevelDesign/Week09-LevelDesign_Start.unity`
**完成見本**：`Assets/Application/Week09-LevelDesign/Week09-LevelDesign_Goal.unity`

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
namespace Week09
{
    public class Sample : MonoBehaviour
    {
    }
}
```

> **namespace は必ず `Week09` を付けてください。**
> 付けないと、同じクラス名を作った人とぶつかってプロジェクト全体がコンパイルできなくなります。

---

## 🎬 お手本（DemoGame）で見るところ

---

目覚めの間から中央広間への導線。壊せる木箱の置き方。高所の印の見せ方。

> `Assets/_DemoGame/` は**完成形の参考展示**です。授業中に編集はしません。
> 「作るとこうなる」を先に見てから、演習シーンで自分の手を動かします。

---

## 💡 今週持ち帰る考え方

---

> **説明せずに伝える。** 構造そのものが意図を語る

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

- [ProBuilder ドキュメント](https://docs.unity3d.com/Packages/com.unity.probuilder@latest)
- [Level of Detail (LOD)](https://docs.unity3d.com/Manual/LevelOfDetail.html)
- [Occlusion Culling](https://docs.unity3d.com/Manual/OcclusionCulling.html)
