using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> RunnerTexts = new Dictionary<string, string[]>
        {
            {
                "Runner.PreparingBatch",
                new[]
                {
                    "빌드 준비 중...",
                    "Preparing build...",
                    "ビルド準備中..."
                }
            },
            {
                "Runner.OtherBuildRunning",
                new[]
                {
                    "다른 Warudo 빌드가 진행 중입니다. 끝난 뒤 다시 시도하세요.",
                    "Another Warudo build is in progress. Try again after it finishes.",
                    "別の Warudo ビルドが進行中です。終わってからもう一度お試しください。"
                }
            },
            {
                "Runner.Preparing",
                new[]
                {
                    "준비 중: {0}",
                    "Preparing: {0}",
                    "準備中: {0}"
                }
            },
            {
                "Runner.Building",
                new[]
                {
                    "빌드 실행 중: {0}",
                    "Building: {0}",
                    "ビルド実行中: {0}"
                }
            },
            {
                "Runner.WaitingOutput",
                new[]
                {
                    "산출물 생성 대기 중: {0} ({1})",
                    "Waiting for output: {0} ({1})",
                    "出力ファイル待機中: {0} ({1})"
                }
            },
            {
                "Runner.Done",
                new[]
                {
                    "빌드 완료: 성공 {0} · 실패 {1}",
                    "Build finished: {0} succeeded, {1} failed",
                    "ビルド完了: 成功 {0}・失敗 {1}"
                }
            },
            {
                "Runner.Canceled",
                new[]
                {
                    "빌드 중단됨: 성공 {0} · 실패 {1} · 건너뜀 {2}",
                    "Build canceled: {0} succeeded, {1} failed, {2} skipped",
                    "ビルド中断: 成功 {0}・失敗 {1}・スキップ {2}"
                }
            },
            {
                "Runner.CancelRequested",
                new[]
                {
                    "중단 요청됨: 현재 항목을 안전하게 정리하는 중입니다.",
                    "Cancel requested: finishing the current item safely.",
                    "中断が要求されました。現在の項目を安全に片付けています。"
                }
            },
            {
                "Runner.CanceledItem",
                new[]
                {
                    "사용자 요청으로 중단됨",
                    "Canceled by user",
                    "ユーザー操作により中断"
                }
            },
            {
                "Runner.SkippedItem",
                new[]
                {
                    "빌드 중단으로 건너뜀",
                    "Skipped because the batch was canceled",
                    "ビルド中断のためスキップ"
                }
            },
            {
                "Runner.ErrorMove",
                new[]
                {
                    "빌드용 복사본을 옮기지 못했습니다: {0}",
                    "Could not move the build copy: {0}",
                    "ビルド用コピーを移動できませんでした: {0}"
                }
            },
            {
                "Runner.ErrorSettings",
                new[]
                {
                    "모드 빌드 설정에 값을 넣지 못했습니다: {0}",
                    "Could not write the mod's build settings: {0}",
                    "MODのビルド設定に書き込めませんでした: {0}"
                }
            },
            {
                "Runner.ErrorBuild",
                new[]
                {
                    "Warudo Mod Tool(SDK) 빌드를 시작하지 못했습니다: {0}",
                    "Could not start the Warudo Mod Tool (SDK) build: {0}",
                    "Warudo Mod Tool（SDK）のビルドを開始できませんでした: {0}"
                }
            },
            {
                "Runner.ErrorNoOutput",
                new[]
                {
                    "산출 파일이 생성되지 않았습니다: {0}",
                    "Output file was not created: {0}",
                    "出力ファイルが生成されませんでした: {0}"
                }
            },
            {
                "Runner.ErrorNameCollision",
                new[]
                {
                    "같은 이름의 파일이 이미 있습니다: {0}",
                    "A file with the same name already exists: {0}",
                    "同名のファイルが既に存在します: {0}"
                }
            },
            {
                "Runner.ErrorException",
                new[]
                {
                    "처리 중 예외가 발생했습니다: {0}",
                    "An exception occurred: {0}",
                    "処理中に例外が発生しました: {0}"
                }
            },
            {
                "Runner.OutputBackupFailed",
                new[]
                {
                    "출력 폴더의 기존 파일을 백업하지 못해 이 항목은 빌드하지 않았습니다(기존 파일은 그대로입니다): {0}",
                    "This item was not built because the existing file in the output folder could not be backed up (the existing file is unchanged): {0}",
                    "出力フォルダの既存ファイルをバックアップできなかったため、この項目はビルドしませんでした（既存ファイルはそのままです）: {0}"
                }
            },
            {
                "Runner.FailedOutputMoved",
                new[]
                {
                    "실패한 파일은 출력 폴더에서 빼서 {0} 에 두었습니다.",
                    "The file from the failed build was taken out of the output folder and kept at {0}.",
                    "失敗したファイルは出力フォルダから外し、{0} に保管しました。"
                }
            },
            {
                "Runner.FailedOutputRestored",
                new[]
                {
                    "실패한 파일은 {0} 에 두고, 출력 폴더에는 이전 파일을 되돌려 두었습니다.",
                    "The file from the failed build was kept at {0}, and the previous file was put back in the output folder.",
                    "失敗したファイルは {0} に保管し、出力フォルダには以前のファイルを戻しました。"
                }
            },
            {
                "Runner.PreviousOutputRestored",
                new[]
                {
                    "출력 폴더에 이전 파일을 되돌려 두었습니다.",
                    "The previous file was put back in the output folder.",
                    "出力フォルダに以前のファイルを戻しました。"
                }
            },
            {
                "Runner.OutputRestoreFailed",
                new[]
                {
                    "출력 폴더에 이전 파일을 되돌리지 못했습니다. 이전 파일의 백업: {0}",
                    "Could not put the previous file back in the output folder. Backup of the previous file: {0}",
                    "出力フォルダに以前のファイルを戻せませんでした。以前のファイルのバックアップ: {0}"
                }
            },
            {
                "Runner.FailedOutputStuck",
                new[]
                {
                    "실패한 파일을 출력 폴더에서 빼지 못했습니다(다른 프로그램이 쓰는 중일 수 있습니다): {0}",
                    "Could not take the file from the failed build out of the output folder (another program may be using it): {0}",
                    "失敗したファイルを出力フォルダから外せませんでした（別のプログラムが使用中の可能性があります）: {0}"
                }
            },
            {
                "Runner.ProfileRestored",
                new[]
                {
                    "모드 빌드 설정을 원래 값으로 되돌렸습니다.",
                    "Restored the mod's build settings to their original values.",
                    "MODのビルド設定を元の値に戻しました。"
                }
            },
            {
                "Runner.ProfileRestoreFailed",
                new[]
                {
                    "모드 빌드 설정을 원래대로 되돌리지 못했습니다: {0}",
                    "Could not restore the mod's build settings: {0}",
                    "MODのビルド設定を元に戻せませんでした: {0}"
                }
            },
            {
                "Runner.ProfileSnapshotFailed",
                new[]
                {
                    "모드 빌드 설정을 읽지 못해 빌드를 시작하지 않았습니다: {0}",
                    "The build did not start because the mod's build settings could not be read: {0}",
                    "MODのビルド設定を読み取れなかったため、ビルドを開始しませんでした: {0}"
                }
            },
            {
                "Runner.TempFolderNotice",
                new[]
                {
                    "커스텀 스크립트 폴더가 없어 임시 폴더를 만들어 빌드합니다(끝나면 지웁니다).",
                    "No custom script folder is set. A temporary folder will be created for the build and removed afterwards.",
                    "カスタムスクリプトフォルダが未設定のため、一時フォルダを作成してビルドします（終了後に削除します）。"
                }
            },
            {
                "Runner.TempFolderNotice.hint",
                new[]
                {
                    "모드에 함께 넣을 런타임 스크립트(.cs)가 있을 때만 폴더를 지정하면 됩니다. 그 외에는 이대로 두면 됩니다.",
                    "You only need to set a folder when the mod ships runtime scripts (.cs). Otherwise leave it as is.",
                    "MODに同梱するランタイムスクリプト(.cs)がある場合のみフォルダを指定してください。それ以外はこのままで問題ありません。"
                }
            },
            {
                "Runner.TempFolderFailed",
                new[]
                {
                    "임시 빌드 폴더를 만들지 못했습니다. Assets 폴더 쓰기 권한을 확인하세요.",
                    "Could not create a temporary build folder. Check write permission on the Assets folder.",
                    "一時ビルドフォルダを作成できませんでした。Assetsフォルダの書き込み権限を確認してください。"
                }
            },
            {
                "Runner.ProfileIndexChanged",
                new[]
                {
                    "빌드 중에 모드 빌드 설정의 모드 목록이 바뀌었습니다. 다른 모드의 설정을 덮어쓰지 않도록 멈췄습니다.",
                    "The mod list in the mod's build settings changed during the build. Stopped so another mod's settings are not overwritten.",
                    "ビルド中にMODのビルド設定のMOD一覧が変わりました。別のMODの設定を上書きしないよう止めました。"
                }
            },
            {
                "Runner.NoProfiles",
                new[]
                {
                    "모드 빌드 설정에 등록된 모드가 없습니다. Warudo 메뉴의 New Mod 로 새 모드를 한 번 만드세요.",
                    "No mod is registered in the mod's build settings. Create a new mod once with Warudo > New Mod.",
                    "MODのビルド設定に登録されたMODがありません。Warudo メニューの New Mod で新しい MOD を一度作成してください。"
                }
            },
            {
                "Runner.FieldMissing",
                new[]
                {
                    "모드 빌드 설정에서 필요한 항목을 찾지 못했습니다({0}). Warudo Mod Tool(SDK) 버전이 바뀌었을 수 있습니다.",
                    "A required entry was not found in the mod's build settings ({0}). The Warudo Mod Tool (SDK) version may have changed.",
                    "MODのビルド設定に必要な項目が見つかりません（{0}）。Warudo Mod Tool（SDK）のバージョンが変わった可能性があります。"
                }
            },
            {
                "Runner.SettingsLoadFailed",
                new[]
                {
                    "모드 빌드 설정을 불러오지 못했습니다: {0}",
                    "Could not load the mod's build settings: {0}",
                    "MODのビルド設定を読み込めませんでした: {0}"
                }
            },
            {
                "Runner.StartBuildMissing",
                new[]
                {
                    "Warudo Mod Tool(SDK)의 빌드 기능을 찾지 못했습니다.",
                    "The build function of the Warudo Mod Tool (SDK) was not found.",
                    "Warudo Mod Tool（SDK）のビルド機能が見つかりません。"
                }
            },
            {
                "Runner.NoSelection",
                new[]
                {
                    "빌드할 프리팹이 선택되지 않았습니다.",
                    "No prefab is selected for building.",
                    "ビルドするプレハブが選択されていません。"
                }
            },
            {
                "Runner.NoSelection.hint",
                new[]
                {
                    "목록에서 항목을 클릭하거나 '전체 선택'을 눌러 하나 이상 선택하세요.",
                    "Click items in the list or press 'Select All' to pick at least one.",
                    "リストで項目をクリックするか「全選択」で1つ以上選んでください。"
                }
            },
            {
                "Runner.NoBuildFolder",
                new[]
                {
                    "② 빌드 설정을 불러오지 못했습니다.",
                    "Could not load the ② Build settings.",
                    "② ビルド設定を読み込めませんでした。"
                }
            },
            {
                "Runner.NoBuildFolder.hint",
                new[]
                {
                    "창을 닫았다가 다시 열고 ② 빌드 설정을 확인하세요.",
                    "Close and reopen the window, then check the ② Build settings.",
                    "ウィンドウを開き直して、② ビルド設定を確認してください。"
                }
            },
            {
                "Runner.NoPrefabFolder",
                new[]
                {
                    "① 폴더에서 고른 폴더를 찾을 수 없습니다.",
                    "The folder picked in ① Folders no longer exists.",
                    "① フォルダで選んだフォルダが見つかりません。"
                }
            },
            {
                "Runner.NoPrefabFolder.hint",
                new[]
                {
                    "폴더가 지워졌거나 옮겨졌습니다. ① 폴더 트리에서 다른 폴더나 Assets 를 고르세요.",
                    "It was deleted or moved. Pick another folder, or Assets, in the ① Folders tree.",
                    "フォルダが削除または移動されました。① フォルダのツリーで別のフォルダか Assets を選んでください。"
                }
            },
            {
                "Runner.SameFolder",
                new[]
                {
                    "스크립트 폴더와 ① 폴더에서 고른 폴더가 같습니다.",
                    "The script folder is the same as the folder picked in ① Folders.",
                    "スクリプトフォルダと ① フォルダで選んだフォルダが同じです。"
                }
            },
            {
                "Runner.OutputFolderFail",
                new[]
                {
                    "출력 폴더를 만들 수 없습니다: {0}",
                    "Cannot create the output folder: {0}",
                    "出力フォルダを作成できません: {0}"
                }
            },
            {
                "Runner.OutputPathInvalid",
                new[]
                {
                    "출력 경로 형식이 올바르지 않습니다: {0}",
                    "The output path is not a valid form: {0}",
                    "出力パスの形式が正しくありません: {0}"
                }
            },
            {
                "Runner.OutputPathInvalid.hint",
                new[]
                {
                    "드라이브로 시작하는 전체 경로(예: C:/Program Files (x86)/Steam/.../StreamingAssets/Props)나 프로젝트 안 Assets/... 경로를 넣으세요.",
                    "Use a full rooted path (e.g. C:/Program Files (x86)/Steam/.../StreamingAssets/Props) or a project path starting with Assets/.",
                    "ドライブから始まるフルパス（例: C:/Program Files (x86)/Steam/.../StreamingAssets/Props）またはプロジェクト内の Assets/... を指定してください。"
                }
            },
            {
                "Runner.OutputFolderMissing",
                new[]
                {
                    "출력 폴더가 아직 없습니다. 빌드를 시작할 때 만듭니다: {0}",
                    "The output folder does not exist yet. It will be created when the build starts: {0}",
                    "出力フォルダはまだありません。ビルド開始時に作成します: {0}"
                }
            },
            {
                "Runner.OutputFolderMissing.hint",
                new[]
                {
                    "경로가 의도한 곳인지 한 번 확인하세요. Warudo 설치 폴더가 맞는지 특히 확인하는 게 좋습니다.",
                    "Double-check that this is the folder you meant. In particular, check that it is the Warudo install folder.",
                    "意図したフォルダかどうか確認してください。特に Warudo のインストールフォルダであることを確認してください。"
                }
            },
            {
                "Runner.LeftoverPrefab",
                new[]
                {
                    "스크립트 폴더에 빌드가 쓰는 이름의 프리팹이 이미 있습니다: {0}",
                    "The script folder already has a prefab with the name the build uses: {0}",
                    "スクリプトフォルダに、ビルドが使う名前のプレハブが既にあります: {0}"
                }
            },
            {
                "Runner.DuplicateModName",
                new[]
                {
                    "모드명이 중복됩니다: {0}",
                    "Duplicate mod name: {0}",
                    "MOD名が重複しています: {0}"
                }
            },
            {
                "Runner.DuplicateModName.hint",
                new[]
                {
                    "산출 파일이 서로 덮어써집니다. 프리팹 이름이나 접두사를 바꾸세요.",
                    "The outputs would overwrite each other. Change the prefab name or the prefix.",
                    "出力ファイルが互いに上書きされます。プレハブ名か接頭辞を変更してください。"
                }
            },
            {
                "Runner.UnsafeModName",
                new[]
                {
                    "모드명에 파일명으로 쓸 수 없는 문자가 있습니다: {0}",
                    "The mod name contains characters that are invalid in a file name: {0}",
                    "MOD名にファイル名として使えない文字があります: {0}"
                }
            },
            {
                "Runner.UnsafeModName.hint",
                new[]
                {
                    "/ \\ : * ? \" < > | 문자는 쓸 수 없습니다. 프리팹 이름이나 접두사를 고치세요.",
                    "The characters / \\ : * ? \" < > | are not allowed. Fix the prefab name or the prefix.",
                    "/ \\ : * ? \" < > | は使用できません。プレハブ名か接頭辞を修正してください。"
                }
            },
            {
                "Runner.PrefabMissing",
                new[]
                {
                    "프리팹 파일을 찾을 수 없습니다: {0}",
                    "Prefab file not found: {0}",
                    "プレハブファイルが見つかりません: {0}"
                }
            },
            {
                "Runner.PrefabMissing.hint",
                new[]
                {
                    "목록을 다시 스캔하세요. 파일이 이동되었거나 삭제되었습니다.",
                    "Rescan the list. The file was moved or deleted.",
                    "リストを再スキャンしてください。ファイルが移動または削除されています。"
                }
            },
            {
                "Runner.NoUMod",
                new[]
                {
                    "Warudo Mod Tool(SDK)을 찾지 못했습니다.",
                    "The Warudo Mod Tool (SDK) was not found.",
                    "Warudo Mod Tool（SDK）が見つかりません。"
                }
            },
            {
                "Runner.NoUMod.hint",
                new[]
                {
                    "Warudo Mod Tool(SDK)이 이 프로젝트에 설치되어 있는지, 콘솔에 컴파일 오류가 없는지 확인하세요.",
                    "Check that the Warudo Mod Tool (SDK) is installed in this project and the Console shows no compile errors.",
                    "Warudo Mod Tool（SDK）がこのプロジェクトに導入されているか、コンソールにコンパイルエラーが無いか確認してください。"
                }
            },
            {
                "Runner.NoExportSettings",
                new[]
                {
                    "모드 빌드 설정 파일을 찾지 못했습니다.",
                    "The mod's build settings file was not found.",
                    "MODのビルド設定ファイルが見つかりません。"
                }
            },
            {
                "Runner.NoExportSettings.hint",
                new[]
                {
                    "빌드 탭의 '빌드 설정 만들기' 를 누르면 Assets 폴더에 ExportSettings.asset 과 모드 한 개를 SDK 의 방식으로 만듭니다. Warudo 메뉴의 New Mod 로도 만들 수 있습니다.",
                    "Press 'Create settings' in the Build tab: it creates ExportSettings.asset in the Assets folder with one mod, the way the SDK does. Warudo > New Mod works too.",
                    "ビルドタブの「ビルド設定を作成」を押すと、Assets フォルダに ExportSettings.asset と MOD を 1 つ SDK の方法で作成します。Warudo メニューの New Mod でも作れます。"
                }
            },
            {
                "Runner.OutputExists",
                new[]
                {
                    "이미 있는 산출물 {0}개를 덮어씁니다.",
                    "{0} existing output file(s) will be overwritten.",
                    "既存の出力ファイル {0} 件を上書きします。"
                }
            },
            {
                "Runner.OutputExists.hint",
                new[]
                {
                    "예: {0}",
                    "e.g. {0}",
                    "例: {0}"
                }
            },
            {
                "Runner.NoAuthor",
                new[]
                {
                    "제작자가 비어 있습니다.",
                    "The author field is empty.",
                    "作者が空です。"
                }
            },
            {
                "Runner.NoAuthor.hint",
                new[]
                {
                    "빌드 설정 카드의 '제작자'를 채우면 모드 정보에 표시됩니다.",
                    "Fill 'Author' in the build settings card so it shows up in the mod info.",
                    "ビルド設定カードの「作者」を入力すると MOD 情報に表示されます。"
                }
            },
            {
                "Runner.NoIcon",
                new[]
                {
                    "아이콘이 없는 항목이 {0}개 있습니다.",
                    "{0} item(s) have no icon.",
                    "アイコンが無い項目が {0} 件あります。"
                }
            },
            {
                "Runner.NoIcon.hint",
                new[]
                {
                    "툴바의 '썸네일 일괄 생성'으로 만들 수 있습니다. 없어도 빌드는 진행됩니다.",
                    "Use 'Generate thumbnails' in the toolbar. The build still runs without icons.",
                    "ツールバーの「サムネイル一括生成」で作成できます。無くてもビルドは進みます。"
                }
            },
            {
                "Runner.PrefabsInBuildFolder",
                new[]
                {
                    "스크립트 폴더에 프리팹이 {0}개 있습니다.",
                    "The script folder contains {0} prefab(s).",
                    "スクリプトフォルダにプレハブが {0} 件あります。"
                }
            },
        };
    }
}
