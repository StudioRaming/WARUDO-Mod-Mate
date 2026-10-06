# WARUDO Mod Mate 1.0.0

## 한국어

### 설치와 준비

Unity 2021.3.45f2에서 Release의 `.unitypackage`를 임포트하고 **StudioRaming → WARUDO Mod Mate**를 엽니다. `.warudo` 제작에는 WARUDO Mod Tool SDK와 모드 빌드 설정이 필요합니다. SDK는 이 패키지에 포함하지 않습니다. SDK 사용 시 Player Settings의 API Compatibility Level을 **.NET Framework**로 설정합니다.

### 빌드

1. **빌드** 탭에서 프리팹이 있는 폴더를 선택합니다.
2. 빌드할 프리팹을 선택하고 모드 종류, 제작자, 버전, 출력 경로를 확인합니다.
3. 사전 점검의 안내를 해결한 뒤 빌드를 시작합니다.
4. 빌드 이력에서 결과와 로그를 확인합니다.

필요한 런타임 스크립트는 SDK가 있는 프로젝트에서 자동 수집합니다. 고급 설정의 수동 스크립트 폴더도 사용할 수 있습니다. SDK가 없으면 `.warudo` 빌드는 차단되고 설치 안내가 표시됩니다.

씬 오브젝트는 Hierarchy에서 우클릭하여 **WARUDO Mod Mate → Prop / Character**로 빠르게 빌드할 수 있습니다. 원본 대신 임시 복사본으로 빌드합니다.

### 목록·이력·패키지

**모드 목록**은 지정 경로의 모드 파일을 확인합니다. **빌드 이력**은 성공·실패와 재빌드를 관리합니다. 빌드 탭에서 **.unitypackage 내보내기**를 선택하면 프리팹과 의존 에셋을 Unity 패키지로 내보냅니다.

출력 파일이 있으면 빌드 전 백업하고 실패 시 이전 출력을 복원합니다. 삭제·정리·덮어쓰기 작업 전 대상과 경로를 확인하세요. 빌드 중 Unity를 강제로 종료하지 마세요.

### 업데이트와 문제 해결

상단 **? → 최신 패키지 다운로드**에서 새 버전을 받을 수 있습니다. 같은 제품 폴더로 임포트하면 됩니다. 기존 `WarudoBatchBuilder` 내부 이름과 설정 키는 유지합니다.

- SDK 또는 빌드 설정이 없다는 안내: WARUDO SDK를 설치하고 **Warudo → New Mod**로 모드 빌드 설정을 준비하세요.
- 화면이 마젠타이거나 모드가 보이지 않음: Unity와 WARUDO의 Built-in/URP 설정 및 셰이더 지원을 확인하세요.
- 컴파일·셰이더 처리 중 진행이 느림: 빌드 로그를 확인하고 처리 중인 항목이 끝날 때까지 기다리세요.
- 다른 프로젝트의 출력 경로가 표시됨: 빌드 전 경로·제작자·선택 목록을 직접 확인하세요.

이 버전에는 Live Test가 포함되지 않습니다. 문의: **studioraming@gmail.com**

## English

Import the release package into Unity **2021.3.45f2** and open **StudioRaming → WARUDO Mod Mate**. `.warudo` builds require the separately installed WARUDO Mod Tool SDK and mod export settings. Use **.NET Framework** API Compatibility Level for the SDK.

In the **Build** tab, select a prefab folder and prefabs, check the mod type, author, version and output path, resolve preflight messages, then build. Check **History** for results and logs. Required runtime scripts are collected automatically when the SDK is installed; manual script folders are available in advanced settings.

Right-click a scene object in Hierarchy and choose **WARUDO Mod Mate → Prop / Character** for a quick build. The tool builds temporary copies. Use the package-export option in the Build tab to export prefabs and dependencies as `.unitypackage` files. The SDK is not required for package export.

Existing output is backed up before a build and restored after a failed build. Check paths before deletion, cleanup or overwriting. Avoid forcing Unity to exit during a build. Existing settings may include paths from another project; verify them before building.

Open **? → Download latest package** to update, then import into the same product folder. Match your Unity project's render pipeline with WARUDO. Live Test and third-party SDKs, shaders and assets are not included.

Support: **studioraming@gmail.com**

## 日本語

Unity **2021.3.45f2** にリリースパッケージをインポートし、**StudioRaming → WARUDO Mod Mate** を開きます。`.warudo` のビルドには別途 WARUDO Mod Tool SDK とモードのビルド設定が必要です。SDK 使用時は API Compatibility Level を **.NET Framework** に設定してください。

**ビルド** タブでフォルダーとプリファブを選択し、種類・作者・バージョン・出力先を確認します。事前確認の案内を解決してビルドを開始し、**ビルド履歴** で結果とログを確認してください。SDK があれば必要なランタイムスクリプトを自動収集します。詳細設定で手動のスクリプトフォルダーも指定できます。

Hierarchy のシーンオブジェクトを右クリックし、**WARUDO Mod Mate → Prop / Character** を選ぶとクイックビルドできます。ビルドには一時コピーを使用します。ビルドタブのパッケージ書き出しで、プリファブと依存アセットを `.unitypackage` に書き出せます。この書き出しに SDK は不要です。

既存の出力はビルド前にバックアップし、失敗時に復元します。削除・整理・上書きの前に対象とパスを確認してください。ビルド中に Unity を強制終了しないでください。別のプロジェクトの設定が残る場合は、ビルド前に出力先などを確認してください。

**? → 最新パッケージをダウンロード** から更新できます。同じ製品フォルダーにインポートしてください。Unity と WARUDO のレンダーパイプラインを合わせてください。Live Test とサードパーティーの SDK・シェーダー・アセットは含まれません。

お問い合わせ: **studioraming@gmail.com**

## License

[MIT License](LICENSE.txt) · © 2026 StudioRaming

[GitHub](https://github.com/StudioRaming/WARUDO-Mod-Mate) · [Downloads](https://github.com/StudioRaming/WARUDO-Mod-Mate/releases/latest)
