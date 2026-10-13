using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> AsmdefTexts = new Dictionary<string, string[]>
        {
            {
                "Asmdef.NotCompiled",
                new[]
                {
                    "Unity가 플레이어용으로 컴파일하지 않는 어셈블리 정의 {0}개는 모드에 넣지 않습니다.",
                    "{0} assembly definition(s) that Unity does not compile for the player stay out of the mod.",
                    "Unity がプレイヤー用にコンパイルしないアセンブリ定義 {0} 個は MOD に入れません。"
                }
            },
            {
                "Asmdef.NotCompiled.hint",
                new[]
                {
                    "에디터 전용, 테스트, 플랫폼·정의 조건으로 빠진 어셈블리입니다. 모드에 필요하다면 Platforms 와 Define Constraints 를 확인하세요. 예: {0}",
                    "These are editor-only, test or platform/define-filtered assemblies. If the mod needs one, check its Platforms and Define Constraints. e.g. {0}",
                    "エディター専用、テスト、プラットフォームや定義条件で除外されたアセンブリです。MOD に必要な場合は Platforms と Define Constraints を確認してください。例: {0}"
                }
            },
            {
                "Asmdef.ReferenceMissing",
                new[]
                {
                    "어셈블리 정의 '{0}' 가 '{1}' 을(를) 참조하지만, WARUDO 가 제공하지 않고 이 모드에도 들어 있지 않습니다.",
                    "The assembly definition '{0}' references '{1}', which WARUDO does not provide and which is not part of this mod.",
                    "アセンブリ定義 '{0}' は '{1}' を参照していますが、WARUDO は提供しておらず、この MOD にも含まれていません。"
                }
            },
            {
                "Asmdef.ReferenceMissing.hint",
                new[]
                {
                    "'{1}' 의 어셈블리 정의를 스크립트 폴더로 옮기거나 참조를 지우세요.",
                    "Move the assembly definition of '{1}' into the script folder, or remove the reference.",
                    "'{1}' のアセンブリ定義をスクリプトフォルダへ移動するか、参照を削除してください。"
                }
            },
            {
                "Asmdef.PrecompiledMissing",
                new[]
                {
                    "어셈블리 정의 '{0}' 가 미리 컴파일된 DLL '{1}' 을(를) 쓰지만, WARUDO 에는 이 DLL 이 없습니다.",
                    "The assembly definition '{0}' uses the precompiled DLL '{1}', which WARUDO does not provide.",
                    "アセンブリ定義 '{0}' は事前コンパイル済み DLL '{1}' を使っていますが、WARUDO にはこの DLL がありません。"
                }
            },
            {
                "Asmdef.PrecompiledMissing.hint",
                new[]
                {
                    "모드에는 DLL 을 넣을 수 없습니다. 소스 코드로 바꾸거나 참조를 지우세요.",
                    "A mod cannot ship DLLs. Replace it with source code or remove the reference.",
                    "MOD に DLL は入れられません。ソースコードに置き換えるか、参照を削除してください。"
                }
            },
            {
                "Asmdef.HostUnknown",
                new[]
                {
                    "WARUDO 를 찾지 못해 어셈블리 정의 {0}개의 참조를 검사하지 못했습니다.",
                    "WARUDO was not found, so the references of {0} assembly definition(s) could not be checked.",
                    "WARUDO が見つからないため、アセンブリ定義 {0} 個の参照を確認できませんでした。"
                }
            },
            {
                "Asmdef.HostUnknown.hint",
                new[]
                {
                    "WARUDO 를 설치하거나 위치를 지정하면 컴파일 뒤에 다시 검사합니다.",
                    "Install WARUDO or set its location, and the references are checked again after compiling.",
                    "WARUDO をインストールするか場所を指定すると、コンパイル後にもう一度確認します。"
                }
            },
            {
                "Asmdef.NameCollidesHost",
                new[]
                {
                    "어셈블리 정의 '{0}' 의 이름이 WARUDO 가 이미 가진 어셈블리와 같습니다.",
                    "The assembly definition '{0}' has the same name as an assembly WARUDO already provides.",
                    "アセンブリ定義 '{0}' の名前が、WARUDO がすでに持つアセンブリと同じです。"
                }
            },
            {
                "Asmdef.NameCollidesHost.hint",
                new[]
                {
                    "이름을 바꾸세요. 모드가 WARUDO 의 어셈블리를 대체하면 안 됩니다.",
                    "Rename it. A mod must not replace an assembly of WARUDO itself.",
                    "名前を変更してください。MOD が WARUDO 自身のアセンブリを置き換えてはいけません。"
                }
            },
            {
                "Asmdef.PlainUnresolved",
                new[]
                {
                    "스크립트 폴더의 일반 스크립트가 어느 Unity 어셈블리에 속하는지 알 수 없습니다.",
                    "Mod Mate could not tell which Unity assembly the plain scripts of the script folder belong to.",
                    "スクリプトフォルダの通常のスクリプトがどの Unity アセンブリに属するか判別できません。"
                }
            },
            {
                "Asmdef.PlainUnresolved.hint",
                new[]
                {
                    "{0} 이(가) 플레이어용 어셈블리에 들어 있지 않습니다. 플랫폼 설정이나 지원되지 않는 폴더에 있는지 확인하세요.",
                    "{0} is not part of a Unity player assembly. Check that it is not excluded by a platform setting or kept in an unsupported folder.",
                    "{0} は Unity のプレイヤー用アセンブリに含まれていません。プラットフォーム設定や非対応フォルダにないか確認してください。"
                }
            },
            {
                "Asmdef.PlainCompileHint",
                new[]
                {
                    "찾지 못한 타입이나 이름이 스크립트 폴더 밖의 프로젝트 스크립트(Assembly-CSharp)에 있다면, 그 스크립트를 폴더 안으로 옮기세요. 모드는 폴더 밖의 프로젝트 스크립트를 쓸 수 없습니다.",
                    "If the missing type or name comes from a project script outside the script folder (Assembly-CSharp), move that script into the folder. A mod cannot use project scripts from outside it.",
                    "見つからない型や名前がスクリプトフォルダ外のプロジェクトスクリプト (Assembly-CSharp) にある場合は、そのスクリプトをフォルダ内へ移動してください。MOD はフォルダ外のプロジェクトスクリプトを使えません。"
                }
            },
            {
                "Asmdef.Compiling",
                new[]
                {
                    "{0}: 런타임 어셈블리를 컴파일하는 중…",
                    "{0}: compiling the runtime assemblies…",
                    "{0}: ランタイムアセンブリをコンパイル中…"
                }
            },
            {
                "Asmdef.Compiled",
                new[]
                {
                    "런타임 어셈블리 {0}개를 직접 컴파일해 모드에 넣었습니다: {1}",
                    "Compiled {0} runtime assembly(ies) and added them to the mod: {1}",
                    "ランタイムアセンブリ {0} 個をコンパイルして MOD に入れました: {1}"
                }
            },
            {
                "Asmdef.CompileFailed",
                new[]
                {
                    "어셈블리 '{0}' 컴파일에 실패했습니다: {1}",
                    "Compiling the assembly '{0}' failed: {1}",
                    "アセンブリ '{0}' のコンパイルに失敗しました: {1}"
                }
            },
            {
                "Asmdef.CompileTimeout",
                new[]
                {
                    "어셈블리 '{0}' 컴파일이 {1}초 안에 끝나지 않았습니다.",
                    "Compiling the assembly '{0}' did not finish within {1} seconds.",
                    "アセンブリ '{0}' のコンパイルが {1} 秒以内に終わりませんでした。"
                }
            },
            {
                "Asmdef.CompileDeclined",
                new[]
                {
                    "Unity 가 '{0}' 컴파일을 시작하지 않았습니다. 다른 컴파일이 끝난 뒤 다시 시도하세요.",
                    "Unity did not start compiling '{0}'. Try again when no other compile is running.",
                    "Unity が '{0}' のコンパイルを開始しませんでした。ほかのコンパイルが終わってからもう一度お試しください。"
                }
            },
            {
                "Asmdef.EditorReference",
                new[]
                {
                    "컴파일된 어셈블리 '{0}' 가 에디터나 프로젝트에만 있는 '{1}' 을(를) 참조합니다.",
                    "The compiled assembly '{0}' references '{1}', which exists only in the Unity Editor or the project.",
                    "コンパイルされたアセンブリ '{0}' が、エディターやプロジェクトにしかない '{1}' を参照しています。"
                }
            },
            {
                "Asmdef.HostMissingRef",
                new[]
                {
                    "컴파일된 어셈블리 '{0}' 가 WARUDO 에 없는 '{1}' 을(를) 참조합니다.",
                    "The compiled assembly '{0}' references '{1}', which WARUDO does not provide.",
                    "コンパイルされたアセンブリ '{0}' が、WARUDO にない '{1}' を参照しています。"
                }
            },
            {
                "Asmdef.HostCheckSkipped",
                new[]
                {
                    "WARUDO를 찾지 못해 런타임 참조를 WARUDO와 대조하지 못했습니다: {0}",
                    "WARUDO was not found, so the runtime references were not checked against WARUDO: {0}",
                    "WARUDO が見つからなかったため、ランタイムの参照を WARUDO と照合できませんでした: {0}"
                }
            },
            {
                "Asmdef.ScriptsNotAllowed",
                new[]
                {
                    "설치된 UMod 설정이 모드 안의 스크립트를 허용하지 않습니다.",
                    "The installed UMod settings do not allow scripts in mods.",
                    "インストールされている UMod の設定は、MOD 内のスクリプトを許可していません。"
                }
            },
            {
                "Asmdef.RegisterFailed",
                new[]
                {
                    "UMod 이 컴파일된 어셈블리 '{0}' 을(를) 받아들이지 않았습니다.",
                    "UMod rejected the compiled assembly '{0}'.",
                    "UMod がコンパイルされたアセンブリ '{0}' を受け付けませんでした。"
                }
            },
            {
                "Asmdef.HookUnavailable",
                new[]
                {
                    "이 UMod 버전에는 Mod Mate 가 어셈블리 정의를 넣는 데 필요한 연결 지점이 없습니다: {0}",
                    "This UMod version does not expose the build hook that Mod Mate needs for assembly definitions: {0}",
                    "この UMod バージョンには、Mod Mate がアセンブリ定義を入れるために必要な接続点がありません: {0}"
                }
            },
            {
                "Asmdef.ModuleMissing",
                new[]
                {
                    "컴파일된 어셈블리가 모드 파일에 들어가지 않았습니다. 모드가 스크립트 없이 만들어졌을 수 있습니다.",
                    "The compiled assemblies were not packed into the mod file. The mod may have been built without scripts.",
                    "コンパイルしたアセンブリが MOD ファイルに入っていません。スクリプトなしの MOD になっている可能性があります。"
                }
            },
            {
                "Win.TabError",
                new[]
                {
                    "이 탭에서 오류가 나서 멈췄습니다: {0} (자세한 내용은 콘솔에 있습니다)",
                    "This tab stopped with an error: {0} (details are in the Console)",
                    "このタブでエラーが発生して停止しました: {0}（詳細はコンソールにあります）"
                }
            },
            {
                "Win.TabRetry",
                new[]
                {
                    "다시 시도",
                    "Retry",
                    "再試行"
                }
            },
            {
                "Safety.CreateSettings",
                new[]
                {
                    "빌드 설정 만들기",
                    "Create settings",
                    "ビルド設定を作成"
                }
            },
            {
                "Safety.SettingsCreated",
                new[]
                {
                    "빌드 설정을 만들었습니다: {0}",
                    "Build settings created: {0}",
                    "ビルド設定を作成しました: {0}"
                }
            },
            {
                "Safety.SettingsCreateFailed",
                new[]
                {
                    "빌드 설정을 만들지 못했습니다: {0}",
                    "Could not create the build settings: {0}",
                    "ビルド設定を作成できませんでした: {0}"
                }
            },
            {
                "Runner.NoProfiles.hint",
                new[]
                {
                    "'빌드 설정 만들기' 를 누르면 SDK 의 Export Settings 창과 같은 호출로 모드 한 개를 등록합니다.",
                    "Press 'Create settings' to register one mod with the same call the SDK's Export Settings window uses.",
                    "「ビルド設定を作成」を押すと、SDK の Export Settings ウィンドウと同じ呼び出しで MOD を 1 つ登録します。"
                }
            },
            {
                "Safety.SecurityHeader",
                new[]
                {
                    "코드 보안 검사가 모드를 거부했습니다. 항목 {0}개:",
                    "The code security check rejected the mod. {0} entries:",
                    "コードセキュリティチェックが MOD を拒否しました。項目 {0} 件:"
                }
            },
            {
                "Safety.SecurityMore",
                new[]
                {
                    "…외 {0}개 (전체 목록은 빌드 로그에 있습니다)",
                    "…and {0} more (the full list is in the build log)",
                    "…ほか {0} 件（全リストはビルドログにあります）"
                }
            },
            {
                "Runner.AbortReload",
                new[]
                {
                    "빌드 도중 Unity 가 스크립트를 다시 컴파일했습니다.",
                    "Unity recompiled scripts while the build was running.",
                    "ビルド中に Unity がスクリプトを再コンパイルしました。"
                }
            },
            {
                "Runner.AbortPlayMode",
                new[]
                {
                    "빌드 도중 Unity 가 플레이 모드에 들어갔습니다.",
                    "Unity entered Play Mode while the build was running.",
                    "ビルド中に Unity がプレイモードに入りました。"
                }
            },
            {
                "Runner.AbortedItem",
                new[]
                {
                    "{0} 이 항목은 되돌렸습니다. 다시 빌드하세요.",
                    "{0} This item was rolled back. Build it again.",
                    "{0} この項目は元に戻しました。もう一度ビルドしてください。"
                }
            },
            {
                "Runner.AbortNotice",
                new[]
                {
                    "빌드가 중단되었습니다: {0} 설정 복원: {1}",
                    "The build was stopped: {0} Settings restore: {1}",
                    "ビルドが中断されました: {0} 設定の復元: {1}"
                }
            },
            {
                "Runner.AbortRestored",
                new[]
                {
                    "완료.",
                    "done.",
                    "完了。"
                }
            },
            {
                "Runner.AbortRestorePending",
                new[]
                {
                    "일부가 남았고 Unity 로딩이 끝나면 다시 시도합니다.",
                    "partly pending; it is retried when Unity finishes loading.",
                    "一部が未完了で、Unity の読み込み完了後に再試行します。"
                }
            }
        };
    }
}
