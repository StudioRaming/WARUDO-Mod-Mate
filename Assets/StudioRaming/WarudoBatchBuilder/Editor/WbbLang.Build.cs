using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> BuildTexts = new Dictionary<string, string[]>
        {
            {
                "Build.SettingsSection",
                new[]
                {
                    "② 빌드 설정",
                    "② Build settings",
                    "② ビルド設定"
                }
            },
            {
                "Build.SettingsSection.tip",
                new[]
                {
                    "타입 · 출력 경로 · 버전 · 제작자 · 이름 규칙을 정합니다. 프리셋으로 저장해 두면 한 번에 되돌릴 수 있습니다.",
                    "Type, output path, version, author and naming rules. Save them as a preset to restore in one click.",
                    "タイプ・出力先・バージョン・作者・命名ルールを設定します。プリセット保存でワンクリック復元できます。"
                }
            },
            {
                "Build.Pipeline",
                new[]
                {
                    "렌더 파이프라인: {0}",
                    "Render pipeline: {0}",
                    "レンダーパイプライン: {0}"
                }
            },
            {
                "Build.Pipeline.tip",
                new[]
                {
                    "WARUDO와 렌더 파이프라인이 다르면 모드가 안 보이거나 분홍색(마젠타)으로 보입니다. 빌드 실패가 아니라 파이프라인 차이입니다.\n" + "WARUDO 설정: 왼쪽 아래 WARUDO 아이콘 → 설정 → URP/NiloToon 탭 → 활성화됨 (예 = URP, 아니요 = Built-in)\n" + "WARUDO 설정을 바꾸지 않으려면 WARUDO와 같은 파이프라인을 쓰는 프로젝트에서 다시 빌드하세요.",
                    "If WARUDO uses a different render pipeline, the mod is invisible or shows as magenta (pink). It is not a build failure, only a pipeline mismatch.\n" + "In WARUDO: open the settings from the WARUDO icon at the bottom left → URP/NiloToon tab → Enabled (on = URP, off = Built-in)\n" + "To keep WARUDO as it is, rebuild the mod in a project that uses the same pipeline as WARUDO.",
                    "WARUDO とレンダーパイプラインが異なると、MOD が表示されないか、マゼンタ（ピンク）で表示されます。ビルドの失敗ではなく、パイプラインの違いです。\n" + "WARUDO 側: 左下の WARUDO アイコンから設定を開き → URP/NiloToon タブ → 有効化（オン = URP、オフ = Built-in）\n" + "WARUDO の設定を変えない場合は、WARUDO と同じパイプラインのプロジェクトでビルドし直してください。"
                }
            },
            {
                "Build.PipelineUrp",
                new[]
                {
                    "WARUDO도 URP여야 합니다(URP/NiloToon 활성화됨: 예).",
                    "WARUDO must use URP too (URP/NiloToon Enabled: on).",
                    "WARUDO も URP にしてください（URP/NiloToon の有効化: オン）。"
                }
            },
            {
                "Build.PipelineBuiltIn",
                new[]
                {
                    "WARUDO도 Built-in이어야 합니다(URP/NiloToon 활성화됨: 아니요).",
                    "WARUDO must use Built-in too (URP/NiloToon Enabled: off).",
                    "WARUDO も Built-in にしてください（URP/NiloToon の有効化: オフ）。"
                }
            },
            {
                "Build.PipelineHdrp",
                new[]
                {
                    "WARUDO는 HDRP를 쓰지 않습니다(URP 또는 Built-in만). URP나 Built-in 프로젝트에서 빌드하세요.",
                    "WARUDO does not use HDRP (only URP or Built-in). Build from a URP or Built-in project.",
                    "WARUDO は HDRP を使いません（URP か Built-in のみ）。URP か Built-in のプロジェクトでビルドしてください。"
                }
            },
            {
                "Build.PipelineCustom",
                new[]
                {
                    "WARUDO는 URP 또는 Built-in으로 그립니다. 이 파이프라인이 WARUDO 설정과 맞는지 확인하세요.",
                    "WARUDO renders with URP or Built-in. Check that this pipeline matches the WARUDO setting.",
                    "WARUDO は URP か Built-in で描画します。このパイプラインが WARUDO の設定と合っているか確認してください。"
                }
            },
            {
                "Build.Type",
                new[]
                {
                    "모드 타입",
                    "Mod type",
                    "MOD タイプ"
                }
            },
            {
                "Build.Type.tip",
                new[]
                {
                    "Warudo 가 이 모드를 어떤 종류로 읽을지 정합니다. 타입을 바꾸면 그 타입에 저장해 둔 출력 경로가 자동으로 불러와집니다.",
                    "Decides how Warudo loads this mod. Changing the type loads the output path saved for that type.",
                    "Warudo がこの MOD をどの種類として読み込むかを決めます。タイプ変更時はそのタイプに保存された出力先を読み込みます。"
                }
            },
            {
                "Build.Output",
                new[]
                {
                    "출력 경로",
                    "Output path",
                    "出力先"
                }
            },
            {
                "Build.Output.tip",
                new[]
                {
                    "산출물(.warudo)이 저장될 폴더입니다. 보통 Warudo 설치 폴더의 StreamingAssets/Props 같은 경로입니다.\n모드 목록 탭 \"폴더 경로 설정\"과 같은 설정이라 한쪽을 바꾸면 다른 쪽도 바뀝니다.",
                    "Where the .warudo output is written. This is usually StreamingAssets/Props inside the Warudo install folder.\nIt is the same setting as \"Folder paths\" in the Library tab; changing one changes the other.",
                    "出力(.warudo)の保存先です。通常は Warudo インストール先の StreamingAssets/Props などです。\nMOD一覧タブの「フォルダパス設定」と同じ設定のため、片方を変えるともう片方も変わります。"
                }
            },
            {
                "Build.OutputPick",
                new[]
                {
                    "…",
                    "…",
                    "…"
                }
            },
            {
                "Build.OutputPick.tip",
                new[]
                {
                    "폴더 선택 창을 엽니다.",
                    "Open a folder picker.",
                    "フォルダ選択ダイアログを開きます。"
                }
            },
            {
                "Build.OutputOpen",
                new[]
                {
                    "폴더 열기",
                    "Open folder",
                    "フォルダを開く"
                }
            },
            {
                "Build.OutputOpen.tip",
                new[]
                {
                    "탐색기에서 출력 폴더를 엽니다.",
                    "Reveal the output folder in the file explorer.",
                    "エクスプローラーで出力フォルダを開きます。"
                }
            },
            {
                "Build.Version",
                new[]
                {
                    "모드 버전",
                    "Mod version",
                    "MOD バージョン"
                }
            },
            {
                "Build.Version.tip",
                new[]
                {
                    "모드 정보에 기록되는 버전 문자열입니다. 이력 배지에도 이 값이 표시됩니다. 예: 1.0.0",
                    "Version string written into the mod info; it also shows up on the status badge. e.g. 1.0.0",
                    "MOD 情報に記録されるバージョン文字列です。ステータスバッジにも表示されます。例: 1.0.0"
                }
            },
            {
                "Build.Author",
                new[]
                {
                    "제작자",
                    "Author",
                    "作者"
                }
            },
            {
                "Build.Author.tip",
                new[]
                {
                    "모드 정보에 표시될 제작자 이름입니다. 비워 두면 빌드 전 점검에서 경고가 나옵니다.",
                    "Author name shown in the mod info. Leaving it empty raises a preflight warning.",
                    "MOD 情報に表示される作者名です。空のままだと事前チェックで警告が出ます。"
                }
            },
            {
                "Build.Description",
                new[]
                {
                    "설명",
                    "Description",
                    "説明"
                }
            },
            {
                "Build.Description.tip",
                new[]
                {
                    "모드 정보에 함께 저장되는 짧은 설명입니다. 비워 둬도 됩니다.",
                    "Short description stored with the mod info. Optional.",
                    "MOD 情報に一緒に保存される短い説明です。省略できます。"
                }
            },
            {
                "Build.AppendDate.tip",
                new[]
                {
                    "같은 프리팹을 날짜별로 여러 번 낼 때 씁니다. 항상 _MM-dd 형식이라 파일명이 깨지지 않습니다.",
                    "Use it when you ship the same prefab on different days. Always _MM-dd, so the file name never breaks.",
                    "同じプレハブを日付別に出す時に使います。常に _MM-dd 形式なのでファイル名が壊れません。"
                }
            },
            {
                "Build.UseFolderPrefix",
                new[]
                {
                    "폴더 이름 쓰기",
                    "Use folder name",
                    "フォルダ名を使う"
                }
            },
            {
                "Build.UseFolderPrefix.tip",
                new[]
                {
                    "폴더에 프리팹이 하나면 모드 이름은 폴더 이름이 되고, 여러 개면 <폴더>_<프리팹> 이 됩니다. 끄면 아래 수동 접두사를 씁니다.",
                    "If the folder has one prefab, the mod name is the folder name; with several it becomes <folder>_<prefab>. When off, the manual prefix below is used.",
                    "フォルダ内のプレハブが1つなら MOD名はフォルダ名、複数なら <フォルダ>_<プレハブ> になります。オフの場合は下の手動接頭辞を使います。"
                }
            },
            {
                "Build.NamePrefix",
                new[]
                {
                    "수동 접두사",
                    "Manual prefix",
                    "手動接頭辞"
                }
            },
            {
                "Build.RunPackage",
                new[]
                {
                    "선택한 {0}개를 패키지로",
                    "Package {0} selected",
                    "選択 {0}件をパッケージ化"
                }
            },
            {
                "Build.RunPackage.tip",
                new[]
                {
                    "선택한 프리팹마다 .unitypackage 를 하나씩 만듭니다. 프리팹이 쓰는 에셋은 원래 폴더 구조대로 들어갑니다.",
                    "Makes one .unitypackage per selected prefab. The assets each prefab uses keep their original folder structure.",
                    "選択したプレハブごとに .unitypackage を1つずつ作成します。使っているアセットは元のフォルダ構造のまま入ります。"
                }
            },
            {
                "Build.PreflightPackage",
                new[]
                {
                    "패키지 모드: Warudo 모드 빌드 설정(스크립트 폴더 등)은 쓰지 않습니다.",
                    "Package mode: the Warudo mod build settings (script folder and so on) are not used.",
                    "パッケージモード: Warudo MOD のビルド設定（スクリプトフォルダなど）は使用しません。"
                }
            },
            {
                "Build.MenuRegenThumb.tip",
                new[]
                {
                    "썸네일을 다시 찍습니다. 씬에 그 프리팹이 놓여 있으면 씬뷰에서 보이는 구도로, 없으면 자동으로 프레임을 잡아 찍습니다.",
                    "Re-shoot the thumbnail. If the prefab is placed in the scene it uses the Scene view framing; otherwise it frames the object automatically.",
                    "サムネイルを撮り直します。シーンに配置されていればシーンビューの構図で、なければ自動でフレーミングして撮影します。"
                }
            },
            {
                "Build.MenuRenamePrefab",
                new[]
                {
                    "프리팹 이름 바꾸기…",
                    "Rename prefab…",
                    "プレハブ名を変更…"
                }
            },
            {
                "Build.MenuRenamePrefab.tip",
                new[]
                {
                    "프로젝트의 프리팹 파일 이름 자체를 바꿉니다. 옆에 있는 <이름>_thumb.png 썸네일도 같이 따라갑니다.",
                    "Rename the actual prefab asset in the project. Its <name>_thumb.png thumbnail is renamed alongside it.",
                    "プロジェクトのプレハブファイル名自体を変更します。隣の <名前>_thumb.png も一緒に追従します。"
                }
            },
            {
                "Build.RenamePrefabFailed",
                new[]
                {
                    "프리팹 이름을 바꾸지 못했습니다: {0}",
                    "Could not rename the prefab: {0}",
                    "プレハブ名を変更できませんでした: {0}"
                }
            },
            {
                "Build.RenamePrefabDone",
                new[]
                {
                    "'{0}' → '{1}' 로 바꿨습니다.",
                    "Renamed '{0}' → '{1}'.",
                    "「{0}」→「{1}」に変更しました。"
                }
            },
            {
                "Build.MenuRename",
                new[]
                {
                    "산출물 이름 수정…",
                    "Edit output name…",
                    "出力名を編集…"
                }
            },
            {
                "Build.MenuRename.tip",
                new[]
                {
                    "이 항목이 만들어 낼 .warudo 파일 이름을 직접 정합니다. 자동 규칙(접두사·날짜)보다 우선합니다. Enter 로 확정, Esc 로 취소.",
                    "Set the .warudo file name this item produces. It overrides the automatic rules (prefix/date). Enter to confirm, Esc to cancel.",
                    "この項目が生成する .warudo のファイル名を直接指定します。自動ルール（接頭辞・日付）より優先されます。Enterで確定、Escで取消。"
                }
            },
            {
                "Build.MenuResetName",
                new[]
                {
                    "자동 이름으로 되돌리기",
                    "Reset to automatic name",
                    "自動名に戻す"
                }
            },
            {
                "Build.MenuResetName.tip",
                new[]
                {
                    "직접 지정한 이름을 지우고 폴더명·날짜 규칙으로 만든 이름을 다시 씁니다.",
                    "Clear the manual name and go back to the name built from the folder/date rules.",
                    "手動で指定した名前を消し、フォルダ名・日付ルールで作られる名前に戻します。"
                }
            },
            {
                "Build.ExportPackage",
                new[]
                {
                    "선택 항목을 .unitypackage 로 내보내기",
                    "Export selection as .unitypackage",
                    "選択項目を .unitypackage で書き出す"
                }
            },
            {
                "Build.ExportPackage.tip",
                new[]
                {
                    "빌드 대신 선택한 프리팹을 .unitypackage 로 내보냅니다. ② 빌드 설정 카드가 '.unitypackage 내보내기'로 바뀝니다. 에셋은 원래 폴더 구조대로 들어가고, 텍스처 · 머티리얼 · FBX · 프리팹은 원하면 한 폴더로 모을 수 있습니다.",
                    "Export the selected prefabs as .unitypackage instead of building. The ② Build settings card switches to 'Export .unitypackage'. Assets keep their original folder structure; textures, materials, FBX and prefabs can optionally be gathered into one folder.",
                    "ビルドの代わりに、選択したプレハブを .unitypackage で書き出します。② ビルド設定カードが「.unitypackage を書き出す」に切り替わります。アセットは元のフォルダ構造のまま入り、テクスチャ・マテリアル・FBX・プレハブは必要なら1つのフォルダにまとめられます。"
                }
            },
            {
                "Build.Overwrite.tip",
                new[]
                {
                    "켜면(기본) 같은 이름의 .warudo 를 덮어씁니다. 끄면 이름 뒤에 _2, _3 같은 번호를 붙여 기존 파일을 남깁니다.",
                    "On (default) overwrites an existing .warudo with the same name. Off adds a number such as _2, _3 so the existing file is kept.",
                    "オン（既定）は同名の.warudoを上書きします。オフは _2, _3 のような番号を付けて既存ファイルを残します。"
                }
            },
            {
                "Build.OverwriteOffHint",
                new[]
                {
                    "→ 같은 이름의 파일이 있으면 _2, _3 처럼 번호가 붙고, 기존 파일은 그대로 둡니다.",
                    "→ If a file with the same name exists, a number like _2, _3 is added and the existing file is kept.",
                    "→ 同名のファイルがあれば _2, _3 のように番号が付き、既存ファイルはそのまま残ります。"
                }
            },
            {
                "Build.StagingFolder",
                new[]
                {
                    "스크립트 폴더",
                    "Script folder",
                    "スクリプトフォルダ"
                }
            },
            {
                "Build.StagingFolder.tip",
                new[]
                {
                    "여러 모드가 함께 쓰는 커스텀 런타임 스크립트(.cs)를 넣어 두는 폴더입니다. 지정하면 빌드할 때마다 자동으로 함께 들어가고, 없으면 비워 두면 됩니다.",
                    "Folder for custom runtime scripts (.cs) shared by several mods. When set, they are included in every build automatically; leave it empty if you have none.",
                    "複数のMODで共通して使うカスタムランタイムスクリプト(.cs)を置くフォルダです。指定するとビルドのたびに自動で含まれます。無ければ空のままで構いません。"
                }
            },
            {
                "Build.ModeWarudo",
                new[]
                {
                    "Warudo 모드 빌드",
                    "Build Warudo mod",
                    "Warudo MODをビルド"
                }
            },
            {
                "Build.ModePackage",
                new[]
                {
                    ".unitypackage 내보내기",
                    "Export .unitypackage",
                    ".unitypackage を書き出す"
                }
            },
            {
                "Build.StagingNotSet",
                new[]
                {
                    "(미지정: 임시 폴더 사용)",
                    "(not set: a temporary folder will be used)",
                    "(未設定: 一時フォルダを使用)"
                }
            },
            {
                "Build.StagingShared",
                new[]
                {
                    "이 타입 전용 지정이 없어 공통 폴더를 씁니다.",
                    "No per-type folder is set, so the shared one is used.",
                    "このタイプ専用の指定がないため共通フォルダを使用します。"
                }
            },
            {
                "Build.StagingMustBeInProject",
                new[]
                {
                    "스크립트 폴더는 이 프로젝트의 Assets 안에 있어야 합니다. 빌드할 때 프리팹 복사본을 잠시 그 폴더에 넣기 때문입니다.",
                    "The script folder must be inside this project's Assets folder, because a copy of the prefab is placed there briefly during the build.",
                    "スクリプトフォルダはこのプロジェクトの Assets 内にある必要があります。ビルド中にプレハブのコピーを一時的にそこへ置くためです。"
                }
            },
            {
                "Build.NamePrefix.tip",
                new[]
                {
                    "폴더 이름 대신 붙일 접두사입니다. 비우면 프리팹 이름만 씁니다.",
                    "Prefix used instead of the folder name. Leave it empty to use the prefab name only.",
                    "フォルダ名の代わりに付ける接頭辞です。空ならプレハブ名のみになります。"
                }
            },
            {
                "Build.NamePreview",
                new[]
                {
                    "모드명 미리보기:  {0}",
                    "Mod name preview:  {0}",
                    "MOD名プレビュー:  {0}"
                }
            },
            {
                "Build.ViewGrid",
                new[]
                {
                    "그리드",
                    "Grid",
                    "グリッド"
                }
            },
            {
                "Build.ViewGrid.tip",
                new[]
                {
                    "썸네일 카드로 봅니다. 어떤 프리팹인지 눈으로 고를 때 편합니다.",
                    "Thumbnail cards. Best when you pick items visually.",
                    "サムネイルカード表示。見た目で選ぶときに便利です。"
                }
            },
            {
                "Build.ViewList",
                new[]
                {
                    "리스트",
                    "List",
                    "リスト"
                }
            },
            {
                "Build.ViewList.tip",
                new[]
                {
                    "한 줄 행으로 봅니다. 버전·빌드 일시를 비교할 때 편합니다.",
                    "One row per item. Best when comparing versions and build dates.",
                    "1行ずつの表示。バージョンやビルド日時の比較に便利です。"
                }
            },
            {
                "Build.Rescan",
                new[]
                {
                    "재스캔",
                    "Rescan",
                    "再スキャン"
                }
            },
            {
                "Build.Rescan.tip",
                new[]
                {
                    "프로젝트의 프리팹을 다시 찾고 빌드 상태 배지를 갱신합니다. 썸네일은 만들지 않습니다.",
                    "Find the prefabs in the project again and refresh the status badges. No thumbnail is created.",
                    "プロジェクトのプレハブを探し直してステータスバッジを更新します。サムネイルは作成しません。"
                }
            },
            {
                "Build.GenerateThumbs",
                new[]
                {
                    "썸네일 일괄 생성",
                    "Generate thumbnails",
                    "サムネイル一括生成"
                }
            },
            {
                "Build.GenerateThumbs.tip",
                new[]
                {
                    "썸네일이 없는 항목만 골라 씬뷰 구도로 PNG 를 만듭니다. 선택된 항목이 있으면 그 안에서만 만듭니다. 진행 중 취소할 수 있습니다.",
                    "Render PNGs (scene-view framing) only for items without a thumbnail. If something is selected, only those are processed. Cancelable.",
                    "サムネイルが無い項目だけをシーンビュー構図で PNG 化します。選択があればその中だけ処理します。途中でキャンセルできます。"
                }
            },
            {
                "Build.SortStatus",
                new[]
                {
                    "상태순",
                    "Status",
                    "状態順"
                }
            },
            {
                "Build.CountOtherScope",
                new[]
                {
                    "다른 폴더 {0}개 포함",
                    "{0} from other folders included",
                    "他のフォルダの {0}件を含む"
                }
            },
            {
                "Build.CountLine",
                new[]
                {
                    "표시 {0}개 · 선택 {1}개",
                    "{0} shown · {1} selected",
                    "表示 {0}件 · 選択 {1}件"
                }
            },
            {
                "Build.ThumbNone",
                new[]
                {
                    "썸네일 없음",
                    "No thumbnail",
                    "サムネイル無し"
                }
            },
            {
                "Build.ThumbCreate",
                new[]
                {
                    "생성",
                    "Create",
                    "生成"
                }
            },
            {
                "Build.ThumbCreate.tip",
                new[]
                {
                    "이 프리팹의 썸네일 PNG 를 지금 만듭니다. 씬뷰 카메라 구도로 찍힙니다.",
                    "Render this prefab's thumbnail PNG now, using the scene view camera framing.",
                    "このプレハブのサムネイル PNG を今すぐ作成します。シーンビューのカメラ構図で撮影されます。"
                }
            },
            {
                "Build.Excluded",
                new[]
                {
                    "제외",
                    "Excluded",
                    "除外"
                }
            },
            {
                "Build.RowMenu.tip",
                new[]
                {
                    "이 항목의 메뉴를 엽니다. (우클릭과 같습니다)",
                    "Open this item's menu. (Same as right-click)",
                    "この項目のメニューを開きます。（右クリックと同じ）"
                }
            },
            {
                "Build.StatusNotBuilt",
                new[]
                {
                    "미빌드",
                    "Not built",
                    "未ビルド"
                }
            },
            {
                "Build.StatusSourceChanged",
                new[]
                {
                    "소스 변경됨",
                    "Source changed",
                    "ソース変更あり"
                }
            },
            {
                "Build.StatusMissingOutput",
                new[]
                {
                    "산출물 없음",
                    "Output missing",
                    "出力が見つからない"
                }
            },
            {
                "Build.StatusFailed",
                new[]
                {
                    "실패",
                    "Failed",
                    "失敗"
                }
            },
            {
                "Build.BadgeTipNone",
                new[]
                {
                    "아직 이 프리팹으로 빌드한 기록이 없습니다.",
                    "This prefab has never been built.",
                    "このプレハブのビルド記録はまだありません。"
                }
            },
            {
                "Build.BadgeTip",
                new[]
                {
                    "모드명: {0}\n버전: {1}\n빌드 일시: {2}\n크기: {3}\n산출물: {4}",
                    "Mod name: {0}\nVersion: {1}\nBuilt at: {2}\nSize: {3}\nOutput: {4}",
                    "MOD名: {0}\nバージョン: {1}\nビルド日時: {2}\nサイズ: {3}\n出力: {4}"
                }
            },
            {
                "Build.BadgeTipChanged",
                new[]
                {
                    "빌드 이후 프리팹이 수정되었습니다. 다시 빌드하는 것을 권장합니다.",
                    "The prefab changed after that build. Rebuilding is recommended.",
                    "そのビルド以降にプレハブが変更されています。再ビルドを推奨します。"
                }
            },
            {
                "Build.BadgeTipMissing",
                new[]
                {
                    "기록된 산출 파일이 지금은 그 경로에 없습니다.",
                    "The recorded output file is no longer at that path.",
                    "記録された出力ファイルが現在そのパスにありません。"
                }
            },
            {
                "Build.CardTip",
                new[]
                {
                    "{0}\n모드명: {1}\n경로: {2}\n\n클릭=단독 선택 · Ctrl+클릭=토글 · Shift+클릭=범위 선택\n더블클릭=Project에서 보기 · 우클릭=메뉴",
                    "{0}\nMod name: {1}\nPath: {2}\n\nClick=select · Ctrl+Click=toggle · Shift+Click=range\nDouble-click=show in Project · Right-click=menu",
                    "{0}\nMOD名: {1}\nパス: {2}\n\nクリック=単独選択 · Ctrl+クリック=トグル · Shift+クリック=範囲選択\nダブルクリック=Projectで表示 · 右クリック=メニュー"
                }
            },
            {
                "Build.MenuBuildThis",
                new[]
                {
                    "이 항목만 빌드",
                    "Build only this item",
                    "この項目だけビルド"
                }
            },
            {
                "Build.MenuRegenThumb",
                new[]
                {
                    "썸네일 다시 만들기",
                    "Regenerate thumbnail",
                    "サムネイルを作り直す"
                }
            },
            {
                "Build.ThumbRegenOk",
                new[]
                {
                    "다시 만들기",
                    "Regenerate",
                    "作り直す"
                }
            },
            {
                "Build.PresetOverwriteOk",
                new[]
                {
                    "덮어쓰기",
                    "Overwrite",
                    "上書き"
                }
            },
            {
                "Build.MenuOpenThumb",
                new[]
                {
                    "썸네일 열기",
                    "Open thumbnail",
                    "サムネイルを開く"
                }
            },
            {
                "Build.MenuPing",
                new[]
                {
                    "Project 창에서 보기",
                    "Show in Project window",
                    "Projectウィンドウで表示"
                }
            },
            {
                "Build.MenuOpenOutput",
                new[]
                {
                    "최근 산출물 폴더 열기",
                    "Open last output folder",
                    "最近の出力フォルダを開く"
                }
            },
            {
                "Build.ThumbConfirmTitle",
                new[]
                {
                    "썸네일 생성",
                    "Generate thumbnails",
                    "サムネイル生成"
                }
            },
            {
                "Build.ThumbConfirmBody",
                new[]
                {
                    "썸네일이 없는 프리팹 {0}개의 썸네일 PNG 를 만듭니다.\n파일은 각 프리팹 옆에 <이름>_thumb.png 로 저장됩니다.\n계속할까요?",
                    "Thumbnail PNGs will be created for {0} prefab(s) that have none.\nEach file is saved next to its prefab as <name>_thumb.png.\nContinue?",
                    "サムネイルが無いプレハブ {0}件の PNG を作成します。\n各プレハブの隣に <名前>_thumb.png として保存されます。\n続行しますか？"
                }
            },
            {
                "Build.ThumbRegenConfirm",
                new[]
                {
                    "'{0}' 의 썸네일 파일을 다시 만들어 덮어씁니다.\n기존 PNG 1개가 사라집니다. 계속할까요?",
                    "The thumbnail of '{0}' will be re-rendered and overwritten.\n1 existing PNG will be replaced. Continue?",
                    "「{0}」のサムネイルを作り直して上書きします。\n既存の PNG 1件が置き換わります。続行しますか？"
                }
            },
            {
                "Build.ThumbNoTarget",
                new[]
                {
                    "썸네일을 만들 대상이 없습니다. 모든 항목에 이미 썸네일이 있습니다.",
                    "Nothing to do. Every item already has a thumbnail.",
                    "対象がありません。すべての項目に既にサムネイルがあります。"
                }
            },
            {
                "Build.ThumbProgressTitle",
                new[]
                {
                    "썸네일 생성 중",
                    "Generating thumbnails",
                    "サムネイル生成中"
                }
            },
            {
                "Build.ThumbDone",
                new[]
                {
                    "썸네일 {0}개를 만들었습니다.",
                    "Created {0} thumbnail(s).",
                    "サムネイルを {0}件 作成しました。"
                }
            },
            {
                "Build.ThumbFail",
                new[]
                {
                    "썸네일 생성 실패: {0}",
                    "Thumbnail generation failed: {0}",
                    "サムネイル生成に失敗しました: {0}"
                }
            },
            {
                "Build.ThumbMissing",
                new[]
                {
                    "이 프리팹에는 썸네일 파일이 없습니다.",
                    "This prefab has no thumbnail file.",
                    "このプレハブにはサムネイルファイルがありません。"
                }
            },
            {
                "Build.PrefabLoadFail",
                new[]
                {
                    "프리팹을 불러오지 못했습니다: {0}",
                    "Could not load the prefab: {0}",
                    "プレハブを読み込めませんでした: {0}"
                }
            },
            {
                "Build.PresetOverwrite",
                new[]
                {
                    "같은 이름의 프리셋 '{0}' 이(가) 이미 있습니다.\n지금 설정으로 덮어쓸까요?",
                    "A preset named '{0}' already exists.\nOverwrite it with the current settings?",
                    "同名のプリセット「{0}」が既にあります。\n現在の設定で上書きしますか？"
                }
            },
            {
                "Build.PresetDeleteConfirm",
                new[]
                {
                    "프리셋 '{0}' 1개를 삭제합니다.\n되돌릴 수 없습니다. 계속할까요?",
                    "Preset '{0}' (1 item) will be deleted.\nThis cannot be undone. Continue?",
                    "プリセット「{0}」1件を削除します。\n元に戻せません。続行しますか？"
                }
            },
            {
                "Build.PresetApplied",
                new[]
                {
                    "프리셋 '{0}' 을(를) 적용했습니다.",
                    "Applied preset '{0}'.",
                    "プリセット「{0}」を適用しました。"
                }
            },
            {
                "Build.Run",
                new[]
                {
                    "선택한 {0}개 빌드",
                    "Build {0} Selected",
                    "選択 {0}件をビルド"
                }
            },
            {
                "Build.Run.tip",
                new[]
                {
                    "선택한 항목을 하나씩 순서대로 굽습니다. 시작 전에 점검 결과와 요약을 한 번 더 보여줍니다.",
                    "Builds the selected items one by one. A preflight summary is shown before it starts.",
                    "選択項目を1件ずつ順にビルドします。開始前に事前チェックと概要を表示します。"
                }
            },
            {
                "Build.RunNone",
                new[]
                {
                    "빌드할 항목 없음",
                    "Nothing Selected",
                    "対象がありません"
                }
            },
            {
                "Build.Stop",
                new[]
                {
                    "빌드 중단",
                    "Stop Build",
                    "ビルドを中断"
                }
            },
            {
                "Build.Stop.tip",
                new[]
                {
                    "아직 준비 중인 항목은 바로 멈추고 중단으로 기록하며, 빌드가 이미 시작된 항목은 끝까지 마칩니다. 남은 항목은 시작하지 않습니다. 그다음 빌드용 복사본을 지우고 모드 빌드 설정을 원래대로 되돌립니다. 원본 프리팹은 건드리지 않습니다.",
                    "An item still being prepared stops right away and is recorded as canceled; an item whose build has already started is finished. The remaining items are not started. Then the build copy is removed and the mod's build settings are restored. Your original prefab is never touched.",
                    "まだ準備中の項目はすぐに止めて中断として記録し、ビルドが始まっている項目は最後まで完了させます。残りの項目は開始しません。その後ビルド用コピーを削除し、MODのビルド設定を元に戻します。元のプレハブには触れません。"
                }
            },
            {
                "Build.Progress",
                new[]
                {
                    "({0} / {1}) {2}",
                    "({0} / {1}) {2}",
                    "({0} / {1}) {2}"
                }
            },
            {
                "Build.PreflightOk",
                new[]
                {
                    "점검 통과: 선택한 {0}개를 바로 빌드할 수 있습니다.",
                    "Preflight passed: {0} selected item(s) are ready to build.",
                    "事前チェック通過: 選択した {0}件をすぐビルドできます。"
                }
            },
            {
                "Build.PreflightWarnLine",
                new[]
                {
                    "경고 {0}건: 빌드는 가능합니다. 시작할 때 내용을 보여줍니다.",
                    "{0} warning(s): you can still build. Details are shown when you start.",
                    "警告 {0}件: ビルドは可能です。開始時に内容を表示します。"
                }
            },
            {
                "Build.PreflightBlockLine",
                new[]
                {
                    "해결해야 할 문제 {0}건: 지금은 빌드할 수 없습니다.",
                    "{0} blocking problem(s): building is not possible yet.",
                    "解決が必要な問題 {0}件: 現在はビルドできません。"
                }
            },
            {
                "Build.ConfirmTitle",
                new[]
                {
                    "빌드 시작 확인",
                    "Confirm build",
                    "ビルド開始の確認"
                }
            },
            {
                "Build.ConfirmOtherScope",
                new[]
                {
                    "다른 폴더 {0}개 포함: {1}",
                    "{0} from other folders included: {1}",
                    "他のフォルダの {0}件を含む: {1}"
                }
            },
            {
                "Build.ConfirmBody",
                new[]
                {
                    "타입: {0}\n대상: {1}개\n버전: {2}\n제작자: {3}\n출력 폴더: {4}\n\n만들어질 파일:\n{5}",
                    "Type: {0}\nItems: {1}\nVersion: {2}\nAuthor: {3}\nOutput folder: {4}\n\nFiles to be created:\n{5}",
                    "タイプ: {0}\n対象: {1}件\nバージョン: {2}\n作者: {3}\n出力フォルダ: {4}\n\n作成されるファイル:\n{5}"
                }
            },
            {
                "Build.ConfirmMore",
                new[]
                {
                    "… 외 {0}개",
                    "… and {0} more",
                    "… ほか {0}件"
                }
            },
            {
                "Build.ConfirmGo",
                new[]
                {
                    "빌드 시작",
                    "Start build",
                    "ビルド開始"
                }
            },
            {
                "Build.WarnHeader",
                new[]
                {
                    "\n\n확인이 필요한 항목:",
                    "\n\nThings to check:",
                    "\n\n確認が必要な項目:"
                }
            },
            {
                "Build.NoAuthorUnset",
                new[]
                {
                    "(비어 있음)",
                    "(empty)",
                    "(未設定)"
                }
            },
            {
                "Build.ResultHeadDone",
                new[]
                {
                    "빌드 완료",
                    "Build finished",
                    "ビルド完了"
                }
            },
            {
                "Build.ResultHeadCanceled",
                new[]
                {
                    "빌드 중단됨",
                    "Build canceled",
                    "ビルド中断"
                }
            },
            {
                "Build.ResultSuccess",
                new[]
                {
                    "성공 {0}",
                    "{0} succeeded",
                    "成功 {0}"
                }
            },
            {
                "Build.ResultFailed",
                new[]
                {
                    "실패 {0}",
                    "{0} failed",
                    "失敗 {0}"
                }
            },
            {
                "Build.ResultCanceledItems",
                new[]
                {
                    "중단 {0}",
                    "{0} canceled",
                    "中断 {0}"
                }
            },
            {
                "Build.ResultSkipped",
                new[]
                {
                    "건너뜀 {0}",
                    "{0} skipped",
                    "スキップ {0}"
                }
            },
            {
                "Build.StatusClose.tip",
                new[]
                {
                    "이 안내 줄을 닫습니다.",
                    "Dismiss this message.",
                    "このお知らせを閉じます。"
                }
            },
            {
                "Build.ResultClose.tip",
                new[]
                {
                    "결과 줄을 닫습니다. 결과는 빌드 이력 탭에서 다시 볼 수 있습니다.",
                    "Close this result line. You can see the results again in the History tab.",
                    "結果の行を閉じます。結果はビルド履歴タブで再確認できます。"
                }
            },
            {
                "Build.OpenOutput",
                new[]
                {
                    "출력 폴더 열기",
                    "Open output folder",
                    "出力フォルダを開く"
                }
            },
            {
                "Build.OpenOutput.tip",
                new[]
                {
                    "방금 빌드한 산출물이 있는 폴더를 탐색기에서 엽니다.",
                    "Reveal the folder that received the outputs.",
                    "出力先フォルダをエクスプローラーで開きます。"
                }
            },
            {
                "Build.OpenHistory",
                new[]
                {
                    "빌드 이력 보기",
                    "Open history",
                    "ビルド履歴を見る"
                }
            },
            {
                "Build.OpenHistory.tip",
                new[]
                {
                    "빌드 이력 탭으로 이동해서 방금 결과를 확인합니다.",
                    "Switch to the History tab to inspect what just happened.",
                    "ビルド履歴タブに移動して結果を確認します。"
                }
            },
            {
                "Build.RebuildQueued",
                new[]
                {
                    "재빌드 준비 완료: '{0}' 1개만 선택했습니다. 아래 빌드 버튼을 누르면 시작합니다.",
                    "Ready to rebuild: only '{0}' is selected. Press the build button below to start.",
                    "再ビルド準備完了: 「{0}」1件のみ選択しました。下のビルドボタンで開始します。"
                }
            },
            {
                "Build.RebuildQueuedMany",
                new[]
                {
                    "재빌드 준비 완료: {0}개를 선택했습니다. 아래 빌드 버튼을 누르면 시작합니다.",
                    "Ready to rebuild: {0} item(s) selected. Press the build button below to start.",
                    "再ビルド準備完了: {0}件を選択しました。下のビルドボタンで開始します。"
                }
            },
            {
                "Build.RebuildNotFound",
                new[]
                {
                    "재빌드 대상 프리팹을 현재 폴더에서 찾지 못했습니다: {0}",
                    "The prefab to rebuild was not found in the current folder: {0}",
                    "再ビルド対象のプレハブが現在のフォルダに見つかりません: {0}"
                }
            },
            {
                "Build.RebuildSwitched",
                new[]
                {
                    "설정이 다른 재빌드 요청입니다. 앞서 쌓아둔 {0}개 선택을 지우고 '{1}' 부터 새로 시작합니다.",
                    "This rebuild uses different settings. The {0} previously queued item(s) were cleared, starting over from '{1}'.",
                    "設定が異なる再ビルド要求です。先に溜めた {0}件の選択を破棄し、「{1}」から新しく開始します。"
                }
            },
            {
                "Build.BusyLock",
                new[]
                {
                    "빌드가 진행 중입니다. 끝나거나 중단할 때까지 설정은 잠깁니다.",
                    "A build is running. Settings stay locked until it finishes or is stopped.",
                    "ビルドが進行中です。終了または中断まで設定はロックされます。"
                }
            },
            {
                "Guide.Build",
                new[]
                {
                    "<b>1. 프리팹을 찾습니다.</b>  목록에는 작업한 프리팹(작업 폴더 · 물리 있음 · 빌드한 적 있음 · 고정함)이 폴더별로 나옵니다. " + "왼쪽 <b>① 폴더</b>에서 폴더를 누르면 그 안만 봅니다. 작업 전 원본과 직접 숨긴 프리팹은 <b>숨김 N개</b> 칩을 누르면 보입니다.\n\n" + "<b>2. 고릅니다.</b>  클릭은 하나만, <b>Ctrl+클릭</b>은 추가/해제, <b>Shift+클릭</b>은 범위 선택입니다. 더블클릭은 Project 창에서 보기, 우클릭은 메뉴입니다. " + "항목 이름이 만들어질 모드 이름입니다. 배지는 빌드 상태(미빌드 · 빌드됨 · 소스 변경됨 · 산출물 없음 · 실패)이고, 마우스를 올리면 지난 빌드 정보가 보입니다.\n\n" + "<b>3. ② 빌드 설정을 확인합니다.</b>  모드 타입 · 버전 · 제작자 · 출력 경로와 이름 규칙을 정합니다. " + "② 카드 <b>고급</b> 버튼 왼쪽의 <b>URP</b> · <b>Built-in</b> 배지는 이 프로젝트의 렌더 파이프라인이며, WARUDO 설정(URP/NiloToon 탭의 활성화됨: 예 = URP, 아니요 = Built-in)과 같아야 모드가 제대로 보입니다. " + "모드에 넣을 스크립트가 있을 때만 <b>고급</b>에서 <b>스크립트 폴더</b>를 지정합니다. 자주 쓰는 설정은 <b>프리셋</b> 메뉴에 저장합니다.\n\n" + "<b>4. 빌드합니다.</b>  아래 <b>선택한 N개 빌드</b>를 누르면 점검 결과와 요약을 보여 준 뒤 시작합니다. " + "창을 닫아도 끝까지 진행되고, <b>빌드 중단</b>으로 멈출 수 있습니다.\n\n" + "<b>5. 원본은 그대로입니다.</b>  빌드는 프리팹 복사본으로만 합니다. 끝나면(실패 · 중단 포함) 복사본을 지우고 모드 빌드 설정을 원래대로 되돌립니다.\n\n" + "<b>6. .unitypackage 로 내보내기.</b>  ② 카드 위쪽에서 <b>.unitypackage 내보내기</b>를 고르면 선택한 프리팹마다 패키지를 하나씩 만듭니다. " + "에셋은 원래 폴더 구조대로 들어가고, <b>한 폴더로 모으기</b>로 텍스처 · 머티리얼 · FBX · 프리팹을 한곳에 모을 수 있습니다. " + "뺄 에셋은 목록에서 체크를 풀고 <b>N개 내보내기</b>를 누릅니다.",
                    "<b>1. Find your prefabs.</b>  The list shows the prefabs you worked on (work folder, has physics, built before, pinned), grouped by folder. " + "Click a folder in <b>① Folders</b> on the left to see only that folder. Untouched originals and prefabs you hid appear when you click the <b>N hidden</b> chip.\n\n" + "<b>2. Select.</b>  Click selects one item, <b>Ctrl+Click</b> adds or removes, <b>Shift+Click</b> selects a range. Double-click shows it in the Project window, right-click opens the menu. " + "The item name is the mod name you will get. The badge is the build state (Not built, Built, Source changed, Output missing, Failed); hover it to see the last build.\n\n" + "<b>3. Check ② Build settings.</b>  Set the mod type, version, author, output path and naming rules. " + "The <b>URP</b> / <b>Built-in</b> badge left of the <b>Advanced</b> button on the ② card is this project's render pipeline; WARUDO must use the same one (URP/NiloToon tab, Enabled: on = URP, off = Built-in) or the mod will not show correctly. " + "Set a <b>Script folder</b> under <b>Advanced</b> only when the mod ships scripts. Use the <b>Preset</b> menu to save settings you reuse.\n\n" + "<b>4. Build.</b>  Press <b>Build N Selected</b> at the bottom. The check results and a summary are shown before it starts. " + "It keeps going even if you close the window, and <b>Stop Build</b> stops it.\n\n" + "<b>5. Your originals stay as they are.</b>  Builds use a copy of each prefab. When a build ends (failures and stops included), the copies are removed and the mod's build settings are restored.\n\n" + "<b>6. Export .unitypackage.</b>  Pick <b>Export .unitypackage</b> at the top of the ② card to make one package per selected prefab. " + "Assets keep their original folder structure, and <b>Gather into one folder</b> can put textures, materials, FBX and prefabs in one place. " + "Untick the assets to leave out in the list, then press <b>Export N</b>.",
                    "<b>1. プレハブを探します。</b>  一覧には作業したプレハブ（作業フォルダ・物理あり・ビルド履歴あり・固定）がフォルダ別に表示されます。" + "左の <b>① フォルダ</b> でフォルダをクリックするとその中だけを表示します。未作業の元データと手動で隠したプレハブは <b>非表示 N件</b> チップを押すと表示されます。\n\n" + "<b>2. 選択します。</b>  クリックで1つだけ、<b>Ctrl+クリック</b>で追加/解除、<b>Shift+クリック</b>で範囲選択です。ダブルクリックで Project ウィンドウに表示、右クリックでメニューです。" + "項目名が作成される MOD 名です。バッジはビルド状態（未ビルド・ビルド済・ソース変更あり・出力が見つからない・失敗）で、マウスを乗せると前回のビルド情報が表示されます。\n\n" + "<b>3. ② ビルド設定を確認します。</b>  MOD タイプ・バージョン・作者・出力先と命名ルールを決めます。" + "② カードの <b>詳細設定</b> ボタンの左にある <b>URP</b> / <b>Built-in</b> バッジはこのプロジェクトのレンダーパイプラインで、WARUDO の設定（URP/NiloToon タブの有効化: オン = URP、オフ = Built-in）と同じでないと MOD が正しく表示されません。" + "MOD にスクリプトを入れる時だけ <b>詳細設定</b> で <b>スクリプトフォルダ</b> を指定します。よく使う設定は <b>プリセット</b> メニューに保存できます。\n\n" + "<b>4. ビルドします。</b>  下の <b>選択 N件をビルド</b> を押すと、チェック結果と概要を表示してから開始します。" + "ウィンドウを閉じても最後まで進み、<b>ビルドを中断</b> で止められます。\n\n" + "<b>5. 元のファイルはそのままです。</b>  ビルドはプレハブのコピーで行います。終わると（失敗・中断を含む）コピーを削除し、MOD のビルド設定を元に戻します。\n\n" + "<b>6. .unitypackage で書き出す。</b>  ② カード上部で <b>.unitypackage を書き出す</b> を選ぶと、選択したプレハブごとにパッケージを1つずつ作ります。" + "アセットは元のフォルダ構造のまま入り、<b>1つのフォルダにまとめる</b> でテクスチャ・マテリアル・FBX・プレハブを1か所に集められます。" + "除外するアセットは一覧でチェックを外し、<b>N件を書き出す</b> を押します。"
                }
            },
        };
    }
}
