using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> QuickTexts = new Dictionary<string, string[]>
        {
            {
                "Quick.WindowTitle",
                new[]
                {
                    "Warudo 빌드",
                    "Warudo Build",
                    "Warudo ビルド"
                }
            },
            {
                "Quick.Inspecting",
                new[]
                {
                    "확인 중: {0}",
                    "Checking: {0}",
                    "確認中: {0}"
                }
            },
            {
                "Quick.NoTargets",
                new[]
                {
                    "빌드할 씬 오브젝트를 Hierarchy 에서 선택하세요.",
                    "Select the scene objects to build in the Hierarchy.",
                    "ビルドするシーンオブジェクトを Hierarchy で選択してください。"
                }
            },
            {
                "Quick.ObjectMissing",
                new[]
                {
                    "오브젝트가 삭제되었습니다.",
                    "The object was deleted.",
                    "オブジェクトが削除されました。"
                }
            },
            {
                "Quick.PingTip",
                new[]
                {
                    "Hierarchy 에서 이 오브젝트를 깜빡여 보여 줍니다.",
                    "Highlight this object in the Hierarchy.",
                    "Hierarchy でこのオブジェクトを示します。"
                }
            },
            {
                "Quick.ModName",
                new[]
                {
                    "모드 이름",
                    "Mod name",
                    "MOD名"
                }
            },
            {
                "Quick.ModName.tip",
                new[]
                {
                    "산출 파일 이름(.warudo 앞부분)입니다. 한 번 빌드하면 이 오브젝트의 이름으로 기억됩니다.",
                    "The output file name (before .warudo). It is remembered for this object after the first build.",
                    "出力ファイル名(.warudo の前)です。一度ビルドするとこのオブジェクトの名前として記憶されます。"
                }
            },
            {
                "Quick.NameEmpty",
                new[]
                {
                    "모드 이름을 입력하세요.",
                    "Enter a mod name.",
                    "MOD名を入力してください。"
                }
            },
            {
                "Quick.NameDuplicate",
                new[]
                {
                    "다른 항목과 이름이 같습니다. 이름을 바꾸거나 '번호 붙이기'를 켜 주세요.",
                    "Same name as another item. Rename it or turn on numbering.",
                    "他の項目と名前が同じです。名前を変えるか「番号を付ける」をオンにしてください。"
                }
            },
            {
                "Quick.NameFromHistory",
                new[]
                {
                    "이력의 이름 그대로",
                    "Name from history",
                    "履歴の名前のまま"
                }
            },
            {
                "Quick.PrependScene",
                new[]
                {
                    "이름 앞에 씬 이름 붙이기 ({0})",
                    "Prefix the scene name ({0})",
                    "名前の前にシーン名を付ける ({0})"
                }
            },
            {
                "Quick.PrependScene.tip",
                new[]
                {
                    "오브젝트가 있는 씬의 이름을 앞에 붙입니다. 이름이 이미 씬 이름으로 시작하면 다시 붙이지 않습니다. 기억되는 이름은 씬 이름을 뺀 기본 이름입니다.",
                    "Adds the name of the scene the object is in. Skipped when the name already starts with it. The remembered name stays the base name without the scene name.",
                    "オブジェクトがあるシーンの名前を前に付けます。名前がすでにシーン名で始まる場合は付けません。記憶される名前はシーン名なしの基本名です。"
                }
            },
            {
                "Quick.SceneNameGeneric",
                new[]
                {
                    "씬이름",
                    "Scene",
                    "シーン名"
                }
            },
            {
                "Quick.SceneNameUnavailable",
                new[]
                {
                    "저장 안 된 씬이라 씬 이름을 붙이지 않습니다",
                    "Scene is not saved, so no scene name is added",
                    "未保存のシーンのため、シーン名は付けません"
                }
            },
            {
                "Quick.AppendDate",
                new[]
                {
                    "이름 뒤에 오늘 날짜 붙이기 ({0})",
                    "Append today's date ({0})",
                    "名前の後ろに今日の日付を付ける ({0})"
                }
            },
            {
                "Quick.AppendDate.tip",
                new[]
                {
                    "입력한 이름 뒤에 오늘 날짜를 붙입니다(빌드 탭의 날짜 붙이기와 같은 형식). 기억되는 이름은 날짜를 뺀 기본 이름입니다.",
                    "Appends today's date to the name (same format as the Build tab). The remembered name stays the base name without the date.",
                    "入力した名前の後ろに今日の日付を付けます(ビルドタブと同じ形式)。記憶される名前は日付なしの基本名です。"
                }
            },
            {
                "Quick.AutoNumber",
                new[]
                {
                    "같은 이름이 있으면 번호 붙이기 (_2, _3…)",
                    "Add a number if the name exists (_2, _3…)",
                    "同名があれば番号を付ける (_2, _3…)"
                }
            },
            {
                "Quick.AutoNumber.tip",
                new[]
                {
                    "출력 폴더에 같은 이름 파일이 있거나 이 창의 다른 항목과 이름이 겹치면 _2, _3 … 을 붙여 덮어쓰지 않습니다. 끄면 같은 이름 파일을 덮어씁니다.",
                    "If a file with the name already exists in the output folder, or another item here uses it, _2, _3 … is added instead of overwriting. Off = overwrite.",
                    "出力フォルダに同名ファイルがあるか、この画面の他の項目と重なる場合は _2, _3 … を付けて上書きしません。オフなら上書きします。"
                }
            },
            {
                "Quick.NameSanitized",
                new[]
                {
                    "파일 이름에 쓸 수 없는 문자는 _ 로 바뀝니다: {0}",
                    "Characters not allowed in file names become _: {0}",
                    "ファイル名に使えない文字は _ になります: {0}"
                }
            },
            {
                "Quick.OutputFile",
                new[]
                {
                    "→ {0}",
                    "→ {0}",
                    "→ {0}"
                }
            },
            {
                "Quick.NameRemembered",
                new[]
                {
                    "지난번 이름",
                    "Last used name",
                    "前回の名前"
                }
            },
            {
                "Quick.FileOverwrite",
                new[]
                {
                    "같은 이름 파일을 덮어씁니다",
                    "Overwrites the existing file",
                    "同名ファイルを上書きします"
                }
            },
            {
                "Quick.FileNew",
                new[]
                {
                    "새 파일",
                    "New file",
                    "新規ファイル"
                }
            },
            {
                "Quick.ThumbPending",
                new[]
                {
                    "썸네일 찍는 중…",
                    "Capturing thumbnail…",
                    "サムネイル撮影中…"
                }
            },
            {
                "Quick.ThumbAuto",
                new[]
                {
                    "썸네일: 씬뷰에 안 보여서 정면 자동 구도로 찍었습니다. 씬뷰에 보이게 한 뒤 다시 찍을 수 있습니다.",
                    "Thumbnail: not visible in the Scene view, so it was framed automatically from the front. Bring it into view and recapture.",
                    "サムネイル: シーンビューに映っていないため正面の自動構図で撮りました。シーンビューに映してから撮り直せます。"
                }
            },
            {
                "Quick.ThumbFailed",
                new[]
                {
                    "썸네일을 못 찍었습니다: {0}",
                    "Thumbnail capture failed: {0}",
                    "サムネイルを撮影できませんでした: {0}"
                }
            },
            {
                "Quick.Recapture",
                new[]
                {
                    "썸네일 다시 찍기",
                    "Recapture thumbnails",
                    "サムネイル撮り直し"
                }
            },
            {
                "Quick.Recapture.tip",
                new[]
                {
                    "지금 씬뷰 구도로 다시 찍습니다. 씬뷰에 보이는 대상 부분이 정사각으로 꽉 차게 아이콘이 됩니다(얼굴에 줌인하면 얼굴 아이콘).",
                    "Capture again from the current Scene view. The visible part of the object fills a square icon (zoom in on a face for a face icon).",
                    "今のシーンビューの構図で撮り直します。見えている部分が正方形いっぱいのアイコンになります(顔にズームすると顔アイコン)。"
                }
            },
            {
                "Quick.Inactive",
                new[]
                {
                    "꺼진 오브젝트입니다. 모드에는 켜진 상태로 들어갑니다.",
                    "This object is disabled. It will be enabled in the mod.",
                    "無効なオブジェクトです。MOD には有効な状態で入ります。"
                }
            },
            {
                "Quick.ExternalRefs",
                new[]
                {
                    "이 오브젝트 밖을 가리키는 참조 {0}개는 모드에서 끊깁니다. 예: {1}",
                    "{0} reference(s) to objects outside this hierarchy will be empty in the mod. e.g. {1}",
                    "この階層の外を指す参照 {0} 件は MOD では外れます。例: {1}"
                }
            },
            {
                "Quick.MissingScripts",
                new[]
                {
                    "Missing 스크립트 {0}개는 빼고 굽습니다(씬 원본은 그대로).",
                    "{0} missing script(s) will be left out of the mod (the scene is not changed).",
                    "Missing スクリプト {0} 件は除いてビルドします(シーンはそのまま)。"
                }
            },
            {
                "Quick.InspectTruncated",
                new[]
                {
                    "컴포넌트가 많아 참조 점검을 일부만 했습니다.",
                    "Too many components; the reference check was partial.",
                    "コンポーネントが多いため参照チェックは一部のみです。"
                }
            },
            {
                "Quick.Settings",
                new[]
                {
                    "출력 {0} · 버전 {1} · 제작자 {2}",
                    "Output {0} · Version {1} · Author {2}",
                    "出力 {0} · バージョン {1} · 作者 {2}"
                }
            },
            {
                "Quick.Staging",
                new[]
                {
                    "커스텀 스크립트 폴더 {0}",
                    "Custom script folder {0}",
                    "カスタムスクリプトフォルダ {0}"
                }
            },
            {
                "Quick.OpenSettings",
                new[]
                {
                    "설정 열기",
                    "Settings",
                    "設定を開く"
                }
            },
            {
                "Quick.OpenSettings.tip",
                new[]
                {
                    "버전 · 제작자 · 출력 폴더는 WARUDO Mod Mate 빌드 탭 설정을 씁니다. 그 창을 엽니다.",
                    "Version, author and output folder come from the WARUDO Mod Mate Build tab. Opens that window.",
                    "バージョン・作者・出力フォルダは WARUDO Mod Mate のビルドタブ設定を使います。その画面を開きます。"
                }
            },
            {
                "Quick.Build",
                new[]
                {
                    "빌드",
                    "Build",
                    "ビルド"
                }
            },
            {
                "Quick.BuildMany",
                new[]
                {
                    "빌드 ({0}개)",
                    "Build ({0})",
                    "ビルド({0}件)"
                }
            },
            {
                "Quick.Build.tip",
                new[]
                {
                    "씬 상태 그대로 임시 스냅샷을 만들어 .warudo 로 굽습니다. 끝나면 임시 파일은 지웁니다. (Enter)",
                    "Builds a temporary snapshot of the current scene state into .warudo, then removes the temporary files. (Enter)",
                    "今のシーン状態の一時スナップショットを .warudo にビルドし、終わったら一時ファイルを消します。(Enter)"
                }
            },
            {
                "Quick.EditorBusy",
                new[]
                {
                    "플레이 모드이거나 컴파일 중에는 빌드할 수 없습니다.",
                    "Cannot build during Play Mode or while compiling.",
                    "プレイモード中やコンパイル中はビルドできません。"
                }
            },
            {
                "Quick.FolderFailed",
                new[]
                {
                    "임시 폴더를 만들지 못했습니다.",
                    "Could not create the temporary folder.",
                    "一時フォルダを作成できませんでした。"
                }
            },
            {
                "Quick.Preparing",
                new[]
                {
                    "스냅샷 만드는 중: {0}",
                    "Creating snapshot: {0}",
                    "スナップショット作成中: {0}"
                }
            },
            {
                "Quick.SnapshotFailed",
                new[]
                {
                    "스냅샷을 만들지 못했습니다 ({0}): {1}",
                    "Could not create the snapshot ({0}): {1}",
                    "スナップショットを作成できませんでした({0}): {1}"
                }
            },
            {
                "Quick.StartFailed",
                new[]
                {
                    "빌드를 시작하지 못했습니다.",
                    "Could not start the build.",
                    "ビルドを開始できませんでした。"
                }
            },
            {
                "Quick.Blocked",
                new[]
                {
                    "빌드 전에 해결해야 할 문제가 있습니다.",
                    "Resolve these before building.",
                    "ビルド前に解決が必要な問題があります。"
                }
            },
            {
                "Quick.JobName",
                new[]
                {
                    "씬 오브젝트 빌드",
                    "Scene object build",
                    "シーンオブジェクトのビルド"
                }
            },
            {
                "Quick.DoneOne",
                new[]
                {
                    "{0} 빌드 완료",
                    "Built {0}",
                    "{0} ビルド完了"
                }
            },
            {
                "Quick.DoneMany",
                new[]
                {
                    "{0}개 빌드 완료 ({1})",
                    "Built {0} mods ({1})",
                    "{0} 件ビルド完了 ({1})"
                }
            },
            {
                "Quick.Elapsed",
                new[]
                {
                    "걸린 시간 {0}",
                    "Took {0}",
                    "所要時間 {0}"
                }
            },
            {
                "Quick.ProgressTitle",
                new[]
                {
                    "Warudo 빌드 {0}/{1}",
                    "Warudo Build {0}/{1}",
                    "Warudo ビルド {0}/{1}"
                }
            },
            {
                "Quick.Eta",
                new[]
                {
                    "남은 약 {0}",
                    "about {0} left",
                    "残り約 {0}"
                }
            },
            {
                "Quick.ResultDone",
                new[]
                {
                    "빌드 완료: 성공 {0} · 실패 {1}",
                    "Build finished: {0} succeeded · {1} failed",
                    "ビルド完了: 成功 {0} · 失敗 {1}"
                }
            },
            {
                "Quick.ResultCanceled",
                new[]
                {
                    "빌드 취소: 성공 {0} · 실패 {1}",
                    "Build canceled: {0} succeeded · {1} failed",
                    "ビルド中止: 成功 {0} · 失敗 {1}"
                }
            },
            {
                "Quick.UnsavedScene",
                new[]
                {
                    "(저장 안 된 씬)",
                    "(unsaved scene)",
                    "(未保存のシーン)"
                }
            },
            {
                "Quick.TipSource",
                new[]
                {
                    "씬 오브젝트 {0} › {1}",
                    "Scene object {0} › {1}",
                    "シーンオブジェクト {0} › {1}"
                }
            },
            {
                "Quick.SourceNotLoaded",
                new[]
                {
                    "씬 오브젝트를 찾지 못했습니다. 이 씬을 연 뒤 다시 시도하세요.\n\n{0}",
                    "Could not find the scene object. Open this scene and try again.\n\n{0}",
                    "シーンオブジェクトが見つかりません。このシーンを開いてからもう一度お試しください。\n\n{0}"
                }
            },
            {
                "Quick.RebuildSkipped",
                new[]
                {
                    "씬이 열려 있지 않은 {0}개, 종류가 다른 {1}개는 뺐습니다. 한 번에 한 종류만 빌드합니다.",
                    "Left out {0} whose scene is not open and {1} of another type. One type is built at a time.",
                    "シーンが開いていない {0} 件と種類が違う {1} 件は除きました。一度に一種類だけビルドします。"
                }
            },
        };
    }
}
