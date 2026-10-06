# WARUDO Mod Mate

**무료 와루도 모딩 도우미 — Free mod-building helper for WARUDO**

프리팹을 골라 한 번에 빌드하고, 씬 오브젝트도 빠르게 모드로 만들 수 있는 Unity 에디터 도구입니다.

**[패키지 다운로드 / Download](https://github.com/StudioRaming/WARUDO-Mod-Mate/releases/latest)** · [사용 설명서 / Manual](Assets/StudioRaming/WarudoBatchBuilder/WARUDO_Mod_Mate_Manual.md) · [MIT License](LICENSE)

## 기능

- 프리팹 일괄 빌드와 씬 오브젝트 빠른 빌드
- 필요한 런타임 스크립트 자동 수집과 수동 스크립트 폴더 설정
- 모드 목록, 빌드 이력, 재빌드
- 프리팹과 의존 에셋을 `.unitypackage`로 내보내기
- 기존 출력 백업과 실패 시 복원
- 한국어·English·日本語 UI

이 배포본은 기본 빌더입니다. Unity에서 WARUDO로 직접 전송하는 Live Test는 포함하지 않습니다. WARUDO SDK, 셰이더, 아바타 및 타사 플러그인은 별도로 준비해야 합니다.

## 설치

1. Unity **2021.3.45f2** 프로젝트를 준비합니다.
2. `.warudo` 빌드에는 WARUDO Mod Tool SDK가 필요합니다. 검증 기준 SDK는 **0.14.3.10**입니다. SDK를 별도로 설치하고 Player Settings의 API Compatibility Level을 **.NET Framework**로 설정합니다.
3. Release의 `WARUDO_Mod_Mate_1.0.0.unitypackage`를 임포트합니다.
4. **StudioRaming → WARUDO Mod Mate**를 엽니다.

SDK가 없어도 도구 설치와 `.unitypackage` 내보내기를 사용할 수 있습니다. `.warudo` 빌드는 SDK 및 모드 빌드 설정이 준비되어야 합니다. Unity 프로젝트와 WARUDO의 렌더 파이프라인을 맞춰 주세요.

업데이트할 때 같은 제품 폴더로 덮어쓰기 임포트합니다. 내부 폴더 이름 `WarudoBatchBuilder`는 기존 설정과 이력 호환을 위해 유지합니다. 다른 프로젝트에서 복사한 설정이 남아 있을 수 있으므로 빌드 전 제작자·출력 경로·선택 목록을 확인해 주세요.

## English

WARUDO Mod Mate is a free Unity Editor tool for batch-building WARUDO mods, quick-building scene objects, browsing build history and exporting prefab dependencies as Unity packages. It includes Korean, English and Japanese UI.

Use Unity **2021.3.45f2**. Building `.warudo` files requires the separately installed WARUDO Mod Tool SDK; the tested SDK baseline is **0.14.3.10**. Set API Compatibility Level to **.NET Framework** for the SDK. Import the release package, then open **StudioRaming → WARUDO Mod Mate**. Live Test and third-party assets are not included. Match the render pipeline of your project and WARUDO.

## 日本語

WARUDO Mod Mate は、プリファブの一括ビルド、シーンオブジェクトのクイックビルド、ビルド履歴の確認、Unity パッケージの書き出しを行う無料の Unity エディターツールです。

Unity **2021.3.45f2** を使用してください。`.warudo` のビルドには別途 WARUDO Mod Tool SDK が必要です。検証基準は **0.14.3.10** です。SDK 使用時は API Compatibility Level を **.NET Framework** に設定します。パッケージをインポートし、**StudioRaming → WARUDO Mod Mate** を開いてください。Live Test とサードパーティーのアセットは含まれません。Unity プロジェクトと WARUDO のレンダーパイプラインを合わせてください。

## License and support

The tool source and package are distributed under the **MIT License**. Personal and commercial use, modification and redistribution are permitted under its terms. Separately installed WARUDO software, SDKs, shaders and assets remain subject to their own licenses. This is an independent StudioRaming tool.

Support: **studioraming@gmail.com**
