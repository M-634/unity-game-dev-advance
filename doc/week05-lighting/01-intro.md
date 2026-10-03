# グラフィックス基礎① Lighting 導入編

## 概要

---

UnityEngine でゲーム制作をする上で、基本的な Light の設定と種類について知る。

同じモデル、同じ配置でも、光の置き方ひとつで「昼の広場」にも「不気味な地下室」にもなります。
ライティングは飾りではなく、**世界観そのものを決める工程**です。

> **今週の問い**：光を置くだけで、なぜ世界の印象が変わるのか？

---

## 今週扱う内容

---

- Unity の **Light コンポーネント**の紹介
- ゲームにおける**ライティングの重要性**（次週の Shader に繋げる）

---

## 演習シーン

---

**使用シーン**：`Assets/Application/Week05-Lighting/Week05-Lighting_Start.unity`
**完成見本**：`Assets/Application/Week05-Lighting/Week05-Lighting_Goal.unity`

---

## Light の種類

---

- **Directional Light**
- **Point Light**
- **Spot Light**
- **Area Light**
- 2D Light（今回は取り扱わない）

### Lighting 関連の用語

---

| 用語 | 意味 |
| --- | --- |
| **Lit / Unlit** | 光の影響を受ける Material かどうか |
| **Shadow** | 影 |
| **Bake** | ライティングの計算を事前に確定させること |
| **Reflection Probe** | 反射の映り込みを制御する |
| **Light Probe** | 動くオブジェクトに、ベイク済みの光を当てる仕組み |

---

## Directional Light

---

Scene 上の**すべての GameObject に対して光を当てる**。太陽光のようなもの。

Bake モードを

- **Realtime**（ゲームシーン中、常にライティングの計算をする）
- **Baked**（Editor 上でライティングの計算を一度確定させる。落ちる影が固定になる）

の2つから選べる**唯一のライト**です。

他のライトは基本的に、後述するベイク（Bake）推奨。

---

## Sample Spheres（ライティングを受ける側の設定）

---

サンプルシーンの4つの sphere を比較しながら設定を確認します。

![4つのSphereの比較](images/sample-spheres-comparison.png)

左から、通常の **Lit**、**影 off**、**影 only**、**Unlit** のもの。

**Lit / Unlit** とは、光の影響を受ける Material を使用しているか
（厳密には、光を考慮したシェーダー計算をしている shader を使用した Material か）ということで、
立体感が出るかどうかが変わります。

![Lit マテリアルの設定項目](images/material-lit.png)

![Unlit マテリアルの設定項目](images/material-unlit.png)

上記2つの Material は Unity デフォルトのものですが、**shader の設定部分が Lit / Unlit でそれぞれ違う**と、
Material 上で設定できる項目も変わっていることが分かるでしょう。

Unlit の shader を適用した Material はライティングの計算をしないので、
**設定項目が少なく軽い反面、立体感は出ません。**

---

## Cast Shadow

---

**MeshRenderer** コンポーネントには、ライティングが対象のオブジェクトに当たった際に、
影を地面に落とすかどうかなどを設定できます。

![MeshRenderer の Cast Shadows 設定](images/mesh-renderer-cast-shadows.png)

**Shadows Only** を選択すると Mesh が消えて**影だけを表示**できるなど、
ユニークなこともできるので覚えておくといいでしょう。

---

## Point Light

---

現実世界で例えると、**街灯や蛍光灯**。

## Spot Light

---

特定の対象に光を当てたいときに使用するライト。
**懐中電灯、ライブのライト、女優ライト**。

## Area Light

---

よりリアルに光を表現したい（街灯やインテリアの細かい光）ときなど、
Point Light より細かい表現をする際に使用します。

> ⚠️ **ただし後述する Bake が前提**です。また URP だと「Rectangle」しか使用できません。

---

## ライティングを Bake する

---

**Window > Rendering > Lighting** から Lighting ウィンドウを表示します。

![Lighting ウィンドウ](images/lighting-window.png)

Scene 上の「Point Light」オブジェクトだけを Active にした状態にしたら（下の図を参考に）

![Point Light だけ Active にした状態](images/point-light-only-active.png)

Lighting ウィンドウの「Scene」タブの一番下、**「Generate Lighting」** ボタンを押下します。

![Generate Lighting ボタン](images/generate-lighting-button.png)

その後、先ほど Active にした Scene 上の「Point Light」も Off にして、
**ライトが光ったままであること**を確認します。

![ライトを消してもベイクされた光が残っている](images/baked-light-result.png)

これが Light をベイク（Bake）する基本的な工程です。

### なぜベイクするのか

---

ベイクをすることによって、**毎フレーム光の計算をせずに済む**ので、ゲームを軽くする役割に用いられます。

ただし、**よく動くオブジェクトに対してベイクしてしまうと、光源や影が残ってしまい不自然な見た目**になります。

> キャラクター等は Directional Light でリアルタイムに影を当てつつ、
> 固定の Object や Map などの地形オブジェクトにベイクを使用するとよいでしょう。

### ベイクしたデータの扱い

---

ベイクしたデータは **1つの Scene ファイルに紐づきます。**
ベイクすると `LightingData.asset` が、ベイクした Scene ファイルと同じ名前のフォルダに、
`.unity` ファイルと同じ階層に作成されます。

![ベイクで生成された Lighting フォルダ](images/lighting-data-folder.png)

⚠️ **このファイルが存在する限り、ベイクしたライトは動かしても反映されません。**

LightingData を Lighting ウィンドウから切り替えるか、一度ベイク情報をクリアしましょう。

> ベイクをクリアすると、ベイク時に作成された Lighting フォルダごと削除されます。

![ベイクデータのクリア](images/clear-baked-data.png)

---

## Reflection Probe

---

ひとことで言うと、**反射を制御するコンポーネント**です。

Hierarchy 上で右クリック > **Light > Reflection Probe** を選択。

![Reflection Probe の作成](images/create-reflection-probe.png)

ReflectionProbe コンポーネントが付いた GameObject が生成されます。

![ReflectionProbe コンポーネント](images/reflection-probe-component.png)

薄い黄色で表示されている範囲が、Reflection Probe で設定できる範囲です。

反射させたいものは基本的に **Static** にチェックを入れる必要があります
（正確には **Reflection Probe Static** にチェックが入っていること）。

![Reflection Probe Static の設定](images/reflection-probe-static.png)

反射させるオブジェクト（上記画面で言うと地面 = ReflectionGround）の Material の設定は
**Metallic** にして、**Metallic Map** と **Smoothness** の値を **1** に設定しましょう。

![Metallic と Smoothness の設定](images/metallic-smoothness-settings.png)

これも基本的に **Bake 前提**です。設定が終わったら最後にベイクします。

---

## Light Probe（ちょっと応用）

---

今回の授業では扱いませんが、こんなのもあるよ、という紹介です（3D系のステージ制作向け）。

Unity 6 の新しいグローバルイルミネーション照明機能もあわせて紹介します。

---

## 🎬 お手本（DemoGame）で見るところ

---

灯が点いていないときの冷たい色の塔と、灯を点けたあとの暖かい色の塔。
**明るさの変化が、そのまま進行度の表示になっている**のを確認してください。

> `Assets/_DemoGame/` は**完成形の参考展示**です。授業中に編集はしません。

---

## 💡 今週持ち帰る考え方

---

> **実行時のコストを「前払い」する。**

ベイクは、毎フレーム払うはずだった計算コストを、制作時に一度だけ払う形に変えることです。
そのかわり「あとから動かせない」という制約を受け入れています。

**何かを得るには何かを差し出す。** どちらが正しいかではなく、
**いまの場面でどちらの制約なら受け入れられるか**を選べることが大事です。

---

## ✅ チェックリスト

---

- [ ] Directional / Point / Spot / Area の違いを説明できる
- [ ] Lit と Unlit の Material で見た目がどう変わるか説明できる
- [ ] ライトをベイクして、ライトを Off にしても光が残る状態を作れた
- [ ] ベイクしたデータがどこに作られるか分かる
- [ ] Reflection Probe で反射を出せた

---

## まとめ

---

- **Directional Light は 1 Scene に 1 つ。**
- その他の Light で**静的なオブジェクト**については、基本的に**ベイクしよう**。
- **Reflection Probe** は物体の反射の映り込みを表現するもの。

---

## 🔗 参考リンク

---

- [Lighting（Unity Manual）](https://docs.unity3d.com/Manual/LightingOverview.html)
- [Light コンポーネント](https://docs.unity3d.com/Manual/class-Light.html)
- [Reflection Probes](https://docs.unity3d.com/Manual/ReflectionProbes.html)
