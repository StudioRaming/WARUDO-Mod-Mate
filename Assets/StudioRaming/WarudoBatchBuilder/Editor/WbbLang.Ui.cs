using System.Collections.Generic;

namespace StudioRaming.WarudoBatchBuilder
{
    public static partial class WbbLang
    {
        private static readonly Dictionary<string, string[]> UiTexts = new Dictionary<string, string[]>
        {
            {
                "Ui.Help.Manual",
                new[]
                {
                    "사용 가이드 · 화면별 설명",
                    "User guide · workflows",
                    "使い方ガイド・画面の説明"
                }
            },
            {
                "Ui.HeightSplitter.tip",
                new[]
                {
                    "드래그해서 위 프리팹 목록과 아래 영역의 높이를 조절합니다. 두 번 클릭하면 자동 높이로 돌아갑니다.",
                    "Drag to resize the prefab list and the lower area. Double-click to restore automatic height.",
                    "ドラッグで上のプリファブ一覧と下の領域の高さを調整します。ダブルクリックで自動の高さに戻します。"
                }
            },
            {
                "Ui.HeaderSubtitle",
                new[]
                {
                    "v{0} · {1}",
                    "v{0} · {1}",
                    "v{0} · {1}"
                }
            },
            {
                "Ui.HelpMenu.tip",
                new[]
                {
                    "도움말 · 버전 정보 · 문의",
                    "Help · version · support",
                    "ヘルプ · バージョン · お問い合わせ"
                }
            },
            {
                "Ui.Help.Repository",
                new[]
                {
                    "GitHub · 소스 코드",
                    "GitHub · source code",
                    "GitHub · ソースコード"
                }
            },
            {
                "Ui.Help.Downloads",
                new[]
                {
                    "최신 패키지 다운로드",
                    "Download latest package",
                    "最新パッケージをダウンロード"
                }
            },
            {
                "Ui.Help.About",
                new[]
                {
                    "버전 정보",
                    "About",
                    "バージョン情報"
                }
            },
            {
                "Ui.Help.CopyDiagnostics",
                new[]
                {
                    "문의용 환경 정보 복사",
                    "Copy environment info for support",
                    "お問い合わせ用の環境情報をコピー"
                }
            },
            {
                "Ui.Help.CopyEmail",
                new[]
                {
                    "문의 메일 주소 복사",
                    "Copy support e-mail",
                    "お問い合わせメールをコピー"
                }
            },
            {
                "Ui.Help.DiagnosticsCopied",
                new[]
                {
                    "환경 정보를 복사했습니다. 문의할 때 붙여 넣어 주세요.",
                    "Environment info copied. Paste it into your support message.",
                    "環境情報をコピーしました。お問い合わせに貼り付けてください。"
                }
            },
            {
                "Ui.Help.EmailCopied",
                new[]
                {
                    "복사했습니다: {0}",
                    "Copied: {0}",
                    "コピーしました: {0}"
                }
            },
            {
                "Ui.About.Body",
                new[]
                {
                    "버전 {0}\n\nUnity {1} (필요 버전 {2})\nWarudo Mod Tool {3}\n\n문의: {4}\n© StudioRaming",
                    "Version {0}\n\nUnity {1} (required {2})\nWarudo Mod Tool {3}\n\nSupport: {4}\n© StudioRaming",
                    "バージョン {0}\n\nUnity {1}(必要バージョン {2})\nWarudo Mod Tool {3}\n\nお問い合わせ: {4}\n© StudioRaming"
                }
            },
        };
    }
}
