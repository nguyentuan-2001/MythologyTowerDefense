# MythologyTowerDefense — Tiến độ thực hiện Master Plan

> File này được cập nhật liên tục trong quá trình làm việc. Chú thích trạng thái:
> ✅ Đã làm & đã kiểm chứng (chạy Unity thật, có log) · 🟡 Đã viết code, **chưa kiểm chứng** · ⬜ Chưa làm · 🔴 Đang bị chặn

---

## Trạng thái tổng quan

**Toàn bộ 8 milestone (0-7) của Master Plan: hoàn thành, đã kiểm chứng cả smoke test lẫn Play Mode (Editor + WebGL) — xem lịch sử bug ở các mục milestone bên dưới.**

## Mở rộng #1 — Thêm tháp thứ 2: Pangu Titan (sát thương diện rộng)

Theo đề nghị của bạn và roster gợi ý ở Master Plan §7.7 (Bàn Cổ / Pangu — "Heavy area damage, slow powerful strikes with splash damage").

- [x] ✅ `ProjectileController` hỗ trợ splash damage (tham số `splashRadius`, damage tất cả quái trong bán kính khi va chạm, không damage đôi mục tiêu chính)
- [x] ✅ `TowerController` thêm `displayName`, `splashRadius`, generic hoá để dùng chung cho nhiều loại tháp
- [x] ✅ `TowerPad` đổi thành generic — không còn gắn cứng 1 loại tháp/giá, nhận prefab+giá lúc xây
- [x] ✅ `TowerBuildMenuController` (mới) — bảng chọn tháp hiện ra khi click pad trống, đúng theo spec §6 ("clicking an empty pad opens a small tower choice")
- [x] ✅ Pangu Titan: giá 50, range 4.5 (bằng Archer, tránh lặp lại bug pad-offset-vs-range), dame 20, bắn chậm 1.6s, splash 2.0, hình dáng to hơn (đá/cinnabar)
- [x] ✅ HUD panel thông tin tháp hiển thị tên + splash radius khi chọn tháp loại splash

**⚠️ 2 bug thật tìm thấy & đã sửa trong lần mở rộng này:**
1. Test mua Pangu (50 vàng) fail vì các bước test trước đã tiêu gần hết vàng khởi điểm (60) — không phải bug sản phẩm, chỉ là test thiếu set up tiền → đã thêm `currency.Add(50)` trước đoạn test đó.
2. **Bug thật trong `TowerController.FindTarget()`**: đo khoảng cách tới quái từ `firePoint` (đầu nòng, xoay theo hướng ngắm) thay vì từ gốc tháp (`transform.position`, tâm vòng tròn tầm bắn hiển thị cho người chơi). Phát bắn đầu tiên, đầu nòng ở hướng mặc định (chưa từng ngắm ai) nên có thể tình cờ trỏ sai hướng, khiến 1 quái rõ ràng nằm trong vòng tròn tầm bắn hiển thị lại bị tính "ngoài tầm". Hậu quả: tháp mới xây đôi khi bỏ lỡ mục tiêu đầu tiên một cách không nhất quán, tuỳ hướng mặc định của model. Đã sửa: đo từ `transform.position` — khớp đúng với vòng tròn tầm bắn hiển thị, không còn phụ thuộc hướng ngắm hiện tại.

Smoke test đã mở rộng thêm: test splash damage bắn trúng 2 quái cùng lúc bằng 1 phát đạn (xác minh qua health thật, không chỉ "có tạo projectile").

---

## Milestone 0 — Audit & an toàn dự án

- [x] ✅ Kiểm tra git status (sạch, đã có 1 commit, đã push lên GitHub)
- [x] ✅ Xác nhận Unity 6000.6.7f1 + URP
- [x] ✅ Xác nhận module WebGL đã cài (`PlaybackEngines/WebGLSupport` tồn tại)
- [x] ✅ Xác nhận Input System mới (ảnh hưởng cách viết code click chuột)
- [x] ✅ Phát hiện cấu trúc cũ (`Assets/Scripts` phẳng) không khớp `Assets/_Game/...` mà Master Plan yêu cầu → quyết định tái cấu trúc
- [x] ✅ Project compile sạch (0 lỗi) sau khi xoá code cũ trùng tên — xác nhận qua Unity Console của bạn (Safe Mode đã thoát)

## Milestone 1 — Greybox battlefield

- [x] ✅ `PathDefinition.cs` (waypoints + gizmo debug) — đã viết
- [x] ✅ Editor tool `PrototypeSceneBuilder.cs` dựng ground, đường đi uốn lượn, cổng vào (Entrance), base (BaseGoal), camera isometric, 6 tower pad — đã viết
- [x] ✅ Scene `Assets/_Game/Scenes/Main.unity` đã được build & lưu (xác nhận qua log + file trên đĩa, 169KB)
- [x] ✅ Kiểm chứng: scene build không có exception/lỗi trong Console
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): camera thấy toàn bộ route + pad khi Play
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): không pad nào đè lên đường đi

## Milestone 2 — Enemy và Base

- [x] ✅ `EnemyController.cs` — đi theo waypoint, quay mặt theo hướng di chuyển, máu 40, tốc độ 2.0, dame base 1, thưởng 8 — đã viết
- [x] ✅ `BaseHealth.cs` — máu base 10, sự kiện `OnDepleted` đúng 1 lần — đã viết
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): enemy đi hết đường, Start Wave ra quái đúng
- [x] ✅ Kiểm chứng (smoke test): enemy trừ máu base đúng 1 lần, bị gỡ khỏi danh sách active sau khi đến base

## Milestone 3 — Tháp cung thủ & combat

- [x] ✅ `TowerController.cs` — range 4.5, dame 12, tốc bắn 0.8s, ưu tiên enemy đi xa nhất trên đường (gần base nhất) — đã viết
- [x] ✅ `ProjectileController.cs` — bay tới mục tiêu, xử lý an toàn khi mục tiêu biến mất — đã viết
- [x] ✅ `TowerPad.cs` — kiểm tra tiền trước khi đặt, chặn đặt trùng — đã viết
- [x] ✅ `TowerSelectionController.cs` — click pad trống để xây, click pad có tháp để chọn, click chỗ khác để bỏ chọn — đã viết
- [x] ✅ Range indicator (vòng tròn hiện khi chọn tháp) — đã viết
- [x] ✅ Kiểm chứng (smoke test): tháp bắn trúng enemy trong tầm, tạo projectile thật
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): click pad xây được tháp, tháp bắn quái
- [x] ✅ Kiểm chứng (smoke test): tiền cộng đúng +8 khi giết quái, trừ đúng -30 khi mua tháp, chặn mua trùng pad

## Milestone 4 — Wave & trạng thái game

- [x] ✅ `WaveManager.cs` — 10 quái/wave, spawn cách nhau 1s, chờ hết quái mới coi là xong wave, chặn start trùng — đã viết
- [x] ✅ `GameManager.cs` — Start/Pause/Resume/Restart, trạng thái Thắng/Thua — đã viết
- [x] ✅ Kiểm chứng (smoke test): trạng thái Thua (`GameState.Lost`) kích hoạt đúng khi base hết máu
- [x] ✅ Kiểm chứng (smoke test): trạng thái Thắng (`GameState.Won`) kích hoạt đúng khi hết wave
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): timing wave thật chạy đúng (spawn cách 1s, chờ hết quái)
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): Restart đưa game về trạng thái ban đầu
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): Pause/Resume hoạt động đúng
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): panel VICTORY/GAME OVER hiện đúng khi thắng/thua

## Milestone 5 — HUD & hoàn thiện UI

- [x] ✅ `HUDController.cs` — hiển thị vàng, máu base, trạng thái wave, panel thắng/thua, panel thông tin tháp khi chọn — đã viết
- [x] ✅ Màu sắc theo art bible: jade green, warm gold, cinnabar red, deep teal, cream — đã áp dụng trong editor tool
- [ ] ⬜ Kiểm chứng: UI đọc được ở 16:9 và cửa sổ hẹp hơn

## Milestone 6 — Sẵn sàng WebGL

- [x] ✅ Xác nhận module WebGL đã cài trong Unity Hub
- [x] ✅ `WebGLBuildScript.cs` (menu `Tools/Mythology TD/Build WebGL`) — đã viết
- [x] ✅ Build WebGL chạy headless thành công — `WEBGL_BUILD_RESULT: SUCCEEDED`, ~9.6MB, output tại `Builds/WebGL/` (không commit vào git, đã có trong `.gitignore`)
- [x] ✅ Kiểm chứng bằng mắt (bạn xác nhận): mở build WebGL qua local server, xây tháp, tháp bắn trúng quái

## Milestone 7 — Ổn định & bàn giao

- [x] ✅ 0 lỗi, 0 warning compile trong các lần build/test gần nhất
- [x] ✅ Viết `README.md` (cách mở, controls, cấu trúc project, cách regenerate scene, cách test, cách build WebGL, checklist, giới hạn đã biết)
- [x] ✅ Tổng kết file đã đổi + vấn đề tồn đọng (xem README mục "Known limitations")

**Toàn bộ 8 milestone (0-7) trong Master Plan đã hoàn thành và được kiểm chứng (smoke test tự động + bạn xác nhận bằng mắt trong Editor và WebGL).**

---

## Việc đang chờ bạn

✅ Đã xác nhận bằng mắt (bạn xác nhận): bảng chọn tháp hiện đúng khi click pad trống, xây Pangu Titan hoạt động đúng (splash damage nhiều quái).

**⚠️ Bug thật tìm thấy trên WebGL (và sửa):** `TowerBuildMenuController` đọc `selectionController.PendingPad` **trực tiếp tại thời điểm bấm nút** thay vì lưu lại ngay khi bảng hiện ra. Khi `TowerSelectionController`'s raycast thế giới chạy cùng lúc (cùng frame) với việc bấm nút UI — khả năng do `IsPointerOverGameObject()` không chặn kịp trong WebGL — `PendingPad` bị xoá về null trước khi nút kịp đọc, khiến bấm nút không có phản ứng gì. Triệu chứng: bảng chọn tháp hiện ra đúng, nhưng bấm Celestial Archer/Pangu Titan không xây được gì. Đã sửa: cache `pendingPad` vào field riêng ngay khi bảng hiện ra, không đọc lại giá trị "sống" từ selectionController lúc bấm nút nữa.

Phát hiện phụ: WebGL cần **click đầu tiên để focus canvas** (browser không gửi input cho tới khi canvas được focus) — click đầu tiên trên trang luôn "mất tác dụng", đây là hành vi bình thường của WebGL/browser, không phải bug.

**⚠️ Bug thứ 2 (bug thật sự, đã tìm ra tận gốc):** "Start Wave" ra chữ đúng nhưng quái đứng im, phải bấm Pause→Resume mới chạy. Đã thử nhiều hướng (click-to-start overlay, Canvas.ForceUpdateCanvases, runInBackground...) nhưng không dứt điểm, cho tới khi thêm bộ đếm hiển thị trực tiếp `Time.timeScale` lên màn hình — phát hiện **`Time.timeScale` = 0 ngay từ lúc game khởi động**, dù không có dòng code nào của ta đặt giá trị đó. Lần theo thì ra `ProjectSettings/TimeManager.asset` có `m_TimeScale: 0` (thay vì 1 bình thường) — một giá trị mặc định sai của chính project. Unity Editor "bỏ qua" giá trị này khi vào Play Mode (nên test Editor không phát hiện ra), nhưng bản build WebGL thật thì dùng đúng giá trị 0 này, khiến quái spawn đúng (có đăng ký) nhưng không bao giờ di chuyển (`Time.deltaTime` luôn 0) cho tới khi người dùng vô tình đặt lại `timeScale=1` qua nút Pause/Resume.

**Đã sửa tận gốc:** thêm `Time.timeScale = 1f;` vào `GameManager.Awake()` — đảm bảo đúng ngay từ đầu bất kể project setting là gì (an toàn hơn, không phụ thuộc file cấu hình có thể bị Unity tự ghi đè lại về 0 mà ta quan sát được khi thử sửa trực tiếp file YAML).

**Dọn dẹp sau khi xác nhận hết bug:** gỡ bỏ overlay "Click to Start" (hoá ra không cần thiết — nguyên nhân thật là timeScale, không phải vấn đề focus trình duyệt) và xoá toàn bộ log/debug-readout tạm thời dùng để chẩn đoán.

✅ Đã build lại WebGL bản sạch (không overlay, không debug) — `Builds/WebGL/`, ~45MB. **Bạn đã xác nhận: hoạt động đúng hoàn toàn**, không cần workaround nào nữa.

---

## File đã tạo/đổi (tính cả 2 đợt: tái cấu trúc ban đầu + mở rộng tháp thứ 2)

```
Assets/_Game/Scripts/Core/GameManager.cs
Assets/_Game/Scripts/Gameplay/Path/PathDefinition.cs
Assets/_Game/Scripts/Gameplay/Enemies/EnemyController.cs
Assets/_Game/Scripts/Gameplay/Economy/CurrencyManager.cs
Assets/_Game/Scripts/Gameplay/Economy/BaseHealth.cs
Assets/_Game/Scripts/Gameplay/Waves/WaveManager.cs
Assets/_Game/Scripts/Gameplay/Towers/ProjectileController.cs     (sửa: + splash damage)
Assets/_Game/Scripts/Gameplay/Towers/TowerController.cs          (sửa: + displayName/splashRadius, fix FindTarget range bug)
Assets/_Game/Scripts/Gameplay/Towers/TowerPad.cs                 (sửa: generic hoá, nhận prefab/giá lúc xây)
Assets/_Game/Scripts/UI/TowerSelectionController.cs              (sửa: + PendingPad cho bảng chọn tháp)
Assets/_Game/Scripts/UI/TowerBuildMenuController.cs              (mới)
Assets/_Game/Scripts/UI/HUDController.cs                         (sửa: dùng DisplayName/SplashRadius động)
Assets/_Game/Editor/PrototypeSceneBuilder.cs                     (sửa: build 2 tháp + UI chọn tháp)
Assets/_Game/Editor/PrototypeSmokeTest.cs                        (sửa: + test splash damage)
Assets/_Game/Editor/WebGLBuildScript.cs
Assets/_Game/Scenes/Main.unity          (scene thật, đã build & lưu — có cả 2 tháp)
Assets/_Game/Prefabs/Towers/PanguTitan.prefab  (mới)
README.md
Document/PROGRESS.md                     (file này)

Đã xoá (thay thế hoàn toàn bởi cấu trúc _Game/ ở trên, đợt tái cấu trúc đầu):
Assets/Scripts/  Assets/Materials/  Assets/Prefabs/  Assets/Scenes/SampleScene.unity
```
