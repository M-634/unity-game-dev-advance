# 『LUMEN』タスク表

> **このドキュメントは非配布です。**
> DemoGame 制作の**残作業と進み具合**をまとめた表です。セッションを始めるときに最初に読み、作業を終えたらここを更新します。
> 細かい仕様・構造は各設計書（`01`〜`14`）が正です。見た目の調整場所は [13-graphics-tuning.md](13-graphics-tuning.md)。ここには「何が残っているか」と「どこを見ればよいか」だけを書きます。

最終更新：2026-10-02（深夜）

---

## 0. 使い方

---

- **セッションの始め**：§1「いまの状況」と §2「次にやること」を読んでから作業に入る
- **作業の終わり**：終わったタスクを `[x]` にして §4「完了したもの」へ移す。新しく見つかった作業は §3 に足す。§1 と最終更新日を書き換える
- 1タスク＝1セッションで終わる大きさを目安にする。大きいものは分けて書く
- 設計を決めないと進まないものは「要決定」と書き、決まったら設計書に書いてからタスクを進める
- 状態の印：`[ ]` 未着手 ／ `[~]` 作業中 ／ `[x]` 完了 ／ `[?]` 要決定（ユーザーに確認）
- **進め方**：まず §2 の順に機能を一通り作り、ゲーム全体がそろってから各機能のブラッシュアップ（§3.9 調整など）に入る

---

## 1. いまの状況

---

- 骨組み（入力・プレイヤー・UI・シーン遷移）、ギミック3種、カメラ（TPS / FPS）、Timeline 演出3本、トゥーンシェーダー・輪郭線は**実装済み**
- 敵は **NavMesh ＋ State パターン**で作り直した（[09-enemy.md](09-enemy.md)）。雑魚敵の頭上 HP バー・ボスの HUD HP バー・ダメージ数・開発用の表示（状態の距離・当たり判定）も入り、動作を確認した。**攻撃の振り下ろしと当たり判定のタイミングがまだ合っていない**（ブラッシュアップで直す。§3.4）
- モデル3体（ユニティちゃん・Monster Minion・Rock Golem）は導入・差し替え済み。`*Test` Prefab は削除した
- ステージは**仮配置**。DemoLevel の大まかな配置（エリア3つ・初期位置・ボス部屋の瓦礫9か所）に合わせて DemoInGame の部品を置き直し、階段と神殿の部屋は DemoLevel の ProBuilder を写した（[10-level-design.md](10-level-design.md) §11.3）。**まだ Unity で開いて確かめていない**
- 音・ポーズ・リスポーンは**手つかず**
- **タイトル（DemoTitle）と設定画面を作った**（[14-outgame.md](14-outgame.md)）。設定は `settings.json` に保存。サウンドの設定は値だけ（音は未接続）。**Unity での動作確認はまだ**
- **2026-10-02：ゲームデザインを大きく変えた**（[10-level-design.md](10-level-design.md)）。舞台は霧のフィールド、攻略の順番は固定、攻撃は**杖（殴る）＋火焔玉（チャージ3段）**、篝火で杖に火を灯し、その火で灯火を点ける。視界の明るさ **Lumen** を持つ。クリアはボス撃破後の光の柱に触れたとき。**一通り実装した（[10-level-design.md](10-level-design.md) §11）。通しの動作確認はまだ**
- ライティング（Mixed / Baked Indirect・Layer `Level`・ベイクのメニュー・灯火の数で主光源と霧・Lumen の Volume・チャージの画面の縁の炎）と、オブジェクトごとの輪郭線の強さを実装した（[11-lighting.md](11-lighting.md) §8・[05-graphics.md](05-graphics.md) §5.5）。**ベイクの実行と見た目の確認はまだ**。エフェクト（Particle System）も一通り作った（[12-vfx.md](12-vfx.md) §7。見た目の確認はまだ）
- **このセッション（2026-10-02）の終わり**：エフェクト・Volume・ライティングは一通り入った。細かい調整は後回し。調整の場所は [13-graphics-tuning.md](13-graphics-tuning.md)。**Unity は Auto Refresh がオフなので、ファイルを直したら Cmd+R**
- ステージの骨組みは講師が **`DemoLevel.unity`（ProBuilder）** で作っている。こちらは DemoInGame で部品を Prefab にしておき、最後にステージを DemoInGame へ反映する（[10-level-design.md](10-level-design.md) §3.4）

---

## 2. 次にやること（優先順）

---

機能を一通りそろえる順番です。終わったものは消し、§4 に移します。

1. **DemoInGame を通しで遊んで確認する**（§3.11 の確認項目。ベイクの実行も）
2. エフェクトの見た目の確認（§3.7）
3. 描画の確認（Frame Debugger。§3.2）
4. ステージ（§3.6）：DemoLevel の骨組みを DemoInGame に反映 → 部品を置き直す → ベイク・Light Probe
5. 進行管理（失敗時の再開・エリアの記録。§3.3）とサウンド（§3.8）
6. UI（タイトル・設定の確認 → ポーズ。§3.5）
7. 通しの確認（§3.2）→ ブラッシュアップ（§3.4 の残り・§3.9）

---

## 3. 残作業

---

### 3.1 キャラクター（[04-character-prefab.md](04-character-prefab.md) §7）

---

- [ ] 見た目の調整（大きさ・向き・コライダー）を Scene で確認する
- [?] 攻撃の仕様（モーション・当たるタイミング・硬直）を決める。今は入力した瞬間に判定が出る（Animation Event 未使用）
- [ ] プレイヤーの被弾点滅（`_HitFlash`）（[05-graphics.md](05-graphics.md) §11 #12）。`CharacterVisual.Flash()` を `PlayerHealth` の被弾から呼べばよい
- [x] オブジェクトごとの輪郭線の強さ `_OutlineStrength`（[05-graphics.md](05-graphics.md) §5.5）。目まわり 0・顔と頬 0.5・ボス 0.7。**見た目の確認はまだ**

### 3.2 通しの確認（[02-spec.md](02-spec.md) §11.1 の手順 2）

---

- [ ] DemoTitle → DemoInGame → クリア → リザルト → タイトル が一周できるか
- [ ] 導入演出・封印解除・ボス登場の演出が通しの中で正しく流れるか
- [ ] Frame Debugger でバッチを確認する（[05-graphics.md](05-graphics.md) §10・§11 #10）

### 3.3 進行管理（[02-spec.md](02-spec.md) §7）

---

- [ ] `GameProgress.CurrentArea`（エリアの記録）を実装する
- [ ] 失敗時に `CurrentArea` の入口から再開する（HP 全回復・灯の状態は維持）。今はリスポーンが無い

### 3.4 敵・ボス（[09-enemy.md](09-enemy.md)）

---

> 実装は一通り終わり。以下はブラッシュアップでやる

- [ ] **攻撃の振り下ろしと当たり判定の出るタイミングが合っていない**（ボス・雑魚敵とも。2026-10-02 の確認）。時間帯（`HitStartTime` / `HitEndTime`）を開発用の表示を見ながら合わせる。合わないなら Animation Event か、Animator の再生位置（normalizedTime）で区切る方式に変える
- [ ] 残りの確認（[09-enemy.md](09-enemy.md) §10 #9）：周回 → 発見 → 追跡 → 攻撃、怯み、死亡と消滅、ボスの怒り（HP 150）・無敵（3 秒に 5 発）、F1 のデバッグモード、HP バー・ダメージ数の見え方
- [ ] 敵とプレイヤーの攻撃の当たり判定（範囲・位置）を、開発用の表示を見ながら合わせる（[09-enemy.md](09-enemy.md) §8.2）
- [ ] 雑魚敵の周回ルート（`PatrolPoints`）をステージに合わせて置く（今は出現位置のまわりをランダム）
- [?] ボスの突進攻撃を入れるか（元の仕様にあった。今は近接攻撃だけ）
- [ ] 無敵（構え）・怒りの見た目を、エフェクトで強める（今は体の色だけ。[12-vfx.md](12-vfx.md) #15・#16）
- [ ] ダメージ数を Object Pool にするか検討する（今は毎回 Instantiate / Destroy）

### 3.5 UI（[02-spec.md](02-spec.md) §8・§9）

---

- [x] タイトル画面（START / SETTINGS / QUIT）と設定画面（Graphics・Sound）、`settings.json` への保存（[14-outgame.md](14-outgame.md)）
- [ ] **タイトル・設定の動作を確認する**（Unity で Cmd+R → DemoTitle を再生。キーボード・マウス・ゲームパッド、`settings.json` の中身、ビルドで解像度・画面モードが変わるか。[14-outgame.md](14-outgame.md) §7 #4）
- [ ] ポーズ画面（Resume / Settings / Title）。設定は `SettingsPresenter` をそのまま使う
- [ ] DemoInGame を直接再生したときも保存した設定を反映する（今はタイトルを通ったときだけ。[14-outgame.md](14-outgame.md) §7 #5）
- [?] マウス感度を設定に入れるか（今の設定画面には無い）
- [?] リザルトの仕様を確定する（今は仮。[08-timeline.md](08-timeline.md) §9 #3）
- [?] UI フォントが英字のみ。日本語を出すか決める

### 3.6 ステージ（[02-spec.md](02-spec.md) §11.1 の手順 3〜4）

---

- [~] ステージの骨組みを `DemoLevel.unity` で ProBuilder で作る（**講師**）。動かない物は `LevelRoot` の下・Layer `Level` に
- [x] DemoLevel の大まかな配置に合わせて、DemoInGame の部品を置き直す（2026-10-02。[10-level-design.md](10-level-design.md) §11.3）。階段・神殿の部屋は ProBuilder を写した（`LevelRoot/StageBlockout`）
- [ ] **置き直した DemoInGame を開いて確かめる**：階段を上れるか・ボス扉が開くか（10×10 にした）・瓦礫 9 か所の高さ・灯火エリアの向き・導入のカメラ。ベイクし直す
- [ ] 骨組みを DemoInGame の `LevelRoot` に反映し、今の仮配置（ハブ型の Cube）を置き換える。部品の Prefab を置き直す（[10-level-design.md](10-level-design.md) §3.4）
- [ ] エリア（目覚めの地 → 灯火 1 → 2 → 3 → 神殿）を一本道のフィールドで組む（[10-level-design.md](10-level-design.md) §3）
- [?] ステージのモデル（遺跡・岩・木・篝火・空）を選ぶ。無料の候補は [03-asset-candidates.md](03-asset-candidates.md) §7
- [ ] ステージ用のマテリアル・シェーダー（ステージができてから。[05-graphics.md](05-graphics.md) §11 #16）
- [ ] 箱押しの詰み防止 `PressBox.ResetPosition()` の呼び出し方を決めて組む（[06-gimmick.md](06-gimmick.md) §9 #1）
- [?] 灯の間 B のスイッチを「手で押すボタン（IK）」にするか（[06-gimmick.md](06-gimmick.md) §9 #2）
- [ ] 灯の間 B の制限時間を実プレイで調整する
- [ ] 高所の印を FPS でしか読めないようにする仕掛け（[07-camera.md](07-camera.md) §7）
- [ ] 紋様（`Emblem_A / B / C`）の見た目を仮の円盤から差し替える（[08-timeline.md](08-timeline.md) §7）

### 3.7 エフェクト（[05-graphics.md](05-graphics.md)）

---

- [x] Particle System のエフェクト（◎ を中心に17個。テクスチャも自作。[12-vfx.md](12-vfx.md) §7）
- [ ] **エフェクトの見た目を Unity で確認して直す**（大きさ・数・色。炎が上へ出るか、霧が濃すぎないか）
- [ ] 残りのエフェクト（[12-vfx.md](12-vfx.md) §7.3。ボスの叩きつけの予兆など）
- [ ] 親マテリアルの Lock を設定する（[05-graphics.md](05-graphics.md) §11 #2）

### 3.8 サウンド

---

- [?] BGM・SE の素材を選ぶ（候補は [03-asset-candidates.md](03-asset-candidates.md) §7。未選定）
- [ ] BGM の再生と、灯の数による変化（[02-spec.md](02-spec.md) §7.1 の図）
- [ ] SE（攻撃・被弾・点灯・扉など）
- [ ] Timeline 演出に Audio Track を足す（[08-timeline.md](08-timeline.md) §8）
- [ ] 設定の音量（`SettingsModel` の Master / BGM / SE。値の保存だけ済み）を AudioMixer につなぐ

### 3.9 調整

---

- [ ] カメラ感度（TPS / FPS 別にするか）・FPS の上下の制限角・Impulse の強さ（[07-camera.md](07-camera.md) §7）
- [ ] 演出の尺の調整（[08-timeline.md](08-timeline.md) §7）

### 3.10 ライティング

---

- [x] ベイクの仕組み（`DemoGame > Lighting`）・仮配置の Layer `Level`・主光源 Mixed・霧 ON（[11-lighting.md](11-lighting.md) §8）
- [x] 世界の明るさ `WorldLightingController`・視界の Volume（`DemoVisionVolumes`）・チャージの画面の縁の炎（`FlameVignette`）
- [ ] **DemoInGame で `Bake Level` を実行して焼く**。暗さ・霧・Volume・縁の炎の見え方を見て数値を決める（霧の濃さで光の柱が遠くから見えるかも）
- [ ] ライトの配置・Light Probe（ステージ完成後。Reflection Probe は基本置かない。同 §3.3・§4）

### 3.11 新しい遊び（[10-level-design.md](10-level-design.md)）

---

> 部品（篝火・灯火・道をふさぐ物・蔦と瓦礫・光の柱）は**1つずつ Prefab にして、どこに置いても動く**ようにする

- [x] 杖・導入で拾う杖・攻撃（殴る／火焔玉・チャージ）・火焔玉（Object Pool）・篝火（ボス戦用も）・Lumen（[10-level-design.md](10-level-design.md) §11）
- [x] 灯火の点け方（杖の火で E）・光の柱・灯火 1（箱をスイッチに載せる／`TimedDoor` の Hold）・灯火 2（火をつけたら敵が出る）・灯火 3（魔法の壁の中の、蔦に覆われた灯火）・`AreaGate`
- [x] ボス戦（出てくる篝火・叩きつけで弱くなる・蔦と瓦礫）・撃破演出 `BossDefeat`・`ClearBeacon`（触れたらクリア）
- [x] 火を移すアクション：篝火・灯火・蔦を E にし、右腕を IK で対象へ伸ばす（`PlayerStaffReach`・`Rig_Reach`。[10-level-design.md](10-level-design.md) §4.5）。蔦は E でも燃やせる。**篝火で E → 杖に火が移るのを確認した（2026-10-02）**
- [ ] 腕の伸ばし方を見て直す（Hint の位置・伸ばす時間・杖の先が対象に届いて見えるか）
- [ ] **通しの確認**：杖を拾う → 篝火で灯る → B（箱をスイッチへ）→ C の壁が消える → 火をつけて敵 → 全滅で点く → A の壁が消える → 蔦を燃やして点ける → ボス扉 → 篝火がせり上がる・叩きつけで弱くなる・瓦礫を落とす → 撃破 → 光 → 触れてクリア。チャージ（灯火を点けるたびに段が増える）と、画面の縁の炎・暗さ
- [ ] 導入演出に杖を拾うカットを足す（今は演出が終わったときに拾う）
- [ ] HUD に杖の火・光の段を出すか決める
- [ ] 杖のモデルを選ぶ（候補は [03-asset-candidates.md](03-asset-candidates.md) §7）

### 3.12 仕上げ

---

- [ ] 設計書を「実際に作った構造」に更新し、仕様書（[02-spec.md](02-spec.md)）とのずれをなくす
- [ ] 外部依存が [setup.md](../setup.md) にすべて載っているか確認する
- [ ] `.unitypackage` に書き出し、配布先の手順（[setup.md](../setup.md)）どおりにコンパイル・起動できるか確認する

---

## 4. 完了したもの

---

| 日付 | 内容 | 設計書 |
| --- | --- | --- |
| 〜2026-10-01 | 骨組み（入力・プレイヤー・UI・シーン遷移・仮ギミック） | [02-spec.md](02-spec.md) |
| 〜2026-10-01 | キャラクター Prefab の2層構造・モデル3体の差し替え・Animator Controller・剣・敵の `_HitFlash` / `_Dissolve` | [04-character-prefab.md](04-character-prefab.md) |
| 2026-10-01 | `DemoGame/Toon` シェーダー・Material Variant・輪郭線・Global Volume | [05-graphics.md](05-graphics.md) |
| 〜2026-10-01 | ギミック（灯・箱押し・時限扉・撃破）と仮配置、HUD のインタラクト表示、両手 IK | [06-gimmick.md](06-gimmick.md) |
| 〜2026-10-01 | TPS / FPS 切替・FPS の体の非表示・カメラシェイク | [07-camera.md](07-camera.md) |
| 〜2026-10-01 | Timeline 演出3本（導入・封印解除・ボス登場）とスキップ | [08-timeline.md](08-timeline.md) |
| 2026-10-02 | 敵の State パターン（Init / Search / Pursue / Attack / Damage / Die / Debug ＋ボスの Angry / Invincible）、パラメータ、HP バー（雑魚敵は頭上・ボスは HUD）、ダメージ数、F1 のデバッグモード | [09-enemy.md](09-enemy.md) |
| 2026-10-02 | シーンの参照を新しい Prefab に切り替え、`*Test.prefab` を削除 | [04-character-prefab.md](04-character-prefab.md) |
| 2026-10-02 | 新しいゲームデザイン（杖・篝火・火焔玉・Lumen・順番固定・クリア）の設計 | [10-level-design.md](10-level-design.md) |
| 2026-10-02 | ライティング（Mixed / Baked Indirect・Layer `Level`・ベイクの流れ・Lumen の見せ方）とエフェクトの洗い出しの設計、輪郭線の強さの設計 | [11-lighting.md](11-lighting.md)・[12-vfx.md](12-vfx.md)・[05-graphics.md](05-graphics.md) §5.5 |
| 2026-10-02 | 見た目の設定と調整ガイド・クラス図 | [13-graphics-tuning.md](13-graphics-tuning.md) |
| 2026-10-02 | エフェクト（Particle System 17個・自作テクスチャ4枚・`FxSpawner`・`BossEffects`・`FogEffectController`） | [12-vfx.md](12-vfx.md) §7 |
| 2026-10-02 | タイトル画面・設定画面（MVP ＋ R3）と設定の JSON 保存、`DemoTitle.unity` の組み立て | [14-outgame.md](14-outgame.md) |
| 2026-10-02 | 新しい遊びの実装（杖・篝火・火焔玉・Lumen・灯火 1〜3 の仕掛け・AreaGate・ボス戦の篝火と瓦礫・撃破演出とクリア）、ライティングと視界の実装、輪郭線の強さ。DemoInGame の仮配置に反映 | [10-level-design.md](10-level-design.md) §11・[11-lighting.md](11-lighting.md) §8 |
| 2026-10-02 | DemoLevel の配置に合わせて DemoInGame を置き直し（階段・神殿は ProBuilder を写した）。無料アセットの候補を調べた | [10-level-design.md](10-level-design.md) §11.3・[03-asset-candidates.md](03-asset-candidates.md) §7 |
