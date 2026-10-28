# Balloon Go

> Một chú bóng bay lạc trong rừng sương mù. Bạn có tìm được đường về với gia đình?

**Balloon Go** là một game endless runner 2D lấy cảm hứng từ Flappy Bird, được làm hoàn toàn trong Unity. Người chơi điều khiển một chú bóng bay bay qua khu rừng sương mù, tránh cành cây và những chú chim, nhặt xu và tìm đường về nhà.

## Điểm nổi bật

- **Lối chơi dễ học, khó thành thạo:** nhấn Space hoặc chạm để bay lên; chạm ranh giới trên/dưới hoặc vật cản là thua.
- **Điểm theo thời gian sinh tồn:** 10 điểm mỗi giây, càng bay lâu càng khó.
- **18 mẫu bóng bay:** 10 màu cổ điển và 8 hình dáng đặc biệt (tim, sao, pixel, pixel dài).
- **Cửa hàng đầy đủ:** mua và chọn bóng bằng xu nhặt được; lưu tiến trình bằng `PlayerPrefs`.
- **Kỷ lục & xu được lưu lại** giữa các lần chơi.
- **Rừng sương mù:** nền parallax nhiều lớp, sương trôi, bụi sáng, vignette và dải cảnh báo khi đến gần ranh giới.
- **Cành cây từ mép rừng:** 3 biến thể cành xanh, vàng và rêu mọc từ mép trên/dưới, gốc cành luôn nằm ngoài vùng chơi nên không bị cắt lửng.
- **Chim nhỏ, dễ nhận biết:** kích thước chim được thu nhỏ để không che vật cản và dễ né hơn.
- **UI/UX tiếng Việt:** menu, HUD, tạm dừng, game over, cửa hàng, câu chuyện và hướng dẫn; panel/button gỗ, đinh tán và lá cây được vẽ procedural; hỗ trợ safe area cho thiết bị có tai thỏ.
- **Hiệu ứng & âm thanh procedural:** không cần file âm thanh ngoài; tiếng vỗ, xu, va chạm, mua hàng, kỷ lục và nhạc nền được tạo bằng code.
- **Một scene duy nhất**, chạy mượt trên WebGL và Windows.

## Điều khiển

| Hành động | Phím / thao tác |
|---|---|
| Bay lên | `Space`, chuột trái hoặc chạm màn hình |
| Tạm dừng / tiếp tục | `Esc` hoặc `P` |
| Chơi lại nhanh | `Space` ở màn hình game over |
| Bắt đầu từ menu | `Space` hoặc `Enter` |

## Chạy trong Unity

1. Cài **Unity 6000.3.6f1** (hoặc bản Unity 6 LTS tương thích).
2. Mở project tại thư mục gốc.
3. Mở scene `Assets/Scenes/GamePlay.unity`.
4. Nhấn **Play**.

Các hệ thống quan trọng (`GameManager`, `ScoreManager`, `ShopSystem`, `AudioManager`, `EffectsManager`, `FogController`) được tạo/khởi tạo tự động khi chạy. UI được dựng hoàn toàn bằng code trong `UIManager`, không phụ thuộc vào Canvas cũ trong scene.

## Build

Trong Unity, dùng menu:

- **Balloon Go > Build Windows**
- **Balloon Go > Build WebGL**
- **Balloon Go > Build All**

Kết quả nằm trong:

```
Builds/
  Windows/        # BalloonGo.exe + BalloonGo_Data/
  WebGL/          # index.html + Build/ + TemplateData/ + StreamingAssets/
  BalloonGo-Windows.zip
  BalloonGo-WebGL.zip
```

Với itch.io, upload file zip tương ứng. Với WebGL, đặt kích thước nhúng khuyến nghị **1280 x 720** (game vẫn co giãn theo màn hình). Bản WebGL dùng **Gzip + Decompression Fallback**, đã test cả khi server có và không có header `Content-Encoding`.

## Cấu trúc chính

```
Assets/
  Scenes/GamePlay.unity          # scene duy nhất
  Scripts/
    Core/                        # GameState, GameManager, ScoreManager, ShopSystem,
                                 # SaveSystem, AudioManager, GameBootstrap
    Player/                      # PlayerController, BalloonSkinSystem
    Obstacles/                   # Obstacle, ObstacleSpawner, BranchObstacle
    Enemies/                     # BirdEnemy, BirdSpawn
    Collectibles/                # Coin, CoinMove, CoinSpawner
    UI/                          # UIManager, UITheme, UIPanel, SafeArea, ButtonFX,
                                 # FloatingUI, ShopCardUI
    Visual/                      # FogController, EffectsManager, CameraShake,
                                 # ParticleBit, FloatingWorldText, ParallaxBackground
  Resources/
    Balloons/                    # 18 sprite bóng bay dùng cho shop/gameplay
    Obstacles/                   # 3 biến thể cành: BranchGreen, BranchGold, BranchMoss
    Fonts/                       # BeVietnamPro-SemiBold + ExtraBold (dự phòng LiberationSans)
  Editor/                        # Build script
```

## Ghi chú kỹ thuật

- UI dùng `UnityEngine.UI.Text` (legacy) với font **Be Vietnam Pro** (SemiBold/ExtraBold) để hiển thị đầy đủ dấu tiếng Việt, tránh phụ thuộc vào TMP atlas; tự fallback về **Liberation Sans** nếu thiếu font chính.
- Panel, button, card, đinh tán, lá cây và nét cọ tiêu đề được sinh procedural trong `UITheme`; coin, ring, gradient, vignette và fog cũng được vẽ bằng code.
- Cành cây dùng 3 texture trong `Resources/Obstacles`; `BranchObstacle` xoay cành từ mép trên/dưới và đặt gốc ra ngoài vùng chơi nên không lộ phần bị cắt.
- Chim được thu nhỏ (scale 0.11) để không che vật cản và dễ né hơn.
- Âm thanh được sinh procedural trong `AudioManager`; không cần import file âm thanh.
- Game reset ngay trong scene, không reload scene, nên trạng thái shop/âm thanh/UI được giữ ổn định.
- `PlayerPrefs` lưu: điểm cao, tổng xu, các bóng đã mở, bóng đang chọn, trạng thái âm thanh và đã xem câu chuyện.

## Credits

- **Balloon sprites:** Qookie Games – *Balloons Free*.
- **Bird sprites:** SR Studios Kerala – *Cute Birds* / *Cute Birds (4 in 1) Mini* / *Eagle Bird 2D*.
- **Coin sprite:** 2DRPK.
- **Forest parallax background:** *FREE Parallax Forest Background HQ*.
- **Font:** Be Vietnam Pro (SIL Open Font License); Liberation Sans làm font dự phòng (SIL OFL).
- **Code, UI, hiệu ứng và âm thanh procedural:** dự án Balloon Go.

Các asset đồ họa thuộc bản quyền của tác giả gốc và tuân theo giấy phép đi kèm của từng asset pack. Vui lòng kiểm tra lại điều khoản thương mại trước khi phát hành chính thức.

## Trạng thái

Bản hiện tại là bản hoàn thiện để chơi và upload itch.io:

- [x] Gameplay endless runner, va chạm, ranh giới, điểm, xu
- [x] Câu chuyện mở đầu và hướng dẫn
- [x] Shop 18 vật phẩm, mua/chọn/lưu
- [x] 3 biến thể cành mọc từ mép trên/dưới, gốc cành nằm ngoài vùng chơi
- [x] Chim nhỏ hơn, dễ nhận biết và né
- [x] UI/UX tiếng Việt, gỗ thủ công, responsive, safe area
- [x] Hiệu ứng, camera shake, fog, parallax không lộ đường ghép nền
- [x] Âm thanh procedural và tắt/bật
- [x] Build WebGL/Windows, test WebGL với Gzip + Decompression Fallback, đóng gói zip
