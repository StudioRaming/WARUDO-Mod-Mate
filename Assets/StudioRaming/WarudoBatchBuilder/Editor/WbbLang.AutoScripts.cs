using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> AutoScriptTexts = new Dictionary<string, string[]>
        {
            {
                "Build.AutoScripts",
                new[]
                {
                    "런타임 스크립트 자동 수집",
                    "Automatically collect runtime scripts",
                    "ランタイムスクリプトを自動収集"
                }
            },
            {
                "Build.AutoScripts.tip",
                new[]
                {
                    "프리팹과 하위 오브젝트에 필요한 소스를 자동으로 포함합니다. WARUDO가 이미 제공하는 타입과 에디터 전용 코드는 제외합니다.",
                    "Automatically includes the source files required by the prefab and its child objects. Excludes types already provided by WARUDO and editor-only code.",
                    "プレハブと子オブジェクトに必要なソースを自動的に含めます。WARUDO がすでに提供している型とエディター専用コードは除外します。"
                }
            },
            {
                "Build.ManualScripts",
                new[]
                {
                    "스크립트 폴더 직접 지정",
                    "Specify the script folder manually",
                    "スクリプトフォルダーを手動で指定"
                }
            },
            {
                "Build.ManualScripts.tip",
                new[]
                {
                    "자동 수집 대신 기존 스크립트 폴더 방식으로 빌드합니다.",
                    "Builds using the existing script-folder workflow instead of automatic collection.",
                    "自動収集の代わりに、従来のスクリプトフォルダー方式でビルドします。"
                }
            },
            {
                "Safety.AutoScriptsFailed",
                new[]
                {
                    "런타임 스크립트를 자동 수집하지 못했습니다: {0}",
                    "Failed to automatically collect runtime scripts: {0}",
                    "ランタイムスクリプトを自動収集できませんでした: {0}"
                }
            },
            {
                "Runner.AutoScripts",
                new[]
                {
                    "런타임 소스 {0}개 포함 · WARUDO 제공 타입 {1}개 제외.",
                    "Included {0} runtime sources · Excluded {1} types provided by WARUDO.",
                    "ランタイムソースを {0} 個含めました · WARUDO 提供の型を {1} 個除外しました。"
                }
            }
        };
    }
}
