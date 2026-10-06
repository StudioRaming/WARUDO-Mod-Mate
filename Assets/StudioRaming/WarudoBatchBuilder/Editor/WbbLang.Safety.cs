using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> SafetyTexts = new Dictionary<string, string[]>
        {
            {
                "Safety.Copying",
                new[]
                {
                    "빌드용 복사본 만드는 중: {0}",
                    "Making a build copy: {0}",
                    "ビルド用コピーを作成中: {0}"
                }
            },
            {
                "Safety.Staging",
                new[]
                {
                    "스크립트 폴더에 복사본 넣는 중: {0}",
                    "Placing the build copy in the script folder: {0}",
                    "スクリプトフォルダにコピーを配置中: {0}"
                }
            },
            {
                "Safety.WritingSettings",
                new[]
                {
                    "빌드 설정 적용 중: {0}",
                    "Applying build settings: {0}",
                    "ビルド設定を適用中: {0}"
                }
            },
            {
                "Safety.Verifying",
                new[]
                {
                    "결과 확인 중: {0}",
                    "Checking the result: {0}",
                    "結果を確認中: {0}"
                }
            },
            {
                "Safety.CleaningUp",
                new[]
                {
                    "복사본 정리 중: {0}",
                    "Removing the build copy: {0}",
                    "コピーを片付け中: {0}"
                }
            },
            {
                "Safety.CopyFailed",
                new[]
                {
                    "빌드용 복사본을 만들지 못했습니다: {0}",
                    "Could not make the build copy: {0}",
                    "ビルド用コピーを作成できませんでした: {0}"
                }
            },
            {
                "Safety.CopyCleanupFailed",
                new[]
                {
                    "임시 복사본을 지우지 못해 남은 항목을 멈췄습니다: {0} (원본이 아닌 복사본입니다. 다음 빌드나 에디터를 다시 열 때 자동으로 정리됩니다)",
                    "Stopped the remaining items because a temporary copy could not be removed: {0} (it is a copy, not your original; it is cleaned up automatically before the next build or when the editor reopens)",
                    "一時コピーを削除できなかったため残りの項目を止めました: {0}（元のファイルではなくコピーです。次のビルドかエディタ再起動時に自動で片付けます）"
                }
            },
            {
                "Safety.BuildFailed",
                new[]
                {
                    "빌드 실패: {0}",
                    "Build failed: {0}",
                    "ビルド失敗: {0}"
                }
            },
            {
                "Safety.BuildFailedNoReason",
                new[]
                {
                    "원인이 기록되지 않았습니다. 로그를 확인하세요",
                    "no reason was reported. Check the log",
                    "原因が記録されていません。ログを確認してください"
                }
            },
            {
                "Safety.BuildSettingsRejected",
                new[]
                {
                    "빌드 설정이 올바르지 않아 빌드가 시작되지 않았습니다(모드 이름 · 스크립트 폴더 · 버전을 확인하세요).",
                    "The build did not start because the build settings were rejected (check the mod name, script folder and version).",
                    "ビルド設定が不正なためビルドが開始されませんでした（MOD名・スクリプトフォルダ・バージョンを確認してください）。"
                }
            },
            {
                "Safety.SilentFailure",
                new[]
                {
                    "빌드는 끝났지만 스크립트 처리 중 오류가 있어 모드가 온전하지 않습니다",
                    "The build finished, but script processing reported errors, so the mod is incomplete",
                    "ビルドは終わりましたが、スクリプト処理でエラーがあったため MOD が不完全です"
                }
            },
            {
                "Safety.ScriptsMissing",
                new[]
                {
                    "스크립트 폴더의 스크립트가 모드에 들어가지 않았습니다(컴파일 실패 가능성). 로그를 확인하세요.",
                    "The scripts from the script folder were not included in the mod (they may have failed to compile). Check the log.",
                    "スクリプトフォルダのスクリプトが MOD に入っていません（コンパイル失敗の可能性）。ログを確認してください。"
                }
            },
            {
                "Safety.Fixup.MissingScripts",
                new[]
                {
                    "누락된 스크립트 {0}개 제거",
                    "Removed {0} missing script(s)",
                    "欠落スクリプト {0} 個を除去"
                }
            },
            {
                "Safety.Fixup.Vrc",
                new[]
                {
                    "VRChat 전용 컴포넌트 {0}개 제외",
                    "Left out {0} VRChat-only component(s)",
                    "VRChat 専用コンポーネント {0} 個を除外"
                }
            },
            {
                "Safety.Fixup.ModularAvatar",
                new[]
                {
                    "Modular Avatar/NDMF 컴포넌트 {0}개 제외",
                    "Left out {0} Modular Avatar/NDMF component(s)",
                    "Modular Avatar/NDMF コンポーネント {0} 個を除外"
                }
            },
            {
                "Safety.Fixup.EditorOnlyComponent",
                new[]
                {
                    "에디터 전용 컴포넌트 {0}개 제외",
                    "Left out {0} editor-only component(s)",
                    "エディタ専用コンポーネント {0} 個を除外"
                }
            },
            {
                "Safety.Fixup.EditorOnlyTag",
                new[]
                {
                    "EditorOnly 태그 오브젝트 {0}개(그대로 둠)",
                    "{0} object(s) tagged EditorOnly (kept as is)",
                    "EditorOnly タグのオブジェクト {0} 個（そのまま）"
                }
            },
            {
                "Safety.Fixup.Kept",
                new[]
                {
                    "다른 컴포넌트가 필요로 해서 {0}개는 남겨 둠",
                    "Kept {0} because other components require them",
                    "他のコンポーネントが必要とするため {0} 個は残しました"
                }
            },
            {
                "Safety.EnvironmentUnsupported",
                new[]
                {
                    "Environment(환경) 모드는 프리팹으로 만들 수 없습니다.",
                    "Environment mods cannot be built from prefabs.",
                    "Environment（環境）MOD はプレハブからは作れません。"
                }
            },
            {
                "Safety.EnvironmentUnsupported.hint",
                new[]
                {
                    "WARUDO 환경은 Environment.unity 씬이 있어야 합니다. 씬은 Warudo 메뉴의 Build Mod 로 만들고, 프리팹은 Prop 이나 Character 로 빌드하세요.",
                    "A WARUDO environment needs an Environment.unity scene. Build scenes with Warudo > Build Mod, and build prefabs as Prop or Character.",
                    "WARUDO の環境には Environment.unity シーンが必要です。シーンは Warudo メニューの Build Mod で、プレハブは Prop か Character としてビルドしてください。"
                }
            },
            {
                "Safety.WarudoNotFound",
                new[]
                {
                    "WARUDO 폴더를 찾지 못했습니다. 출력 폴더를 지정하세요.",
                    "WARUDO was not found. Choose an output folder.",
                    "WARUDO フォルダが見つかりません。出力フォルダを指定してください。"
                }
            },
            {
                "Safety.WarudoNotFound.hint",
                new[]
                {
                    "Steam 에 설치된 WARUDO 를 찾지 못했습니다. 출력 경로에 WARUDO 설치 폴더의 Warudo_Data/StreamingAssets/Props(또는 Characters) 폴더를 지정하세요.",
                    "No Steam install of WARUDO was found. Set the output path to Warudo_Data/StreamingAssets/Props (or Characters) inside your WARUDO install folder.",
                    "Steam にインストールされた WARUDO が見つかりません。出力先に WARUDO のインストールフォルダ内の Warudo_Data/StreamingAssets/Props（または Characters）を指定してください。"
                }
            },
            {
                "Safety.WarudoFolderMissing",
                new[]
                {
                    "출력 경로의 WARUDO 폴더가 없습니다: {0}",
                    "The WARUDO folder in the output path does not exist: {0}",
                    "出力先の WARUDO フォルダが存在しません: {0}"
                }
            },
            {
                "Safety.SettingsBroken",
                new[]
                {
                    "빌드 설정이 비어 있거나 읽을 수 없습니다.",
                    "The build settings are empty or unreadable.",
                    "ビルド設定が空、または読み取れません。"
                }
            },
            {
                "Safety.SettingsBroken.hint",
                new[]
                {
                    "{0} 에 저장된 정상 백업으로 되돌릴 수 있습니다.",
                    "You can restore the working backup saved at {0}.",
                    "{0} に保存された正常なバックアップに戻せます。"
                }
            },
            {
                "Safety.SettingsBroken.hintNoBackup",
                new[]
                {
                    "되돌릴 백업이 없습니다. Warudo 메뉴에서 새 모드를 한 번 만들면 빌드 설정이 다시 생깁니다.",
                    "There is no backup to restore. Create a new mod once from the Warudo menu to recreate the build settings.",
                    "戻せるバックアップがありません。Warudo メニューで新しい MOD を一度作成するとビルド設定が作り直されます。"
                }
            },
            {
                "Safety.UModBusy",
                new[]
                {
                    "다른 모드 빌드가 진행 중입니다.",
                    "Another mod build is in progress.",
                    "別の MOD ビルドが進行中です。"
                }
            },
            {
                "Safety.UModBusy.hint",
                new[]
                {
                    "끝난 뒤 다시 시도하세요.",
                    "Try again after it finishes.",
                    "終わってから再試行してください。"
                }
            },
            {
                "Safety.UntitledScene",
                new[]
                {
                    "저장하지 않은 새 씬에 변경이 있습니다: {0}",
                    "A new scene has unsaved changes: {0}",
                    "未保存の新しいシーンに変更があります: {0}"
                }
            },
            {
                "Safety.UntitledScene.hint",
                new[]
                {
                    "빌드 중에 씬이 다시 열리면서 이 내용이 사라집니다. 씬을 한 번 저장(Ctrl+S)한 뒤 다시 시도하세요. 저장된 씬은 빌드를 시작할 때 자동으로 저장됩니다.",
                    "The scene is reopened during the build and these changes would be lost. Save the scene once (Ctrl+S) and try again. Saved scenes are saved automatically when a build starts.",
                    "ビルド中にシーンが開き直され、この内容は失われます。シーンを一度保存（Ctrl+S）してから再試行してください。保存済みのシーンはビルド開始時に自動保存されます。"
                }
            },
            {
                "Safety.PrefabStageDirty",
                new[]
                {
                    "편집 중인 프리팹에 저장하지 않은 변경이 있습니다: {0}",
                    "The prefab you are editing has unsaved changes: {0}",
                    "編集中のプレハブに未保存の変更があります: {0}"
                }
            },
            {
                "Safety.PrefabStageDirty.hint",
                new[]
                {
                    "저장하거나 프리팹 편집을 닫은 뒤 빌드하세요. 빌드는 저장된 내용으로 만들어집니다.",
                    "Save it or close prefab editing before building. Builds use the saved content.",
                    "保存するかプレハブ編集を閉じてからビルドしてください。ビルドは保存された内容で作られます。"
                }
            },
            {
                "Safety.JournalPending",
                new[]
                {
                    "지난 빌드가 도중에 끊겨 정리할 것이 남아 있습니다.",
                    "The last build was interrupted and left things to clean up.",
                    "前回のビルドが途中で止まり、片付けが残っています。"
                }
            },
            {
                "Safety.JournalPending.hint",
                new[]
                {
                    "빌드를 시작하면 먼저 자동으로 정리합니다(임시 복사본 삭제 · 빌드 설정 복원). 원본은 건드리지 않습니다.",
                    "Starting a build cleans it up first (removes temporary copies and restores the build settings). Your originals are not touched.",
                    "ビルドを開始すると先に自動で片付けます（一時コピーの削除・ビルド設定の復元）。元のファイルには触れません。"
                }
            },
            {
                "Safety.DatabaseLoadFailed",
                new[]
                {
                    "빌드 이력/프리셋 파일을 읽지 못해 이번 결과가 저장되지 않을 수 있습니다.",
                    "The build history/preset file could not be read, so this result may not be saved.",
                    "ビルド履歴/プリセットのファイルを読めなかったため、今回の結果は保存されない可能性があります。"
                }
            },
            {
                "Safety.DatabaseLoadFailed.hint",
                new[]
                {
                    "빌드 이력 탭에서 '직전 백업으로 되돌리기' 또는 '빈 상태로 계속'을 고르세요. 읽지 못한 파일은 따로 보관해 두었습니다.",
                    "In the History tab choose 'Restore the previous save' or 'Continue with an empty list'. The unreadable file has been kept aside.",
                    "ビルド履歴タブで「直前のバックアップに戻す」か「空の状態で続ける」を選んでください。読めなかったファイルは別に保管してあります。"
                }
            },
            {
                "Safety.VersionInvalid",
                new[]
                {
                    "버전 형식이 올바르지 않습니다: {0}",
                    "The version is not valid: {0}",
                    "バージョンの形式が正しくありません: {0}"
                }
            },
            {
                "Safety.VersionInvalid.hint",
                new[]
                {
                    "숫자 세 개를 점으로 이어 쓰세요(예: 1.0.0).",
                    "Use three numbers separated by dots (e.g. 1.0.0).",
                    "数字 3 つをドットでつないでください（例: 1.0.0）。"
                }
            },
            {
                "Safety.VersionAutoFixed",
                new[]
                {
                    "버전을 {0} 으로 채워서 빌드합니다.",
                    "The version will be built as {0}.",
                    "バージョンを {0} に補ってビルドします。"
                }
            },
            {
                "Safety.AsmdefScripts",
                new[]
                {
                    "스크립트 폴더의 스크립트 {0}개가 어셈블리 정의(.asmdef)에 속해 있습니다.",
                    "{0} script(s) in the script folder belong to an assembly definition (.asmdef).",
                    "スクリプトフォルダのスクリプト {0} 個がアセンブリ定義（.asmdef）に属しています。"
                }
            },
            {
                "Safety.AsmdefScripts.hint",
                new[]
                {
                    "이런 스크립트는 모드에 들어가지 않아 스크립트 없는 모드가 만들어집니다. .asmdef 가 없는 폴더로 옮기세요. 예: {0}",
                    "Such scripts are not packed, so the mod would be built without them. Move them to a folder without an .asmdef. e.g. {0}",
                    "このようなスクリプトは MOD に入らず、スクリプトのない MOD ができます。.asmdef のないフォルダへ移動してください。例: {0}"
                }
            },
            {
                "Safety.DllInScriptFolder",
                new[]
                {
                    "스크립트 폴더에 DLL 이 {0}개 있습니다.",
                    "The script folder contains {0} DLL file(s).",
                    "スクリプトフォルダに DLL が {0} 個あります。"
                }
            },
            {
                "Safety.DllInScriptFolder.hint",
                new[]
                {
                    "모드에는 DLL 을 넣을 수 없어 빌드가 실패합니다. 스크립트 폴더 밖으로 옮기세요. 예: {0}",
                    "Mods cannot contain DLLs, so the build fails. Move them out of the script folder. e.g. {0}",
                    "MOD に DLL は入れられず、ビルドが失敗します。スクリプトフォルダの外へ移動してください。例: {0}"
                }
            },
            {
                "Safety.UnsupportedFiles",
                new[]
                {
                    "스크립트 폴더에 모드에 넣을 수 없는 파일이 {0}개 있습니다.",
                    "The script folder has {0} file(s) that cannot go into a mod.",
                    "スクリプトフォルダに MOD に入れられないファイルが {0} 個あります。"
                }
            },
            {
                "Safety.UnsupportedFiles.hint",
                new[]
                {
                    "빌드에서 빠지거나 경고가 납니다. 필요 없으면 다른 폴더로 옮기세요. 예: {0}",
                    "They are skipped or cause warnings during the build. Move them elsewhere if not needed. e.g. {0}",
                    "ビルドで除外されるか警告が出ます。不要なら別のフォルダへ移動してください。例: {0}"
                }
            },
            {
                "Safety.SameFolder.hint",
                new[]
                {
                    "스크립트 폴더 안의 프리팹은 모든 모드에 함께 들어갑니다. 프리팹은 다른 폴더에 두세요.",
                    "Every prefab inside the script folder is packed into every mod. Keep prefabs in a different folder.",
                    "スクリプトフォルダ内のプレハブはすべての MOD に一緒に入ります。プレハブは別のフォルダに置いてください。"
                }
            },
            {
                "Safety.LeftoverPrefab.hint",
                new[]
                {
                    "빌드는 이 이름으로 임시 복사본을 넣습니다. 이 파일을 다른 폴더로 옮기거나 이름을 바꾼 뒤 다시 시도하세요.",
                    "The build places its temporary copy under this name. Move or rename this file and try again.",
                    "ビルドはこの名前で一時コピーを置きます。このファイルを別のフォルダへ移すか名前を変えてから再試行してください。"
                }
            },
            {
                "Safety.PrefabsInBuildFolder.hint",
                new[]
                {
                    "스크립트 폴더의 프리팹은 모든 모드에 함께 들어가 용량이 커집니다. 필요 없으면 다른 폴더로 옮기세요.",
                    "Prefabs in the script folder are packed into every mod and make them larger. Move them elsewhere if not needed.",
                    "スクリプトフォルダのプレハブはすべての MOD に入り容量が増えます。不要なら別のフォルダへ移動してください。"
                }
            },
            {
                "Safety.JournalWriteFailed",
                new[]
                {
                    "빌드 복구 기록을 쓰지 못해 시작하지 않았습니다. 프로젝트의 Library 폴더 쓰기 권한을 확인하세요.",
                    "The build did not start because its recovery record could not be written. Check write access to the project's Library folder.",
                    "復旧記録を書き込めなかったためビルドを開始しませんでした。プロジェクトの Library フォルダの書き込み権限を確認してください。"
                }
            },
            {
                "Safety.SceneSaveFailed",
                new[]
                {
                    "씬을 자동 저장하지 못해 시작하지 않았습니다: {0}",
                    "The build did not start because the scene could not be auto-saved: {0}",
                    "シーンを自動保存できなかったためビルドを開始しませんでした: {0}"
                }
            },
            {
                "Safety.Recovered",
                new[]
                {
                    "지난 빌드가 도중에 끊겨 자동으로 정리했습니다(임시 복사본 {0}개 삭제, 빌드 설정 복원).",
                    "The last build was interrupted; it was cleaned up automatically ({0} temporary copy(ies) removed, build settings restored).",
                    "前回のビルドが途中で止まったため自動で片付けました（一時コピー {0} 個を削除、ビルド設定を復元）。"
                }
            },
            {
                "Safety.RecoveredWithProblems",
                new[]
                {
                    "지난 빌드를 정리했지만 {1}건은 직접 확인이 필요합니다(임시 복사본 {0}개 삭제). 예: {2}",
                    "The last build was cleaned up, but {1} item(s) need a manual check ({0} temporary copy(ies) removed). e.g. {2}",
                    "前回のビルドを片付けましたが、{1} 件は手動確認が必要です（一時コピー {0} 個を削除）。例: {2}"
                }
            },
            {
                "Safety.RecoveredOutputs",
                new[]
                {
                    "끝나지 않은 빌드의 출력 파일 {0}개를 빌드 전 상태로 되돌렸습니다(새로 만들어진 파일은 {1} 에 보관).",
                    "{0} output file(s) from the unfinished build were put back to how they were before the build (newly written files are kept at {1}).",
                    "終わらなかったビルドの出力ファイル {0} 個をビルド前の状態に戻しました（新しく作られたファイルは {1} に保管）。"
                }
            },
            {
                "Safety.RecoverSettingsFailed",
                new[]
                {
                    "빌드 설정 복원 실패: {0}",
                    "Build settings restore failed: {0}",
                    "ビルド設定の復元に失敗: {0}"
                }
            },
            {
                "Safety.JournalUnreadable",
                new[]
                {
                    "지난 빌드의 복구 기록을 읽지 못했습니다. 아무것도 지우지 않았습니다. 콘솔을 확인하세요.",
                    "The recovery record of the last build could not be read. Nothing was deleted. Check the Console.",
                    "前回のビルドの復旧記録を読めませんでした。何も削除していません。コンソールを確認してください。"
                }
            },
            {
                "Safety.RestoreSettings",
                new[]
                {
                    "백업으로 되돌리기",
                    "Restore from backup",
                    "バックアップから復元"
                }
            },
            {
                "Safety.RestoreSettings.tip",
                new[]
                {
                    "빌드 설정을 가장 최근의 정상 백업으로 되돌립니다. 지금 파일은 따로 보관합니다.",
                    "Restore the build settings from the newest working backup. The current file is kept aside.",
                    "ビルド設定を最新の正常なバックアップに戻します。現在のファイルは別に保管します。"
                }
            },
            {
                "Safety.SettingsRestored",
                new[]
                {
                    "빌드 설정을 백업으로 되돌렸습니다.",
                    "The build settings were restored from the backup.",
                    "ビルド設定をバックアップから復元しました。"
                }
            },
            {
                "Safety.SettingsRestoreFailed",
                new[]
                {
                    "빌드 설정을 되돌리지 못했습니다: {0}",
                    "Could not restore the build settings: {0}",
                    "ビルド設定を復元できませんでした: {0}"
                }
            },
            {
                "Safety.SettingsRestoreBadFile",
                new[]
                {
                    "이 백업 파일로는 되돌릴 수 없습니다.",
                    "This backup file cannot be used for restoring.",
                    "このバックアップファイルでは復元できません。"
                }
            },
            {
                "Safety.DbLoadFailedBanner",
                new[]
                {
                    "빌드 이력/프리셋 파일을 읽지 못했습니다. 원래 파일은 그대로 두고 저장을 멈췄습니다.",
                    "The build history/preset file could not be read. The file was left untouched and saving is paused.",
                    "ビルド履歴/プリセットのファイルを読めませんでした。元のファイルはそのままにして保存を止めています。"
                }
            },
            {
                "Safety.DbRestoreBackup",
                new[]
                {
                    "직전 백업으로 되돌리기",
                    "Restore the previous save",
                    "直前のバックアップに戻す"
                }
            },
            {
                "Safety.DbRestoreBackup.tip",
                new[]
                {
                    "마지막으로 정상 저장된 파일로 되돌립니다. 그 뒤에 새로 생긴 기록은 합쳐 둡니다.",
                    "Go back to the last good save. Records added since then are merged in.",
                    "最後に正常に保存されたファイルに戻します。その後に追加された記録は統合します。"
                }
            },
            {
                "Safety.DbStartFresh",
                new[]
                {
                    "빈 상태로 계속",
                    "Continue with an empty list",
                    "空の状態で続ける"
                }
            },
            {
                "Safety.DbStartFresh.tip",
                new[]
                {
                    "읽지 못한 파일은 사본으로 보관한 채, 지금부터 새로 저장합니다.",
                    "Keep the unreadable file as a copy and start saving fresh from now on.",
                    "読めなかったファイルはコピーとして保管し、今から新しく保存します。"
                }
            },
            {
                "Safety.StripVrc",
                new[]
                {
                    "VRChat 전용 컴포넌트 빼고 굽기",
                    "Leave out VRChat-only components",
                    "VRChat 専用コンポーネントを除外してビルド"
                }
            },
            {
                "Safety.StripVrc.tip",
                new[]
                {
                    "빌드용 복사본에서 PhysBone · Contact · Constraint · Avatar Descriptor 같은 VRChat 전용 컴포넌트를 뺍니다. 원본은 그대로입니다. 남겨 두면 WARUDO 가 PhysBone 을 자동으로 흔들림으로 바꿔 MagicaCloth/SpringBone 과 같은 뼈를 이중으로 흔들 수 있습니다.",
                    "Leaves VRChat-only components (PhysBone, Contact, Constraint, Avatar Descriptor, ...) out of the build copy. Your original is untouched. If kept, WARUDO converts PhysBones to its own jiggle and the same bones may be simulated twice alongside MagicaCloth/SpringBone.",
                    "ビルド用コピーから PhysBone・Contact・Constraint・Avatar Descriptor などの VRChat 専用コンポーネントを除外します。元のファイルはそのままです。残すと WARUDO が PhysBone を自動で揺れ物に変換し、MagicaCloth/SpringBone と同じボーンを二重に揺らすことがあります。"
                }
            },
            {
                "Safety.FixupHeader",
                new[]
                {
                    "빌드용 복사본 자동 정리",
                    "Tidied in the build copy",
                    "ビルド用コピーの自動整理"
                }
            },
            {
                "Safety.ViewLog",
                new[]
                {
                    "로그 보기",
                    "View log",
                    "ログを見る"
                }
            },
            {
                "Safety.ViewLog.tip",
                new[]
                {
                    "이 항목을 빌드할 때의 상세 로그를 메모장으로 엽니다. 로그 파일이 없으면 흐리게 표시됩니다.",
                    "Open the detailed log from this build in Notepad. Grayed out when the log file is missing.",
                    "この項目をビルドしたときの詳細ログをメモ帳で開きます。ログファイルがない場合はグレー表示になります。"
                }
            },
            {
                "Safety.ViewLogReveal",
                new[]
                {
                    "로그를 탐색기에서 보기",
                    "Show log in Explorer",
                    "ログをエクスプローラーで表示"
                }
            },
            {
                "Safety.ViewLogReveal.tip",
                new[]
                {
                    "로그 파일이 있는 폴더를 탐색기로 열고 그 파일을 선택합니다.",
                    "Open the log file's folder in Explorer with the file selected.",
                    "ログファイルのあるフォルダをエクスプローラーで開き、そのファイルを選択します。"
                }
            },
            {
                "Safety.PackageRisk",
                new[]
                {
                    "재배포 주의",
                    "Redistribution risk",
                    "再配布注意"
                }
            },
            {
                "Safety.PackageRisk.tip",
                new[]
                {
                    "스크립트 폴더 밖의 코드 · 셰이더 파일입니다. 다른 사람이 만든 것일 수 있어 기본으로 뺐습니다. 직접 만든 파일이면 체크해서 넣으세요.",
                    "A code or shader file outside your script folder. It may belong to someone else, so it is left out by default. Tick it if you made it yourself.",
                    "スクリプトフォルダ外のコード・シェーダーファイルです。他者が作ったものの可能性があるため既定で除外しました。自作ならチェックして含めてください。"
                }
            },
            {
                "Safety.PackageCustomScript",
                new[]
                {
                    "내 스크립트",
                    "My script",
                    "自作スクリプト"
                }
            },
            {
                "Safety.PackageCustomScript.tip",
                new[]
                {
                    "스크립트 폴더 안의 파일이라 기본으로 넣습니다.",
                    "Inside your script folder, so it is included by default.",
                    "スクリプトフォルダ内のファイルなので既定で含めます。"
                }
            },
            {
                "Safety.StrandedOriginals",
                new[]
                {
                    "예전 버전 빌드가 스크립트 폴더에 남긴 프리팹이 {0}개 있습니다.",
                    "{0} prefab(s) were left in a script folder by an older version's build.",
                    "旧バージョンのビルドがスクリプトフォルダに残したプレハブが {0} 個あります。"
                }
            },
            {
                "Safety.StrandedOriginals.hint",
                new[]
                {
                    "자동으로 옮기지 않습니다. 원래 폴더로 직접 옮겨 주세요. 예: {0} → {1}",
                    "They are not moved automatically. Move them back to their original folder. e.g. {0} → {1}",
                    "自動では移動しません。元のフォルダへ手動で戻してください。例: {0} → {1}"
                }
            },
            {
                "Safety.DeletedToRecycleBin",
                new[]
                {
                    "휴지통으로 보냈습니다.",
                    "Moved to the Recycle Bin.",
                    "ごみ箱に移動しました。"
                }
            },
            {
                "Safety.DeletedToTrashFolder",
                new[]
                {
                    "휴지통 대신 _WBB_Trash 폴더로 옮겼습니다.",
                    "Moved to the _WBB_Trash folder instead of the Recycle Bin.",
                    "ごみ箱の代わりに _WBB_Trash フォルダへ移動しました。"
                }
            },
            {
                "Safety.DeleteNotFound",
                new[]
                {
                    "파일이 이미 없습니다.",
                    "The file no longer exists.",
                    "ファイルが既にありません。"
                }
            },
            {
                "Safety.DeleteReadOnly",
                new[]
                {
                    "읽기 전용 파일이라 지우지 않았습니다. 파일 속성을 확인하세요.",
                    "Not removed: the file is read-only. Check its properties.",
                    "読み取り専用のため削除しませんでした。ファイルのプロパティを確認してください。"
                }
            },
            {
                "Safety.DeleteInUse",
                new[]
                {
                    "다른 프로그램(WARUDO 등)이 사용 중이라 지우지 못했습니다.",
                    "Could not remove it because another program (such as WARUDO) is using it.",
                    "他のプログラム（WARUDO など）が使用中のため削除できませんでした。"
                }
            },
            {
                "Safety.DeleteAccessDenied",
                new[]
                {
                    "권한이 없어 지우지 못했습니다.",
                    "Could not remove it: access denied.",
                    "権限がないため削除できませんでした。"
                }
            },
            {
                "Safety.DeleteFailed",
                new[]
                {
                    "지우지 못했습니다",
                    "Could not remove the file",
                    "削除できませんでした"
                }
            },
        };
    }
}
