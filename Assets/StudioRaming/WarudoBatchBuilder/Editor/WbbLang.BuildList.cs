using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> BuildListTexts = new Dictionary<string, string[]>
        {
            {
                "Build.AdvancedSection",
                new[]
                {
                    "고급",
                    "Advanced",
                    "詳細設定"
                }
            },
            {
                "Build.AdvancedToggle.tip",
                new[]
                {
                    "설명 · 스크립트 폴더 · VRC 컴포넌트 빼기를 펴거나 접습니다. 접어도 설정은 그대로 적용됩니다.",
                    "Show or hide Description, Script folder and Remove VRC components. The settings still apply while hidden.",
                    "説明・スクリプトフォルダ・VRC コンポーネント除去を開閉します。閉じても設定はそのまま適用されます。"
                }
            },
            {
                "Build.AppendDate.short",
                new[]
                {
                    "날짜 붙이기 (_MM-dd)",
                    "Add date (_MM-dd)",
                    "日付を付ける (_MM-dd)"
                }
            },
            {
                "Build.Overwrite.short",
                new[]
                {
                    "같은 이름 덮어쓰기",
                    "Overwrite same name",
                    "同名を上書き"
                }
            },
            {
                "Build.OutputNotSet",
                new[]
                {
                    "(출력 폴더 미지정)",
                    "(output folder not set)",
                    "(出力先未設定)"
                }
            },
            {
                "Build.OutputNotFoundHint",
                new[]
                {
                    "Warudo 설치 폴더를 자동으로 찾지 못했습니다. [ … ] 버튼으로 출력 폴더를 지정하세요. 보통 Warudo_Data/StreamingAssets/Props 같은 폴더입니다.",
                    "The Warudo install folder could not be found automatically. Set the output folder with the [ … ] button. It is usually a folder like Warudo_Data/StreamingAssets/Props.",
                    "Warudo のインストール先を自動で見つけられませんでした。[ … ] ボタンで出力先を指定してください。通常は Warudo_Data/StreamingAssets/Props のようなフォルダです。"
                }
            },
            {
                "Build.PresetActive",
                new[]
                {
                    "프리셋: {0}",
                    "Preset: {0}",
                    "プリセット: {0}"
                }
            },
            {
                "Build.PresetMenu.tip",
                new[]
                {
                    "지금 빌드 설정을 프리셋으로 저장하거나, 저장한 프리셋을 적용 · 덮어쓰기 · 삭제합니다.",
                    "Save the current build settings as a preset, or apply, overwrite or delete a saved one.",
                    "現在のビルド設定をプリセットとして保存するか、保存済みのプリセットを適用・上書き・削除します。"
                }
            },
            {
                "Build.PresetSaveNew",
                new[]
                {
                    "지금 설정을 새 프리셋으로 저장",
                    "Save current settings as a new preset",
                    "現在の設定を新しいプリセットとして保存"
                }
            },
            {
                "Build.PresetApplyMenu",
                new[]
                {
                    "적용",
                    "Apply",
                    "適用"
                }
            },
            {
                "Build.PresetOverwriteMenu",
                new[]
                {
                    "지금 설정으로 덮어쓰기",
                    "Overwrite with current settings",
                    "現在の設定で上書き"
                }
            },
            {
                "Build.PresetDeleteMenu",
                new[]
                {
                    "삭제",
                    "Delete",
                    "削除"
                }
            },
            {
                "Build.TreeToggle.tip",
                new[]
                {
                    "① 폴더 트리를 보이거나 감춥니다. 창이 좁으면 트리가 자동으로 접히고, 이 버튼으로 잠깐 펼 수 있습니다.",
                    "Show or hide the ① folder tree. In a narrow window the tree folds away automatically; use this button to open it for a moment.",
                    "① フォルダツリーを表示・非表示にします。ウィンドウが狭いとツリーは自動で折りたたまれ、このボタンで一時的に開けます。"
                }
            },
            {
                "Build.ScopeChip",
                new[]
                {
                    "폴더: {0}",
                    "Folder: {0}",
                    "フォルダ: {0}"
                }
            },
            {
                "Build.ScopeChip.tip",
                new[]
                {
                    "{0} 안의 프리팹만 보고 있습니다. 누르면 전체 프로젝트로 돌아갑니다.",
                    "Showing only prefabs inside {0}. Click to go back to the whole project.",
                    "{0} 内のプレハブだけを表示しています。クリックでプロジェクト全体に戻ります。"
                }
            },
            {
                "Build.ScopeThis",
                new[]
                {
                    "이 폴더 안만 보기",
                    "Show only this folder",
                    "このフォルダ内だけ表示"
                }
            },
            {
                "Build.ScopeAll",
                new[]
                {
                    "전체 프로젝트 보기",
                    "Show the whole project",
                    "プロジェクト全体を表示"
                }
            },
            {
                "Build.HiddenChip",
                new[]
                {
                    "숨김 {0}개",
                    "{0} hidden",
                    "非表示 {0}件"
                }
            },
            {
                "Build.HiddenChipOn",
                new[]
                {
                    "숨김 {0}개 표시 중",
                    "Showing {0} hidden",
                    "非表示 {0}件を表示中"
                }
            },
            {
                "Build.HiddenChip.tip",
                new[]
                {
                    "작업 전 원본 {0}개와 직접 숨긴 {1}개입니다. 누르면 목록에 함께 보이고, 다시 누르면 감춥니다.",
                    "{0} untouched originals and {1} you hid yourself. Click to show them in the list, click again to hide them.",
                    "未作業の元データ {0}件と、手動で隠した {1}件です。クリックで一覧に表示し、もう一度クリックで隠します。"
                }
            },
            {
                "Build.CollapseAll",
                new[]
                {
                    "모든 폴더 접기",
                    "Collapse all folders",
                    "すべてのフォルダを折りたたむ"
                }
            },
            {
                "Build.ExpandAll",
                new[]
                {
                    "모든 폴더 펼치기",
                    "Expand all folders",
                    "すべてのフォルダを展開"
                }
            },
            {
                "Build.WorkFolderAdd",
                new[]
                {
                    "작업 폴더로 지정",
                    "Mark as work folder",
                    "作業フォルダに指定"
                }
            },
            {
                "Build.WorkFolderRemove",
                new[]
                {
                    "작업 폴더 해제",
                    "Unmark work folder",
                    "作業フォルダを解除"
                }
            },
            {
                "Build.WorkFolderAddItem",
                new[]
                {
                    "이 프리팹의 폴더를 작업 폴더로 지정",
                    "Mark this prefab's folder as a work folder",
                    "このプレハブのフォルダを作業フォルダに指定"
                }
            },
            {
                "Build.WorkFolderAddScope",
                new[]
                {
                    "지금 보는 폴더를 작업 폴더로 지정",
                    "Mark the current folder as a work folder",
                    "表示中のフォルダを作業フォルダに指定"
                }
            },
            {
                "Build.WorkFolderPickHint",
                new[]
                {
                    "왼쪽 트리에서 폴더를 먼저 고르세요. 트리에서 폴더를 우클릭해 바로 지정할 수도 있습니다.",
                    "Pick a folder in the left tree first. You can also right-click a folder there to mark it directly.",
                    "先に左のツリーでフォルダを選んでください。ツリーでフォルダを右クリックして直接指定することもできます。"
                }
            },
            {
                "Build.SuggestWork",
                new[]
                {
                    "작업 폴더로 보이는 폴더가 있습니다: {0}. 작업 폴더로 지정하면 그 안의 프리팹이 항상 목록에 보입니다.",
                    "These look like your mod folders: {0}. Mark them as work folders to always list the prefabs inside.",
                    "作業フォルダらしいフォルダがあります: {0}。作業フォルダに指定すると、中のプレハブが常に一覧に表示されます。"
                }
            },
            {
                "Build.SuggestAccept",
                new[]
                {
                    "작업 폴더로 지정",
                    "Mark as work folders",
                    "作業フォルダに指定"
                }
            },
            {
                "Build.SuggestAccept.tip",
                new[]
                {
                    "제안한 폴더를 모두 작업 폴더로 지정합니다. 트리와 목록에 별 아이콘으로 표시됩니다.",
                    "Mark all suggested folders as work folders. They are shown with a star icon in the tree and the list.",
                    "提案したフォルダをすべて作業フォルダに指定します。ツリーと一覧に星アイコンで表示されます。"
                }
            },
            {
                "Build.ListMenu.tip",
                new[]
                {
                    "목록 메뉴: 전체 선택 · 정렬 · 모두 접기/펴기 · 썸네일 · 작업 폴더 지정 · 재스캔 · 패키지 내보내기.",
                    "List menu: select all, sort, collapse/expand all, thumbnails, mark work folder, rescan, export package.",
                    "リストメニュー: 全選択・並べ替え・すべて折りたたむ/展開・サムネイル・作業フォルダ指定・再スキャン・パッケージ書き出し。"
                }
            },
            {
                "Build.SuggestLater",
                new[]
                {
                    "건너뛰기",
                    "Skip",
                    "スキップ"
                }
            },
            {
                "Build.SuggestLater.tip",
                new[]
                {
                    "이번에만 제안을 숨깁니다. Unity를 다시 켜면 다시 제안합니다. 작업 폴더는 트리에서 폴더를 우클릭해 언제든 지정할 수 있습니다.",
                    "Hide this suggestion for now. It comes back after Unity restarts. You can mark a work folder anytime by right-clicking it in the tree.",
                    "今回だけ提案を隠します。Unityを再起動するとまた提案します。作業フォルダはツリーでフォルダを右クリックしていつでも指定できます。"
                }
            },
            {
                "Build.SuggestNever",
                new[]
                {
                    "다시 보지 않기",
                    "Don't show again",
                    "今後表示しない"
                }
            },
            {
                "Build.SuggestNever.tip",
                new[]
                {
                    "이 프로젝트에서는 작업 폴더 제안을 다시 띄우지 않습니다.",
                    "Never show work folder suggestions in this project again.",
                    "このプロジェクトでは作業フォルダの提案を今後表示しません。"
                }
            },
            {
                "Build.IndexProgress",
                new[]
                {
                    "프로젝트의 프리팹을 찾는 중…",
                    "Finding prefabs in the project…",
                    "プロジェクトのプレハブを検索中…"
                }
            },
            {
                "Build.EmptyIndexingTitle",
                new[]
                {
                    "프리팹을 찾는 중",
                    "Finding prefabs",
                    "プレハブを検索中"
                }
            },
            {
                "Build.EmptyIndexing",
                new[]
                {
                    "잠시만 기다려 주세요. 다 찾으면 여기에 목록이 나옵니다.",
                    "Please wait a moment. The list appears here when it is done.",
                    "しばらくお待ちください。検索が終わると、ここに一覧が表示されます。"
                }
            },
            {
                "Build.EmptyTitle",
                new[]
                {
                    "보이는 프리팹이 없습니다",
                    "No prefab to show",
                    "表示するプレハブがありません"
                }
            },
            {
                "Build.EmptyAllHidden",
                new[]
                {
                    "작업하지 않은 원본과 직접 숨긴 프리팹 {0}개는 기본으로 감춥니다.",
                    "{0} untouched original(s) and prefab(s) you hid are hidden by default.",
                    "未作業の元データと手動で隠したプレハブ {0}件は、標準では隠れています。"
                }
            },
            {
                "Build.EmptyShowHidden",
                new[]
                {
                    "숨긴 프리팹 보기",
                    "Show hidden prefabs",
                    "非表示のプレハブを表示"
                }
            },
            {
                "Build.EmptyNoPrefabs",
                new[]
                {
                    "이 폴더 안에는 프리팹이 없습니다. 다른 폴더를 고르거나 전체 프로젝트를 보세요.",
                    "This folder has no prefab. Pick another folder or view the whole project.",
                    "このフォルダにはプレハブがありません。別のフォルダを選ぶか、プロジェクト全体を表示してください。"
                }
            },
            {
                "Build.EmptyNoPrefabsProject",
                new[]
                {
                    "프로젝트에서 프리팹을 찾지 못했습니다. 방금 에셋을 가져왔다면 재스캔을 눌러 보세요.",
                    "No prefab was found in the project. If you just imported assets, press Rescan.",
                    "プロジェクトにプレハブが見つかりませんでした。アセットを取り込んだ直後なら再スキャンを押してください。"
                }
            },
            {
                "Build.GroupRoot",
                new[]
                {
                    "Assets 바로 아래",
                    "Directly under Assets",
                    "Assets 直下"
                }
            },
            {
                "Build.GroupCount",
                new[]
                {
                    "{0}개",
                    "{0} items",
                    "{0}件"
                }
            },
            {
                "Build.GroupBuilt",
                new[]
                {
                    "빌드됨 {0}",
                    "{0} built",
                    "ビルド済 {0}"
                }
            },
            {
                "Build.GroupChanged",
                new[]
                {
                    "변경됨 {0}",
                    "{0} changed",
                    "変更あり {0}"
                }
            },
            {
                "Build.GroupMissing",
                new[]
                {
                    "산출물 없음 {0}",
                    "{0} output missing",
                    "出力なし {0}"
                }
            },
            {
                "Build.GroupFailed",
                new[]
                {
                    "실패 {0}",
                    "{0} failed",
                    "失敗 {0}"
                }
            },
            {
                "Build.SceneGroup",
                new[]
                {
                    "씬에서 빌드한 모드",
                    "Built from scene",
                    "シーンからビルドしたMOD"
                }
            },
            {
                "Build.SceneGroup.tip",
                new[]
                {
                    "씬 오브젝트를 우클릭(WARUDO Mod Mate)해서 빌드한 모드입니다. 같은 오브젝트로 다시 빌드하거나 씬에서 위치를 볼 수 있습니다.",
                    "Mods built by right-clicking a scene object (WARUDO Mod Mate). You can rebuild them from the same object or find the object in the scene.",
                    "シーンのオブジェクトを右クリック（WARUDO Mod Mate）してビルドしたMODです。同じオブジェクトから再ビルドしたり、シーンで位置を確認できます。"
                }
            },
            {
                "Build.StatusBuiltShort",
                new[]
                {
                    "빌드됨",
                    "Built",
                    "ビルド済"
                }
            },
            {
                "Build.ReasonPinned",
                new[]
                {
                    "고정함",
                    "Pinned",
                    "固定"
                }
            },
            {
                "Build.ReasonWork",
                new[]
                {
                    "작업 폴더",
                    "Work folder",
                    "作業フォルダ"
                }
            },
            {
                "Build.ReasonBuilt",
                new[]
                {
                    "빌드한 적 있음",
                    "Built before",
                    "ビルド履歴あり"
                }
            },
            {
                "Build.ReasonPhysics",
                new[]
                {
                    "물리 있음",
                    "Has physics",
                    "物理あり"
                }
            },
            {
                "Build.ReasonRaw",
                new[]
                {
                    "원본",
                    "Original",
                    "元データ"
                }
            },
            {
                "Build.ReasonHidden",
                new[]
                {
                    "숨김",
                    "Hidden",
                    "非表示"
                }
            },
            {
                "Build.NameManual",
                new[]
                {
                    "이름 직접 지정",
                    "Custom name",
                    "手動の名前"
                }
            },
            {
                "Build.NameDuplicate",
                new[]
                {
                    "이름 겹침",
                    "Name clash",
                    "名前が重複"
                }
            },
            {
                "Build.NameOverwrites",
                new[]
                {
                    "기존 파일 덮어씀",
                    "Overwrites existing file",
                    "既存ファイルを上書き"
                }
            },
            {
                "Build.BuildCount",
                new[]
                {
                    "{0}번 빌드",
                    "Built {0} times",
                    "{0}回ビルド"
                }
            },
            {
                "Build.SceneCardTip",
                new[]
                {
                    "{0}\n씬 오브젝트: {1}\n\n더블클릭=씬에서 보기 · 우클릭=메뉴",
                    "{0}\nScene object: {1}\n\nDouble-click=show in scene · Right-click=menu",
                    "{0}\nシーンオブジェクト: {1}\n\nダブルクリック=シーンで表示 · 右クリック=メニュー"
                }
            },
            {
                "Build.RowHide.tip",
                new[]
                {
                    "목록에서 숨깁니다. 파일은 그대로 있고, 숨김 칩을 켜면 다시 볼 수 있습니다.",
                    "Hide from the list. The file stays; turn on the hidden chip to see it again.",
                    "一覧から隠します。ファイルはそのままで、非表示チップをオンにすると再び表示できます。"
                }
            },
            {
                "Build.RowUnhide.tip",
                new[]
                {
                    "다시 목록에 보이게 합니다.",
                    "Show it in the list again.",
                    "再び一覧に表示します。"
                }
            },
            {
                "Build.RowPin.tip",
                new[]
                {
                    "목록에 고정합니다. 숨김 칩을 꺼도 항상 보입니다.",
                    "Pin to the list so it always shows, even with the hidden chip off.",
                    "一覧に固定します。非表示チップがオフでも常に表示されます。"
                }
            },
            {
                "Build.MenuHide",
                new[]
                {
                    "목록에서 숨기기",
                    "Hide from list",
                    "一覧から隠す"
                }
            },
            {
                "Build.MenuUnhide",
                new[]
                {
                    "다시 보이기",
                    "Unhide",
                    "再表示"
                }
            },
            {
                "Build.MenuPin",
                new[]
                {
                    "목록에 고정",
                    "Pin to list",
                    "一覧に固定"
                }
            },
            {
                "Build.MenuUnpin",
                new[]
                {
                    "고정 해제",
                    "Unpin",
                    "固定を解除"
                }
            },
            {
                "Build.MenuPingScene",
                new[]
                {
                    "씬에서 오브젝트 보기",
                    "Show object in scene",
                    "シーンでオブジェクトを表示"
                }
            },
            {
                "Build.SceneRebuild",
                new[]
                {
                    "재빌드",
                    "Rebuild",
                    "再ビルド"
                }
            },
            {
                "Build.SceneRebuild.tip",
                new[]
                {
                    "씬의 같은 오브젝트로 이 모드를 다시 빌드합니다. 그 씬이 열려 있어야 합니다.",
                    "Rebuild this mod from the same scene object. That scene must be open.",
                    "シーンの同じオブジェクトからこのMODを再ビルドします。そのシーンが開いている必要があります。"
                }
            },
            {
                "Build.IssuesToggle.tip",
                new[]
                {
                    "눌러서 점검 결과 목록을 펼치거나 접습니다.",
                    "Click to show or hide the list of check results.",
                    "クリックでチェック結果の一覧を開閉します。"
                }
            },
            {
                "Build.PreflightDuplicateName",
                new[]
                {
                    "모드 이름 '{0}' 이(가) 선택한 다른 항목과 겹칩니다. 한쪽 이름을 바꾸거나 선택에서 빼세요.",
                    "Mod name '{0}' clashes with another selected item. Rename one of them or remove it from the selection.",
                    "MOD名「{0}」が選択中の別の項目と重複しています。どちらかの名前を変えるか、選択から外してください。"
                }
            },
            {
                "Build.WalkAwayHint",
                new[]
                {
                    "빌드를 시작했습니다. 수정된 씬은 저장해 두었습니다. 알아서 진행되니 자리를 비워도 되고, 이 창을 닫아도 끝까지 진행됩니다.",
                    "Build started. Modified scenes were saved. It runs on its own, so you can step away; it keeps going even if you close this window.",
                    "ビルドを開始しました。変更されたシーンは保存済みです。自動で進むので席を外しても大丈夫です。このウィンドウを閉じても最後まで続きます。"
                }
            },
        };
    }
}
